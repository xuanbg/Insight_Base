using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IOrganizations
    {
        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddOrg(Organization org);

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result RemoveOrg(string id);

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result UpdateOrg(string id, Organization org);

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetOrg(string id);

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetOrgs();

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="org">组织节点对象（被合并节点）</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}/merger", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result OrgMerger(string id, Organization org);

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddOrgMember(string id, Organization org);

        /// <summary>
        /// 根删除职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result RemoveOrgMember(string id, Organization org);

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs/{id}/other", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetOtherOrgMember(string id);

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "orgs/logindept?account={account}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetLoginDepts(string account);
    }
}
