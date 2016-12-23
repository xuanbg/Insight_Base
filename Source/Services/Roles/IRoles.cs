using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddRole(RoleInfo role);

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result RemoveRole(string id);

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result EditRole(string id, RoleInfo role);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles?rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetAllRole(string rows, string page);

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">成员对象集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddRoleMember(string id, List<RoleMember> members);

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/members/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result RemoveRoleMember(string id);

        /// <summary>
        /// 根据角色ID获取角色授权信息
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/permission", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetRolePermission(string id);

        /// <summary>
        /// 根据角色ID获取角色成员集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/members", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetRoleMembers(string id);

        /// <summary>
        /// 根据角色ID获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/users?rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetMemberUsers(string id, string rows, string page);

        /// <summary>
        /// 根据角色ID获取可用的成员集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/othertitles", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetMemberOfTitle(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/othergroups", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetMemberOfGroup(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/otherusers", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetMemberOfUser(string id);

        /// <summary>
        /// 获取可用的操作资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/actions?id={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetActions(string id);

        /// <summary>
        /// 获取可用的数据资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/datas?id={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetDatas(string id);
    }
}
