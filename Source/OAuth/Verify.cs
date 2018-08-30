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
        public Verify(bool checkAuth = true, TokenType tokenType = TokenType.ACCESS_TOKEN)
        {
            this.tokenType = tokenType;

            var requst = WebOperationContext.Current?.IncomingRequest;
            if (requst == null) return;

            ip = getIp(requst.Headers);
            userAgent = requst.UserAgent;

            var auth = requst.Headers[HttpRequestHeader.Authorization];
            var accessToken = Util.base64ToAccessToken(auth);
            if (accessToken != null)
            {
                tokenId = accessToken.id;
                secret = accessToken.secret;
                hash = Util.hash(auth);
                manage = Core.getUserCache(accessToken.userId);
                manage?.getToken(tokenId);

                return;
            }

            if (!checkAuth) return;

            var msg = $"提取验证信息失败。Token is:{auth ?? "null"}";
            new Thread(() => Logger.write("500101", msg)).Start();
        }

        /// <summary>
        /// 对Secret进行校验，返回验证结果
        /// </summary>
        /// <param name="key">操作码，默认为空</param>
        /// <returns>Result</returns>
        public Result<object> compare(string key = null)
        {
            if (manage == null) return result.invalidToken();

            // 验证令牌
            if (manage.isFailure(tokenId, hash, tokenType)) return result.failured();

            if (tokenType == TokenType.ACCESS_TOKEN && manage.isExpiry()) return result.expired();

            if (manage.isInvalid) return result.disabled();

            if (manage.tenantIsExpiry()) return result.tenantIsExpiry();

            if (!manage.verify(secret, tokenType)) return result.invalidAuth();

            if (tokenType == TokenType.REFRESH_TOKEN) result.success();
            else
            {
                var info = new UserInfo
                {
                    id = manage.userId,
                    tenantId = manage.getTenantId(),
                    deptId = manage.getDeptId(),
                    name = manage.userName,
                    account = manage.account,
                    mobile = manage.mobile,
                    email = manage.email
                };
                result.success(info);
            }

            // 如key为空，立即返回；否则进行鉴权
            if (string.IsNullOrEmpty(key)) return result;

            return manage.verifyKeyInCache(key) ? result : result.forbidden();
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <param name="headers">请求头</param>
        /// <returns>string IP地址</returns>
        private static string getIp(NameValueCollection headers)
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