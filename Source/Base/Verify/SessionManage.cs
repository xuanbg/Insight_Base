using System;
using System.Linq;
using System.ServiceModel;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.DataAccess;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base.Verify
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class SessionManage : Interface
    {

        #region 验证

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult Verification()
        {
            return General.Verify();
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult Authorization(string action)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid aid;
            if (!Guid.TryParse(action, out aid)) return result.InvalidGuid();

            return Authority(session, aid) ? result : result.Forbidden();
        }

        #endregion

        #region 短信验证码生成和验证

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>JsonResult</returns>
        public JsonResult NewCode(string mobile, int type, int time)
        {
            var result = General.Verify(mobile + Secret);
            if (!result.Successful) return result;

            var record = SmsCodes.OrderByDescending(r => r.CreateTime).FirstOrDefault(r => r.Mobile == mobile && r.Type == type);
            if (record != null && (DateTime.Now - record.CreateTime).TotalSeconds < 60) return result.TimeTooShort();

            var code = Util.Random.Next(100000, 999999).ToString();
            record = new VerifyRecord
            {
                Type = type,
                Mobile = mobile,
                Code = code,
                FailureTime = DateTime.Now.AddMinutes(time),
                CreateTime = DateTime.Now
            };
            SmsCodes.Add(record);
            return result.Success(code);
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>JsonResult</returns>
        public JsonResult VerifyCode(string mobile, string code, int type, bool remove)
        {
            var result = General.Verify(mobile + Secret);
            if (!result.Successful) return result;

            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null) return result.SMSCodeError();

            if (!remove) return result;

            SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type);
            return result;
        }

        #endregion

        #region Session的操作方法

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetSessions()
        {
            var result = Authorization("331BF752-CDB7-44DE-9631-DF2605BB527E");
            if (!result.Successful) return result;

            var list = Sessions.Where(s => s.UserType > 0 && s.OnlineStatus).ToList();
            return result.Success(Serialize(list));
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateUserInfo(SYS_User user)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            var aid = Guid.Parse("3BC17B61-327D-4EAA-A0D7-7F825A6C71DB");
            if (session.UserId != user.ID && !Authority(session, aid)) return result.Forbidden();

            var r = DataAccess.UpdateUserInfo(user);
            if (!r.HasValue) return result.NotFound();

            if (!r.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == user.ID);
            if (session == null) return result;

            session.LoginName = user.LoginName;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.OpenId = user.OpenId;
            return result;
        }

        /// <summary>
        /// 获取用户登录结果
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult UserLogin()
        {
            var result = new JsonResult();
            var session = GetAuthorization<Session>();
            if (session == null) return result.InvalidAuth();

            var verify = new Common.Verify(session);
            result = verify.Compare();
            if (!result.Successful) return result;

            verify.Basis.DeptId = session.DeptId;
            verify.Basis.DeptName = session.DeptName;
            verify.Basis.MachineId = session.MachineId;
            return result.Success(Serialize(verify.Basis));
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetUserOffline(string account)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            var aid = Guid.Parse("331BF752-CDB7-44DE-9631-DF2605BB527E");
            if (session.LoginName != account && !Authority(session, aid)) return result.Forbidden();

            session = Sessions.SingleOrDefault(s => s.LoginName == account);
            if (session != null) session.OnlineStatus = false;

            return result;
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">用户新密码</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateSignature(string id, string pw)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var aid = Guid.Parse("26481E60-0917-49B4-BBAA-2265E71E7B3F");
            if (session.UserId != uid && !Authority(session, aid)) return result.Forbidden();

            var r = UpdatePassword(uid, pw);
            if (!r.HasValue) return result.NotFound();

            if (!r.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == uid);
            if (session != null) session.Signature = Hash(session.LoginName.ToUpper() + pw);

            return result;
        }

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="validity">可用状态</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetUserStatus(string id, bool validity)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var aid = Guid.Parse(validity ? "369548E9-C8DB-439B-A604-4FDC07F3CCDD" : "0FA34D43-2C52-4968-BDDA-C9191D7FCE80");
            if (session.UserId != uid && !Authority(session, aid)) return result.Forbidden();

            var r = DataAccess.SetUserStatus(uid, validity);
            if (!r.HasValue) return result.NotFound();

            if (!r.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == uid);
            if (session != null) session.Validity = validity;

            return result;
        }

        #endregion

    }
}
