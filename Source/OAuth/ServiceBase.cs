using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public class ServiceBase
    {
        protected Result<object> result = new Result<object>();
        protected TokenManage manage;
        protected string tokenId;
        protected string tenantId;
        protected string appId;
        protected string userId;
        protected string userName;
        protected string deptId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <param name="id">用户ID</param>
        /// <returns>bool 身份是否通过验证</returns>
        protected bool verify(string key = null, string id = null)
        {
            var verify = new Verify();
            tokenId = verify.tokenId;
            manage = verify.manage;
            if (manage == null) return false;

            tenantId = manage.tenantId;
            appId = manage.appId;
            userId = manage.userId;
            userName = manage.userName;
            deptId = manage.deptId;
            result = verify.compare(userId == id ? null : key);

            return result.successful;
        }

        /// <summary>
        /// 获取客户端特征指纹
        /// </summary>
        /// <returns>string 客户端特征指纹</returns>
        protected static string getFingerprint()
        {
            var verify = new Verify(false);

            return Util.hash(verify.ip + verify.userAgent);
        }
    }
}
