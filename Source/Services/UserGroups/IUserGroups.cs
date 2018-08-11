using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IUserGroups
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 根据对象实体数据新增一个用户组
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "groups", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddGroup(Group group);

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "groups/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> RemoveGroup(string id);

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "groups/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> UpdateGroup(string id, Group group);

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "groups/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetGroup(string id);

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "groups?rows={rows}&page={page}&key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetGroups(int rows, int page, string key);

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "groups/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddGroupMember(string id, Group group);

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "groups/{id}/members", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> RemoveMember(string id, Group group);

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "groups/{id}/other", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetOtherUser(string id);
    }
}
