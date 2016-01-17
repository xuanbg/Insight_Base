using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Service
{
    [ServiceContract]
    public interface Iverify
    {

        #region Verify

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult Verification();

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "auth?action={action}", ResponseFormat = WebMessageFormat.Json)]
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
        [WebGet(UriTemplate = "smscode?mobile={mobile}&type={type}&time={time}", ResponseFormat = WebMessageFormat.Json)]
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
        [WebGet(UriTemplate = "smscode/compare?mobile={mobile}&code={code}&type={type}&remove={remove}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult VerifyCode(string mobile, string code, int type, bool remove);

        #endregion

        #region Session

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "sessions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetSessions();

        #endregion

    }
}
