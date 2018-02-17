using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public class Verify
    {
        private readonly Result<object> result = new Result<object>();
        private readonly AccessToken accessToken;

        /// <summary>
        /// 令牌ID
        /// </summary>
        public string tokenId { get; }

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Token basis { get; }

        /// <summary>
        /// 构造方法
        /// </summary>
        public Verify()
        {
            var headers = WebOperationContext.Current.IncomingRequest.Headers;
            var auth = headers[HttpRequestHeader.Authorization];
            accessToken = Util.Base64ToAccessToken(auth);
            if (accessToken == null)
            {
                var msg = $"提取验证信息失败。Token is:{auth ?? "null"}";
                new Thread(() => Logger.Write("500101", msg)).Start();
                return;
            }

            tokenId = accessToken.id;
            basis = Core.GetToken(accessToken.userId);
        }

        /// <summary>
        /// 对Secret进行校验，返回验证结果
        /// </summary>
        /// <param name="key">操作码，默认为空</param>
        /// <returns>Result</returns>
        public Result<object> Compare(string key = null)
        {
            if (basis == null) return result.InvalidToken();

            // 验证令牌
            basis.SelectKeys(tokenId);
            if (basis.IsExpiry()) return result.Expired();

            if (basis.IsFailure()) return result.Failured();

            if (basis.UserIsInvalid())
            {
                Core.SetTokenCache(basis);
                return result.Disabled();
            }

            if (!basis.VerifyToken(accessToken.secret, 1))
            {
                Core.SetTokenCache(basis);
                return result.InvalidAuth();
            }

            // 如key为空，立即返回；否则进行鉴权
            if (string.IsNullOrEmpty(key)) return result.Success();

            // 根据传入的操作码进行鉴权
            using (var context = new BaseEntities())
            {
                var permits = context.GetPermits(basis.userId, basis.deptId);
                var isPermit = permits.Any(i => i.alias.Contains(key) || i.routes.Contains(key));

                return isPermit ? result.Success() : result.Forbidden();
            }
        }
    }
}