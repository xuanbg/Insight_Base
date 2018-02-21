using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public class ServiceBase
    {
        public Result<object> result = new Result<object>();
        public Token token;
        public string tokenId;
        public string tenantId;
        public string userId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <param name="id">用户ID</param>
        /// <returns>bool 身份是否通过验证</returns>
        public bool Verify(string key = null, string id = null)
        {
            var verify = new Verify();
            token = verify.basis;
            tokenId = verify.tokenId;
            tenantId = token.tenantId;
            userId = token.userId;
            result = verify.Compare(userId == id ? null : key);

            return result.successful;
        }
    }
}
