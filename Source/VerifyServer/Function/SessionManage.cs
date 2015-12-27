using System;
using System.Collections.Generic;
using System.Linq;
using static Insight.WS.Verify.Util;

namespace Insight.WS.Verify
{
    public class SessionManage : Interface
    {

        #region 短信验证码生成和验证
        
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="type">验证类型</param>
        /// <param name="mobile">手机号</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>string 验证码</returns>
        public string NewCode(int type, string mobile, int time)
        {
            var record = SmsCodes.OrderByDescending(r => r.CreateTime).FirstOrDefault(r => r.Mobile == mobile && r.Type == type);
            if (record != null && (DateTime.Now - record.CreateTime).TotalSeconds < 60) return null;

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
            return code;
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>bool 是否正确</returns>
        public bool VerifyCode(string mobile, string code, int type, bool remove)
        {
            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null) return false;

            if (!remove) return true;

            return SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type) > 0;
        }

        #endregion

        #region 登录和验证

        /// <summary>
        /// 获取用户登录结果
        /// </summary>
        /// <param name="obj">Session对象实体</param>
        /// <returns>Session对象实体</returns>
        public Session UserLogin(Session obj)
        {
            if (obj == null) return null;

            var session = GetSession(obj);
            if (session.OnlineStatus && session.MachineId == obj.MachineId)
            {
                // 当前已登录或未正常退出，标记为在线
                obj.LoginResult = LoginResult.Online;
                return obj;
            }

            obj.ID = session.ID;
            var verify = new Verify(obj);
            var result = verify.ReturnSession();
            if (result.LoginResult.GetHashCode() > 1) return result;

            session.DeptId = obj.DeptId;
            session.DeptName = obj.DeptName;
            session.Version = obj.Version;
            session.ClientType = obj.ClientType;
            session.MachineId = obj.MachineId;
            return session;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>bool 是否成功</returns>
        public bool Authorization(Session obj, string action)
        {
            Guid actionId;
            if (!Guid.TryParse(action, out actionId)) return false;

            return SimpleVerification(obj) && Authority(obj, actionId);
        }

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>Session 用户会话信息</returns>
        public Session Verification(Session obj)
        {
            if (obj == null) return null;

            var verify = new Verify(obj);
            return verify.ReturnSession();
        }

        /// <summary>
        /// 简单会话合法性验证
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>bool 是否成功</returns>
        public bool SimpleVerification(Session obj)
        {
            if (obj == null) return false;

            var verify = new Verify(obj);
            return verify.ReturnBoolean();
        }

        #endregion

        #region Session的操作方法

        /// <summary>
        /// 获取当前在线状态的全部内部用户的Session
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>全部内部用户的Session</returns>
        public List<Session> GetSessions(Session obj)
        {
            return !SimpleVerification(obj) ? null : Sessions.Where(s => s.UserType > 0 && s.OnlineStatus).ToList();
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="id">用户ID</param>
        /// <param name="pw">用户新密码</param>
        /// <returns>bool 是否成功</returns>
        public bool UpdateSignature(Session obj, Guid id, string pw)
        {
            if (!SimpleVerification(obj)) return false;

            using (var context = new WSEntities())
            {
                // 更新数据库
                var user = context.SYS_User.SingleOrDefault(u => u.ID == id);
                if (user == null) return false;

                if (user.Password == pw) return true;

                user.Password = pw;
                if (context.SaveChanges() <= 0) return false;

                // 更新缓存
                var session = Sessions.SingleOrDefault(s => s.UserId == id);
                if (session != null) session.Signature = Hash(user.LoginName.ToUpper() + pw);

                return true;
            }
        }

        /// <summary>
        /// 根据用户ID更新用户信息
        /// </summary>
        /// <param name="obj">操作员的Session</param>
        /// <param name="id">用户ID</param>
        /// <returns>bool 是否成功</returns>
        public bool UpdateUserInfo(Session obj, Guid id)
        {
            if (!SimpleVerification(obj)) return false;

            var session = Sessions.SingleOrDefault(s => s.UserId == id);
            if (session == null) return true;

            var user = GetUser(id);
            if (user == null) return false;

            session.LoginName = user.LoginName;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.OpenId = user.OpenId;
            return true;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="account">用户账号</param>
        /// <returns>bool 是否成功</returns>
        public bool SetUserOffline(Session obj, string account)
        {
            if (!SimpleVerification(obj)) return false;

            var session = Sessions.SingleOrDefault(s => s.LoginName == account);
            if (session != null) session.OnlineStatus = false;

            return true;
        }

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="obj">操作员的Session</param>
        /// <param name="uid">用户ID</param>
        /// <param name="validity">可用状态</param>
        /// <returns>bool 是否成功</returns>
        public bool SetUserStatus(Session obj, Guid uid, bool validity)
        {
            if (!SimpleVerification(obj)) return false;

            var session = Sessions.SingleOrDefault(s => s.UserId == uid);
            if (session != null) session.Validity = validity;

            return true;
        }

        #endregion

    }
}
