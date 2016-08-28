using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IOrganizations
    {
        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddOrg(SYS_Organization org, int index);

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveOrg(string id);

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult UpdateOrg(string id, SYS_Organization org, int index);

        /// <summary>
        /// 根据对象实体数据更新组织机构表ParentId字段
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}/parent", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SetOrgParent(string id, SYS_Organization org);

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "orgs/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOrg(string id);

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "orgs", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOrgTree();

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="org">组织节点对象（被合并节点）</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "orgs/{id}/merger", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult OrgMerger(string id, SYS_Organization org);

        /// <summary>
        /// 根据参数组集合批量插入职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="ids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "orgs/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddOrgMember(string id, List<Guid> ids);

        /// <summary>
        /// 根据ID集合删除职位成员关系
        /// </summary>
        /// <param name="ids">职位成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "orgs/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveOrgMember(List<Guid> ids);

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "orgs/{id}/other", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOtherOrgMember(string id);

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "orgs/logindept?account={account}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetLoginDepts(string account);
    }
}
