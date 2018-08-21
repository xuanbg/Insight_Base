using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.DTO;
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
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void responseOptions();

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="type">登录类型</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tokens/codes?account={account}&type={type}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getCode(string account, int type);

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="appId">应用ID</param>
        /// <param name="account">登录账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptId">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        [WebGet(
            UriTemplate =
                "tokens?tenantid={tenantId}&appid={appId}&account={account}&signature={signature}&deptid={deptId}",
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getToken(string tenantId, string appId, string account, string signature, string deptId);

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "tokens/secret?action={action}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> verification(string action);

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> refreshToken();

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> removeToken();

        /// <summary>
        /// 获取用户已绑定租户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "{account}/tenants", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getTenants(string account);

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "paypws", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> setPayPw(string code, string password);

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "paypws?pw={password}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> verifyPayPw(string password);

        #endregion

        #region SMSCode

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="life">过期时间（分钟）</param>
        /// <param name="length">字符长度</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "smscodes?mobile={mobile}&type={type}&life={life}&length={length}",
            ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> newCode(string mobile, int type, int life, int length);

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="code">验证码对象</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "smscodes", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> verifyCode(SmsCode code);

        #endregion
    }
}