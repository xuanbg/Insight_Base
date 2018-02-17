using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IAuth
    {

        #region Verify

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 联通性测试接口
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "test", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> Test();

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <param name="type">登录类型</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tokens/codes?account={account}&type={type}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetCode(string account, int type);

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tokens?appid={appId}&account={account}&signature={signature}&deptid={deptid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetToken(string appId, string account, string signature, string deptid);

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> RemoveToken();

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> RefreshToken();

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tokens/verify?action={action}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> Verification(string action);

        #endregion

        #region PayPW

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "paypws", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> SetPayPW(string code, string password);

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "paypws?pw={password}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> VerifyPayPW(string password);

        #endregion

        #region SMSCode

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "smscodes?mobile={mobile}&type={type}&time={time}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> NewCode(string mobile, int type, int time);

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "smscodes/compare?mobile={mobile}&code={code}&type={type}&remove={remove}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> VerifyCode(string mobile, string code, int type, bool remove);
        
        #endregion

    }
}
