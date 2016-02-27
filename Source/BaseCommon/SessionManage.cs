using System;
using System.Collections.Generic;
using System.Linq;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Common
{
    public static class SessionManage
    {
        /// <summary>
        /// Session缓存
        /// </summary>
        private static readonly List<Session> Sessions;

        /// <summary>
        /// 构造方法，初始化Sessions
        /// </summary>
        static SessionManage()
        {
            Sessions = new List<Session>();
        }

        /// <summary>
        /// 获取指定类型的所有在线用户
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Session> GetSessions(int type)
        {
            return Sessions.Where(s => s.UserType == type && s.OnlineStatus).ToList();
        }

        /// <summary>
        /// 根据传入的用户数据更新Session
        /// </summary>
        /// <param name="user">SYS_User</param>
        /// <returns>Session</returns>
        public static Session UpdateSession(SYS_User user)
        {
            var session = GetSession(user.LoginName);
            if (session == null) return null;

            session.LoginName = user.LoginName;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.OpenId = user.OpenId;
            return session;
        }

        /// <summary>
        /// 根据登录账号更新Signature
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">登录密码</param>
        /// <returns>Session</returns>
        public static Session UpdateSignature(string account, string password)
        {
            var session = GetSession(account);
            var sign = Util.Hash(account.ToUpper() + password);
            if (session == null || session.Signature == sign) return session;

            session.Signature = sign;
            return session;
        }

        /// <summary>
        /// 设置指定账号的登录状态为离线
        /// </summary>
        /// <param name="account">登录账号</param>
        public static void Offline(string account)
        {
            var session = GetSession(account);
            if (session != null) session.OnlineStatus = false;
        }

        /// <summary>
        /// 设置指定账号的Validity状态
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="validity">bool 是否有效</param>
        /// <returns>Session</returns>
        public static Session SetValidity(string account, bool validity)
        {
            var session = GetSession(account);
            if (session != null) session.Validity = validity;

            return session;
        }

        /// <summary>
        /// 根据SessionID获取缓存中的Session并返回
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Session</returns>
        public static Session GetSession(Session session)
        {
            var fast = session.ID < Sessions.Count && string.Equals(session.LoginName, Sessions[session.ID].LoginName, StringComparison.CurrentCultureIgnoreCase);
            return fast ? Sessions[session.ID] : GetSession(session.LoginName);
        }

        /// <summary>
        /// 根据登录账号在缓存中查找Session并返回
        /// </summary>
        /// <param name="loginname">登录账号</param>
        /// <returns>Session</returns>
        private static Session GetSession(string loginname)
        {
            var list = Sessions.Where(s => string.Equals(s.LoginName, loginname, StringComparison.CurrentCultureIgnoreCase)).ToList();

            if (list.Count < 2) return list.Count == 0 ? AddSession(loginname) : list[0];

            Sessions.RemoveAll(s => s.LoginName == loginname);
            General.LogToLogServer("000000", $"用户【{loginname}】数据重复，已清除重复数据", "验证服务", "获取验证数据");
            return AddSession(loginname);
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息构造Session加入缓存并返回
        /// </summary>
        /// <param name="loginname">登录账号</param>
        private static Session AddSession(string loginname)
        {
            var user = DataAccess.GetUser(loginname);
            if (user == null) return null;

            var session = new Session
            {
                ID = Sessions.Count,
                UserId = user.ID,
                UserName = user.Name,
                OpenId = user.OpenId,
                LoginName = user.LoginName,
                Signature = Util.Hash(user.LoginName.ToUpper() + user.Password),
                UserType = user.Type,
                Validity = user.Validity,
            };
            Sessions.Add(session);
            return session;
        }

    }
}
