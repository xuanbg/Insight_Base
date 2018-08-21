using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.DTO;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IRoles
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void responseOptions();

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="info">RoleInfo</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addRole(RoleInfo info);

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> removeRole(string id);

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id"></param>
        /// <param name="info"></param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> editRole(string id, RoleInfo info);

        /// <summary>
        /// 获取指定角色
        /// </summary>ID
        /// <param name="id">角色</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getRole(string id);

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles?rows={rows}&page={page}&key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getRoles(int rows, int page, string key);

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">成员对象集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "roles/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addRoleMember(string id, List<RoleMember> members);

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "roles/members/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> removeRoleMember(string id);

        /// <summary>
        /// 根据角色ID获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/users?rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getMemberUsers(string id, int rows, int page);

        /// <summary>
        /// 根据角色ID获取可用的成员集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/othertitles", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getMemberOfTitle(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/othergroups", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getMemberOfGroup(string id);

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/otherusers", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getMemberOfUser(string id);

        /// <summary>
        /// 获取可用的权限资源列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="aid">应用ID（可为空）</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "roles/{id}/allperm?appid={aid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getAppTree(string id, string aid);
    }
}
