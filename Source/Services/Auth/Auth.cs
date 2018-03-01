using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Redis;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class Auth : ServiceBase, IAuth
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="type">登录类型</param>
        /// <returns>Result</returns>
        public Result<object> GetCode(string account, int type = 0)
        {
            // 限流,每用户每天可访问100次
            var key = Util.Hash("GetCode" + account + type);
            var limited = Params.callManage.IsLimited(key, 3600 * 24, 100);
            if (limited) return result.BadRequest("当天获取Code次数已用完,请合理使用接口");

            userId = Core.GetUserId(account);
            if (userId == null) return result.NotExists();

            token = Core.GetToken(userId);
            if (token == null)
            {
                RedisHelper.Delete($"ID:{account}");
                return result.ServerError("缓存异常，请重试");
            }

            // 生成Code
            object code = Core.GenerateCode(token, account, type);

            return result.Success(code);
        }

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="tid">租户ID</param>
        /// <param name="aid">应用ID</param>
        /// <param name="account">登录账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> GetToken(string tid, string aid, string account, string signature, string deptid)
        {
            // 限流,每用户每3秒可访问1次
            var key = Util.Hash("GetToken" + account);
            var surplus = Params.callManage.GetSurplus(key, 3);
            if (surplus > 0) return result.TooFrequent(surplus);

            var code = Core.GetCode(signature);
            if (string.IsNullOrEmpty(code))
            {
                userId = Core.GetUserId(account);
                token = Core.GetToken(userId);
                if (token != null)
                {
                    token.AddFailureCount();
                    Core.SetTokenCache(token);
                    var msg = $"账号 {account} 正在尝试使用错误的签名请求令牌!";
                    new Thread(() => Logger.Write("400101", msg)).Start();
                }

                return token.UserIsLocked() ? result.Locked() : result.InvalidAuth();
            }

            userId = RedisHelper.StringGet(code);
            if (string.IsNullOrEmpty(userId)) return result.ServerError("缓存异常，请重试");

            RedisHelper.Delete(code);
            token = Core.GetToken(userId);
            if (token == null)
            {
                RedisHelper.Delete($"ID:{account}");
                return result.ServerError("缓存异常，请重试");
            }

            // 验证令牌
            if (token.isInvalid) return result.Disabled();

            if (token.UserIsLocked())
            {
                Core.SetTokenCache(token);
                return result.Locked();
            }

            // 创建令牌数据并返回
            var tokens = token.CreatorKey(code, aid, tid);
            Core.SetTokenCache(token);

            return result.Success(tokens);
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        public Result<object> Verification(string action)
        {
            Verify(action);

            return result;
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> RefreshToken()
        {
            var verify = new Verify(TokenType.RefreshToken);
            token = verify.basis;
            tokenId = verify.tokenId;

            // 限流,令牌在其有效期内可刷新60次
            var key = Util.Hash("RefreshToken" + tokenId);
            var limited = Params.callManage.IsLimited(key, token.life * 12, 60);
            if (limited) return result.BadRequest("刷新次数已用完,请合理刷新");

            result = verify.Compare();
            if (!result.successful) return result;

            // 刷新令牌
            var tokens = token.RefreshToken(tokenId);
            Core.SetTokenCache(token);

            return result.Success(tokens);
        }

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> RemoveToken()
        {
            if (!Verify()) return result;

            token.DeleteKeys(tokenId);
            Core.SetTokenCache(token);

            return result.Success();
        }

        /// <summary>
        /// 获取用户已绑定租户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Result</returns>
        public Result<object> GetTenants(string account)
        {
            using (var context = new Entities())
            {
                var user = context.users.SingleOrDefault(u => u.account == account || u.mobile == account || u.email == account);
                if (user == null) return result.NotFound();

                var list = from m in context.tenantUsers.Where(m => m.userId == user.id)
                    join t in context.tenants on m.tenantId equals t.id
                    select new { t.id, Name = t.alias, remark = t.name };
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 获取用户可登录部门
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Result</returns>
        public Result<object> GetDepts(string account)
        {
            using (var context = new Entities())
            {
                var user = context.users.SingleOrDefault(u => u.account == account || u.mobile == account || u.email == account);
                if (user == null) return result.NotFound();

                var list = from m in context.orgMembers.Where(m => m.userId == user.id)
                    join t in context.organizations on m.orgId equals t.id
                    join d in context.organizations on t.parentId equals d.id
                    select new { d.id, Name = d.fullname, remark = d.code };
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> SetPayPW(string code, string password)
        {
            if (string.IsNullOrEmpty(password)) return result.BadRequest("密码不能为空");

            if (!Verify()) return result;

            // 验证短信验证码
            if (!Core.VerifySmsCode(3, token.mobile, code)) return result.SMSCodeError();

            var key = Util.Hash(userId + password);
            if (token.payPassword == key) return result.Success();

            // 保存支付密码到数据库
            var user = Core.GetUserById(userId);
            user.payPassword = key;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            // 更新Token缓存
            token.payPassword = key;
            token.SetChanged();
            Core.SetTokenCache(token);

            return result.Success();
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> VerifyPayPW(string password)
        {
            if (!Verify()) return result;

            var success = token.VerifyPayPassword(password);
            if (success == null) return result.BadRequest("未设置支付密码,请先设置支付密码!");

            return success.Value ? result : result.InvalidPayKey();
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="life">过期时间（分钟）</param>
        /// <param name="length">字符长度</param>
        /// <returns>Result</returns>
        public Result<object> NewCode(string mobile, int type, int life, int length)
        {
            if (!Verify()) return result;

            var code = Core.GenerateSmsCode(type, mobile, life, length);

            return result.Created(code);
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>Result</returns>
        public Result<object> VerifyCode(string mobile, string code, int type, bool remove = true)
        {
            if (!Verify()) return result;

            return Core.VerifySmsCode(type, mobile, code, !remove) ? result : result.SMSCodeError();
        }
    }
}