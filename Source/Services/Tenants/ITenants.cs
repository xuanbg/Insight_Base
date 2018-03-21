using System.Collections.Generic;
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
        Result<object> GetTenants(int rows, int page, string key);

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
        /// <param name="id">租户ID</param>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tenants/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditTenant(string id, Tenant tenant);

        /// <summary>
        /// 延长有效天数
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <param name="expire">续租天数</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tenants/{id}/expire", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> ExtendTenant(string id, int expire);

        /// <summary>
        /// 删除指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tenants/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteTenant(string id);

        /// <summary>
        /// 为租户绑定应用
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <param name="apps">绑定应用ID集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tenants/{id}/apps", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> BindApp(string id, List<string> apps);

        /// <summary>
        /// 为租户关联用户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <param name="tenant">租户-用户关系实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "tenants/{id}/user", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddTenantUser(string id, TenantUser tenant);

        /// <summary>
        /// 删除指定ID的租户和用户的绑定关系
        /// </summary>
        /// <param name="id">租户-用户关系ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tenants/user/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteTenantUser(string id);
    }
}
