using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface ISecurity
    {

        #region Verify

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="stamp">用户特征码（随机字符串）</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "tokens?account={account}&signature={signature}&stamp={stamp}&deptid={deptid}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetToken(string account, string signature, string stamp, string deptid);

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveToken();

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "tokens", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RefreshToken();

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "tokens/verify", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult Verification();

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "tokens/auth?action={action}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult Authorization(string action);

        #endregion

        #region SMSCode

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "smscodes?mobile={mobile}&type={type}&time={time}", ResponseFormat = WebMessageFormat.Json)]
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
        [WebGet(UriTemplate = "smscodes/compare?mobile={mobile}&code={code}&type={type}&remove={remove}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult VerifyCode(string mobile, string code, int type, bool remove);

        #endregion

        #region Session

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <param name="type">用户类型</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "sessions?type={type}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetSessions(string type);

        #endregion

    }
}
