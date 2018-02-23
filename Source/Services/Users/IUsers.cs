using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IUsers
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "users", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddUser(User user);

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> RemoveUser(string id);

        /// <summary>
        /// 根据用户ID更新用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user">用户数据对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> UpdateUserInfo(string id, User user);

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users/myself", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetMyself();

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetUser(string id);

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users?rows={rows}&page={page}&key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetUsers(int rows, int page, string key);

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="code">验证码</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "users/signup?code={code}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> SignUp(string appId, string code, User user);

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="password">新密码</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{id}/signature", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> UpdateSignature(string id, string password);

        /// <summary>
        /// 用户重置登录密码
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <param name="code">短信验证码</param>
        /// <param name="mobile">手机号，默认为空。如为空，则使用account</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{account}/resetpw", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> ResetSignature(string appId, string account, string password, string code, string mobile = null);

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="invalid">可用状态</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "users/{id}/validity", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> SetUserStatus(string id, bool invalid);

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="id">用户账号</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "users/{id}/token", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> UserSignOut(string id);

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="deptid">登录部门ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "users/{id}/roles?deptid={deptid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetUserRoles(string id, string deptid);
    }
}
