using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Server;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class Auth : IAuth
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 联通性测试接口
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> Test()
        {
            return new Result<object>().Success();
        }

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <param name="type">登录类型</param>
        /// <returns>Result</returns>
        public Result<object> GetCode(string account, int type = 0)
        {
            userId = Core.GetUserId(account);
            if (userId == null) return result.NotExists();

            token = Core.GetToken(userId);
            if (token == null)
            {
                RedisHelper.Delete(account);
                return result.ServerError("缓存异常，请重试");
            }

            // 限流,每用户5/30秒可访问一次
            var key = Util.Hash("getCode" + userId + type);
            var surplus = Params.callManage.GetSurplus(key, type == 0 ? 5 : 30);
            if (surplus > 0) return result.TooFrequent(surplus);

            // 生成Code
            object code = Core.GenerateCode(token, account, type);

            return result.Success(code);
        }

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> GetToken(string appId, string account, string signature, string deptid)
        {
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

                return result.InvalidAuth();
            }

            userId = RedisHelper.StringGet(code);
            if (string.IsNullOrEmpty(userId)) return result.ServerError("缓存异常，请重试");

            RedisHelper.Delete(code);
            token = Core.GetToken(userId);
            if (token == null)
            {
                RedisHelper.Delete(account);
                return result.ServerError("缓存异常，请重试");
            }

            if (token.UserIsInvalid())
            {
                Core.SetTokenCache(token);
                return result.Disabled();
            }

            // 创建令牌数据并返回
            var tokens = token.CreatorKey(code, Core.GetTokenLife(appId), appId);
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
            return result.Success();
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> RefreshToken()
        {
            var headers = WebOperationContext.Current.IncomingRequest.Headers;
            var auth = headers[HttpRequestHeader.Authorization];
            var buffer = Convert.FromBase64String(auth);
            var json = Encoding.UTF8.GetString(buffer);
            var refreshToken = Util.Deserialize<RefreshToken>(json);
            if (refreshToken == null) return result.InvalidToken();

            tokenId = refreshToken.id;
            userId = refreshToken.userId;

            // 限流,每客户端每24小时可访问60次
            var key = Util.Hash("refreshToken" + tokenId);
            var limited = Params.callManage.IsLimited(key, 3600 * 24, 60);
            if (limited) return result.BadRequest("刷新次数已用完,请合理刷新");

            // 验证令牌
            var basis = Core.GetToken(userId);
            if (basis == null) return result.InvalidToken();

            basis.SelectKeys(tokenId);
            if (!basis.VerifyToken(refreshToken.secret, 2)) return result.InvalidToken();

            // 刷新令牌
            var tokens = basis.RefreshToken(tokenId);
            Core.SetTokenCache(basis);

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
            var user = Params.GetUser(userId);
            user.payPassword = key;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            // 更新Token缓存
            token.payPassword = key;
            token.isChanged = true;
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
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>Result</returns>
        public Result<object> NewCode(string mobile, int type, int time)
        {
            if (!Verify()) return result;

            var code = Params.Random.Next(100000, 999999).ToString();
            var key = type + mobile;
            var expire = DateTime.Now.AddMinutes(time);

            var msg = $"已经为手机号【{mobile}】的用户生成了类型为【{type}】的短信验证码：【{code}】。此验证码将于{expire}失效。";
            Task.Run(() => Logger.Write("700501", msg));

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

        private Result<object> result = new Result<object>();
        private Token token;
        private string tokenId;
        private string userId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string key = null)
        {
            var verify = new OAuth.Verify();
            result = verify.Compare(key);
            if (!result.successful) return false;

            token = verify.basis;
            tokenId = verify.tokenId;
            userId = token.userId;
            return result.successful;
        }
    }
}