using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Verify : IVerify
    {

        #region Verify

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
            var context = WebOperationContext.Current;
            if (context == null) return;

            var response = context.OutgoingResponse;
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult Verification()
        {
            var verify = new SessionVerify();
            verify.Compare();
            return verify.Result;
        }

        /// <summary>
        /// 会话合法性验证并校验支付密码
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult Confirmation(string paykey)
        {
            var verify = new SessionVerify();
            verify.Confirm(paykey);
            return verify.Result;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult Authorization(string action)
        {
            var verify = new SessionVerify();
            verify.Compare(action);
            return verify.Result;
        }

        #endregion

        #region SMSCode

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>JsonResult</returns>
        public JsonResult NewCode(string mobile, int type, int time)
        {
            var verify = General.Verify(mobile + Util.Secret);
            if (!verify.Successful) return verify;

            var record = Util.SmsCodes.OrderByDescending(r => r.CreateTime).FirstOrDefault(r => r.Mobile == mobile && r.Type == type);
            if (record != null && (DateTime.Now - record.CreateTime).TotalSeconds < 60) return verify.TimeTooShort();

            var code = Util.Random.Next(100000, 999999).ToString();
            record = new VerifyRecord
            {
                Type = type,
                Mobile = mobile,
                Code = code,
                FailureTime = DateTime.Now.AddMinutes(time),
                CreateTime = DateTime.Now
            };
            Util.SmsCodes.Add(record);
            General.LogToLogServer("700501", $"已经为手机号【{mobile}】的用户生成了类型为【{type}】的短信验证码：【{code}】。此验证码将于{record.FailureTime}失效。", "验证服务", "生成短信验证码");
            return verify.Success(code);
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>JsonResult</returns>
        public JsonResult VerifyCode(string mobile, string code, int type, bool remove = true)
        {
            var verify = General.Verify(mobile + Util.Secret);
            if (!verify.Successful) return verify;

            Util.SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = Util.SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null) return verify.SMSCodeError();

            if (!remove) return verify;

            Util.SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type);
            return verify;
        }

        #endregion

        #region Session

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <param name="type">用户类型</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetSessions(string type)
        {
            const string action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var list = SessionManage.GetSessions(Convert.ToInt32(type));
            return list.Count > 0 ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        #endregion

    }
}
