using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Verify
{
    [ServiceContract]
    interface Interface
    {

        #region 验证

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "verify", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult Verification();

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "verify/auth?action={action}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult Authorization(string action);

        #endregion

        #region 短信验证码生成和验证

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "verify/smscode?mobile={mobile}&type={type}&time={time}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult NewCode(string mobile, int type, int time);

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "verify/smscode/compare?mobile={mobile}&code={code}&type={type}&remove={remove}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult VerifyCode(string mobile, string code, int type, bool remove);

        #endregion

        #region Session的操作方法

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "users", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetSessions();

        /// <summary>
        /// 根据用户ID更新用户信息
        /// </summary>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "user", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult UpdateUserInfo(SYS_User user);

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "user/signin", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult UserLogin();

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "user/signout", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SetUserOffline(string account);

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">用户新密码</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "user/signature", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult UpdateSignature(string id, string pw);

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="validity">可用状态</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "user/validity", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SetUserStatus(string id, bool validity);

        #endregion

    }
}
