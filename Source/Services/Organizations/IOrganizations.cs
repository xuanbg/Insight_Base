using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IOrganizations
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void responseOptions();

        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addOrg(Org org);

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> removeOrg(string id);

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> updateOrg(string id, Org org);

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getOrg(string id);

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getOrgs();

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members">成员集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs/{id}/members", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addOrgMember(string id, List<string> members);

        /// <summary>
        /// 根删除职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members">成员集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/{id}/members", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> removeOrgMember(string id, List<string> members);

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs/{id}/other", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getOtherOrgMember(string id);
    }
}