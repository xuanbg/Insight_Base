using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface ITenants
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 根据关键词查询全部租户集合
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tenants?rows={rows}&page={page}&key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetAllTenants(int rows, int page, string key);

        /// <summary>
        /// 获取指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tenants/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetTenant(string id);

        /// <summary>
        /// 新增租户信息
        /// </summary>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "tenants", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddTenant(Tenant tenant);

        /// <summary>
        /// 修改租户信息
        /// </summary>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tenants", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditTenant(Tenant tenant);

        /// <summary>
        /// 删除指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tenants/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteTenant(string id);
    }
}
