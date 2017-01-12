using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IUsers
    {
        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "users", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddUser(User user);

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result RemoveUser(string id);

        /// <summary>
        /// 根据用户ID更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="user">用户数据对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result UpdateUserInfo(string id, User user);

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetUser(string id);

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users?rows={rows}&page={page}&key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetUsers(string rows, string page, string key);

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "users/{account}/signup", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result SignUp(string account, User user);

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{account}/signature", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result UpdateSignature(string account, string password);

        /// <summary>
        /// 用户重置登录密码
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <param name="code">短信验证码</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{account}/resetpw", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result ResetSignature(string account, string password, string code);

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="validity">可用状态</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{account}/validity", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result SetUserStatus(string account, bool validity);

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "users/{account}/token", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result UserSignOut(string account);

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="deptid">登录部门ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users/{id}/roles?deptid={deptid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetUserRoles(string id, string deptid);
    }
}
