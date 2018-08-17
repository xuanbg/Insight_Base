using System.Collections.Specialized;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using Insight.Base.Common;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public class Verify
    {
        private readonly Result<object> result = new Result<object>();
        private readonly TokenType tokenType;
        private readonly string secret;
        private readonly string hash;

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public TokenManage manage { get; }

        /// <summary>
        /// 令牌ID
        /// </summary>
        public string tokenId { get; }

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        public string ip { get; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        public string userAgent { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="checkAuth"></param>
        /// <param name="tokenType">令牌类型：1、访问令牌(默认)；2、刷新令牌</param>
        public Verify(bool checkAuth = true, TokenType tokenType = TokenType.AccessToken)
        {
            this.tokenType = tokenType;

            var requst = WebOperationContext.Current.IncomingRequest;
            ip = GetIp(requst.Headers);
            userAgent = requst.UserAgent;

            var auth = requst.Headers[HttpRequestHeader.Authorization];
            var accessToken = Util.Base64ToAccessToken(auth);
            switch (accessToken)
            {
                case null when checkAuth:
                    var msg = $"提取验证信息失败。Token is:{auth ?? "null"}";
                    new Thread(() => Logger.Write("500101", msg)).Start();
                    return;

                case null:
                    return;
            }

            tokenId = accessToken.id;
            secret = accessToken.secret;
            hash = Util.Hash(auth);
            manage = Core.GetUserCache(accessToken.userId);
            manage?.GetToken(tokenId);
        }

        /// <summary>
        /// 对Secret进行校验，返回验证结果
        /// </summary>
        /// <param name="key">操作码，默认为空</param>
        /// <returns>Result</returns>
        public Result<object> Compare(string key = null)
        {
            if (manage == null) return result.InvalidToken();

            // 验证令牌
            if (manage.IsFailure(tokenId, hash, tokenType)) return result.Failured();

            if (tokenType == TokenType.AccessToken && manage.IsExpiry()) return result.Expired();

            if (manage.isInvalid) return result.Disabled();

            if (manage.TenantIsExpiry()) return result.TenantIsExpiry();

            if (!manage.Verify(secret, tokenType)) return result.InvalidAuth();

            if (tokenType == TokenType.RefreshToken) result.Success();
            else
            {
                var info = new UserInfo
                {
                    id = manage.userId,
                    tenantId = manage.tenantId,
                    deptId = manage.deptId,
                    name = manage.userName,
                    account = manage.account,
                    mobile = manage.mobile,
                    email = manage.email
                };
                result.Success(info);
            }

            // 如key为空，立即返回；否则进行鉴权
            if (string.IsNullOrEmpty(key)) return result;

            return manage.VerifyKeyInCache(key) ? result : result.Forbidden();
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <param name="headers">请求头</param>
        /// <returns>string IP地址</returns>
        private static string GetIp(NameValueCollection headers)
        {
            var rip = headers.Get("X-Real-IP");
            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("X-Forwarded-For");
            }

            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("Proxy-Client-IP");
            }

            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("WL-Proxy-Client-IP");
            }

            return rip;
        }
    }
}