using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.DTO;
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
        public void responseOptions()
        {
        }

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="type">登录类型</param>
        /// <returns>Result</returns>
        public Result<object> getCode(string account, int type)
        {
            // 限流,每客户端每天可访问100次
            var key = Util.hash("GetCode" + getFingerprint());
            var limited = Params.callManage.isLimited(key, 3600 * 24, 100);
            if (limited) return result.badRequest("当天获取Code次数已用完,请合理使用接口");

            userId = Core.getUserId(account);
            if (userId == null) return result.notExists();

            manage = Core.getUserCache(userId);
            if (manage == null)
            {
                RedisHelper.delete($"ID:{account}");
                return result.serverError("缓存异常，请重试");
            }

            // 生成Code
            object code = Core.generateCode(manage, account, type);

            return result.success(code);
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
        public Result<object> getToken(string tid, string aid, string account, string signature, string deptid)
        {
            // 限流,每客户端每天可访问100次
            var key = Util.hash("GetToken" + getFingerprint());
            var limited = Params.callManage.isLimited(key, 3600 * 24, 100);
            if (limited) return result.badRequest("当天获取Token次数已用完,请合理使用接口");

            var code = Core.getCode(signature);
            if (string.IsNullOrEmpty(code))
            {
                userId = Core.getUserId(account);
                manage = Core.getUserCache(userId);
                if (manage != null)
                {
                    manage.addFailureCount();
                    Core.setUserCache(manage);
                    var msg = $"账号 {account} 正在尝试使用错误的签名请求令牌!";
                    new Thread(() => Logger.write("400101", msg)).Start();
                }

                return manage.userIsLocked() ? result.locked() : result.invalidAuth();
            }

            userId = RedisHelper.stringGet(code);
            if (string.IsNullOrEmpty(userId)) return result.serverError("缓存异常，请重试");

            RedisHelper.delete(code);
            manage = Core.getUserCache(userId);
            if (manage == null)
            {
                RedisHelper.delete($"ID:{account}");
                return result.serverError("缓存异常，请重试");
            }

            // 验证令牌
            if (manage.isInvalid) return result.disabled();

            if (manage.userIsLocked())
            {
                Core.setUserCache(manage);
                return result.locked();
            }

            // 创建令牌数据并返回
            var tokens = manage.creator(code, aid, tid);
            Core.setUserCache(manage);

            return result.success(tokens);
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        public Result<object> verification(string action)
        {
            verify(action);

            return result;
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> refreshToken()
        {
            var verify = new Verify(true, TokenType.REFRESH_TOKEN);
            manage = verify.manage;
            tokenId = verify.tokenId;
            if (manage == null) return result.invalidToken();

            // 限流,令牌在其有效期内可刷新60次
            var key = Util.hash("RefreshToken" + tokenId);
            var limited = Params.callManage.isLimited(key, manage.life * 12, 60);
            if (limited) return result.badRequest("刷新次数已用完,请合理刷新");

            result = verify.compare();
            if (!result.successful) return result;

            // 刷新令牌
            var tokens = manage.refresh(tokenId);
            Core.setUserCache(manage);

            return result.success(tokens);
        }

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> removeToken()
        {
            if (!verify()) return result;

            TokenManage.delete(tokenId);
            Core.setUserCache(manage);

            return result.success();
        }

        /// <summary>
        /// 获取用户已绑定租户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Result</returns>
        public Result<object> getTenants(string account)
        {
            using (var context = new Entities())
            {
                var user = context.users.SingleOrDefault(u =>
                    u.account == account || u.mobile == account || u.email == account);
                if (user == null) return result.notFound();

                var list = from m in context.tenantUsers.Where(m => m.userId == user.id)
                    join t in context.tenants on m.tenantId equals t.id
                    select new {t.id, Name = t.alias, remark = t.name};
                return list.Any() ? result.success(list.ToList()) : result.noContent(new List<object>());
            }
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> setPayPw(string code, string password)
        {
            if (string.IsNullOrEmpty(password)) return result.badRequest("密码不能为空");

            if (!verify()) return result;

            // 验证短信验证码
            if (!Core.verifySmsCode(3, manage.mobile, code)) return result.smsCodeError();

            var key = Util.hash(userId + password);
            if (manage.payPassword == key) return result.success();

            // 保存支付密码到数据库
            var user = Core.getUserById(userId);
            user.payPassword = key;
            if (!DbHelper.update(user)) return result.dataBaseError();

            // 更新Token缓存
            manage.payPassword = key;
            manage.setChanged();
            Core.setUserCache(manage);

            return result.success();
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> verifyPayPw(string password)
        {
            if (!verify()) return result;

            var success = manage.verifyPayPassword(password);
            if (success == null) return result.badRequest("未设置支付密码,请先设置支付密码!");

            return success.Value ? result : result.invalidPayKey();
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="life">过期时间（分钟）</param>
        /// <param name="length">字符长度</param>
        /// <returns>Result</returns>
        public Result<object> newCode(string mobile, int type, int life, int length)
        {
            var fingerprint = getFingerprint();
            var limitKey = Util.hash("GetSmsCode" + fingerprint + type);
            var surplus = Params.callManage.getSurplus(limitKey, 10);
            if (surplus > 0) return result.tooFrequent(surplus);

            var code = Core.generateSmsCode(type, mobile, life, length);

            return result.created(code);
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="code">验证码对象</param>
        /// <returns>Result</returns>
        public Result<object> verifyCode(SmsCode code)
        {
            var fingerprint = getFingerprint();
            var limitKey = Util.hash("VerifyCode" + fingerprint + Util.serialize(code));
            var surplus = Params.callManage.getSurplus(limitKey, 600);
            if (surplus > 0) return result.tooFrequent(surplus);

            var verify = Core.verifySmsCode(code.type, code.mobile, code.code, !code.remove);

            return verify ? result.success() : result.smsCodeError();
        }
    }
}