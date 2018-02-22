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
        private readonly TokenType tokenType;
        private readonly string secret;
        private readonly string hash;
        private readonly string tenantId;
        private readonly string userId;
        private readonly string deptId;

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Token basis { get; }

        /// <summary>
        /// 令牌ID
        /// </summary>
        public string tokenId { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tokenType">令牌类型：1、访问令牌(默认)；2、刷新令牌</param>
        public Verify(TokenType tokenType = TokenType.AccessToken)
        {
            this.tokenType = tokenType;

            var headers = WebOperationContext.Current.IncomingRequest.Headers;
            var auth = headers[HttpRequestHeader.Authorization];
            var accessToken = Util.Base64ToAccessToken(auth);
            if (accessToken == null)
            {
                var msg = $"提取验证信息失败。Token is:{auth ?? "null"}";
                new Thread(() => Logger.Write("500101", msg)).Start();
                return;
            }

            tokenId = accessToken.id;
            secret = accessToken.secret;
            hash = Util.Hash(auth);
            basis = Core.GetToken(accessToken.userId);
            if (basis == null) return;

            userId = basis.userId;
            if (!basis.SelectKeys(tokenId)) return;

            tenantId = basis.tenantId;
            deptId = basis.deptId;
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
            if (!basis.VerifyToken(hash, secret, tokenType)) return result.InvalidAuth();

            if (basis.TenantIsExpiry()) return result.TenantIsExpiry();

            if (tokenType == TokenType.AccessToken && basis.IsExpiry()) return result.Expired();

            if (basis.IsFailure()) return result.Failured();

            if (basis.isInvalid) return result.Disabled();

            if (tokenType == TokenType.RefreshToken) result.Success();
            else result.Success(Util.ConvertTo<Session>(basis));

            // 如key为空，立即返回；否则进行鉴权
            if (string.IsNullOrEmpty(key)) return result;

            // 根据传入的操作码进行鉴权
            using (var context = new Entities())
            {
                var list = from f in context.functions
                    join p in context.roleFunctions on f.id equals p.functionId
                    join r in context.userRoles on p.roleId equals r.roleId
                    where r.tenantId == tenantId && r.userId == userId && (r.deptId == null || r.deptId == deptId)
                    group p by new {f.id, f.alias, routes = f.url}
                    into g
                    select new {g.Key, permit = g.Min(i => i.permit)};
                var permits = list.Where(i => i.permit > 0);
                var isPermit = permits.Any(i => i.Key.alias.Contains(key) || i.Key.routes.Contains(key));

                return isPermit ? result : result.Forbidden();
            }
        }
    }
}