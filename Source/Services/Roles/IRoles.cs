using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceContract]
    public interface IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">角色对象实体</param>
        /// <param name="action">功能授权表</param>
        /// <param name="data">数据授权表</param>
        /// <param name="custom">自定义数据授权表</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddRole(SYS_Role role, DataTable action, DataTable data, DataTable custom);

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveRole(string id);

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="obj">角色对象实体</param>
        /// <param name="adl">功能删除列表</param>
        /// <param name="ddl">相对数据授权删除列表</param>
        /// <param name="cdl">绝对数据授权删除列表</param>
        /// <param name="adt">功能授权表</param>
        /// <param name="ddt">相对数据授权表</param>
        /// <param name="cdt">绝对数据授权表</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult EditRole(string id, SYS_Role obj, List<object> adl, List<object> ddl, List<object> cdl, DataTable adt, DataTable ddt, DataTable cdt);

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
        /// <param name="tids">职位ID集合</param>
        /// <param name="gids">用户组ID集合</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddRoleMember(string id, List<string> tids, List<string> gids, List<string> uids);

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <param name="type">成员类型</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "{id}/members?type={type}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult DeleteRoleMember(string id, int type);

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/titles", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfTitle(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/groups", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfGroup(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/users", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetMemberOfUser(string id);

        /// <summary>
        /// 获取指定角色的所有功能操作和授权
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/actions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleActions(string id);

        /// <summary>
        /// 获取指定角色的所有相对数据权限
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/reldatas", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleRelData(string id);

        /// <summary>
        /// 获取角色模块权限授权信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "allowmodules", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleModulePermit();

        /// <summary>
        /// 获取角色操作权限授权信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "allowactions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleActionPermit();

        /// <summary>
        /// 获取角色数据权限授权信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "allowdatas", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRoleDataPermit();

    }
}
