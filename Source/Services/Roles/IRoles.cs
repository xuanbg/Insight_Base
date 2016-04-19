using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using Insight.WS.Base.Common.Utils;

namespace Insight.WS.Base
{
    [ServiceContract]
    public interface IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddRole(RoleInfo role);

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveRole(string id);

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult EditRole(string id, RoleInfo role);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetAllRole();

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members"></param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddRoleMember(string id, List<object> members);

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <param name="type">成员类型</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/members/{id}?type={type}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveRoleMember(string id, string type);

        /// <summary>
        /// 根据角色ID获取可用的成员集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles/{id}/othertitles", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfTitle(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles/{id}/othergroups", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfGroup(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles/{id}/otherusers", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfUser(string id);

        /// <summary>
        /// 获取指定角色的所有功能操作和授权
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles/{id}/actions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleActions(string id);

        /// <summary>
        /// 获取指定角色的所有相对数据权限
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "roles/{id}/reldatas", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleRelData(string id);

    }
}
