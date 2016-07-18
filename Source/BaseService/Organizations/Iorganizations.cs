using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceContract]
    public interface IOrganizations
    {

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddOrg(SYS_Organization org, int index);

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveOrg(string id);

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="obj">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult UpdateOrg(string id, SYS_Organization obj, int index);

        /// <summary>
        /// 根据登录账号获取可选登录部门对象列表
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "logindept?account={account}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetLoginDept(string account);

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOrg(string id);

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOrgTree();

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="org">组织节点合并对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "merger", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddOrgMerger(SYS_OrgMerger org);

        /// <summary>
        /// 根据对象实体数据更新组织机构表ParentId字段
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "{id}/parent", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SetOrgParent(string id, SYS_Organization org);

        /// <summary>
        /// 根据参数组集合批量插入职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddOrgMember(string id, List<Guid> uids);

        /// <summary>
        /// 根据ID集合删除职位成员关系
        /// </summary>
        /// <param name="ids">职位成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveOrgMember(List<Guid> ids);

        /// <summary>
        /// 获取所有职位成员用户
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "members", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOrgMembers();

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "others?id={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetOtherOrgMember(string id);

    }
}
