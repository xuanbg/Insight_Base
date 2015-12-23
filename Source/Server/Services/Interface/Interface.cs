using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using Insight.WS.Server.Common;
using Insight.WS.Server.Common.ORM;
using Insight.WS.Server.Common.Service;
using static Insight.WS.Server.Common.General;
using static Insight.WS.Server.Common.SqlHelper;
using static Insight.WS.Server.Common.Util;

namespace Insight.WS.Service.SuperDentist
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Interface : IInterface
    {

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="smsCode">短信验证码</param>
        /// <param name="password">密码MD5值</param>
        /// <returns>JsonResult</returns>
        public JsonResult Register(string smsCode, string password)
        {
            var result = new JsonResult {Code = "500", Name = "UnknownError", Message = "未知错误" };
            var obj = GetAuthorization<Session>();
            var signature = Hash(obj.LoginName.ToUpper() + smsCode + password);
            if (signature != obj.Signature)
            {
                result.Code = "401";
                result.Name = "InvalidAuthenticationInfo";
                result.Message = "提供的身份验证信息不正确";
                return result;
            }

            using (var context = new WSEntities())
            {
                // 验证用户登录名是否已存在
                var user = context.SYS_User.FirstOrDefault(u => u.LoginName == obj.LoginName);
                if (user != null)
                {
                    result.Code = "409";
                    result.Name = "AccountAlreadyExists";
                    result.Message = "用户已存在";
                    return result;
                }
            }

            //if (!VerifyCode(obj.LoginName, smsCode, 1))
            //{
            //    result.Code = "410";
            //    result.Name = "SMSCodeError";
            //    result.Message = "短信验证码错误";
            //    return result;
            //}

            var cmds = new List<SqlCommand>
            {
                DataAccess.AddUser(new SYS_User {Name = obj.UserName, LoginName = obj.LoginName, Password = password, Type = -1})
            };
            if (!SqlExecute(cmds))
            {
                result.Code = "501";
                result.Name = "DataBaseError";
                result.Message = "数据写入失败";
                return result;
            }

            obj.Signature = Hash(obj.LoginName.ToUpper() + password);
            return GetJson(UserLogin(obj));
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="us">用户会话</param>
        /// <returns>JsonResult</returns>
        public JsonResult Login(Session us)
        {
            var session = UserLogin(us);
            return GetJson(session);
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="id">SessionID</param>
        /// <returns>JsonResult</returns>
        public JsonResult Logout(int id)
        {
            var result = Verify();
            if (result == null || !result.Successful) return result;

            SetOnlineStatus(id, false);
            return result;
        }

    }
}
