using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// 进程同步基元
        /// </summary>
        public static readonly Mutex Mutex = new Mutex();

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
        public static void UpdateSession(SYS_User user)
        {
            var session = GetSession(user.LoginName);
            if (session == null) return;

            session.UserName = user.Name;
            session.UserType = user.Type;
        }

        /// <summary>
        /// 设置指定账号的登录状态为离线
        /// </summary>
        /// <param name="account">登录账号</param>
        public static void Offline(string account)
        {
            var session = GetSession(account);
            if (session == null) return;

            session.OnlineStatus = false;
        }

        /// <summary>
        /// 设置指定账号的Validity状态
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="validity">bool 是否有效</param>
        /// <returns>Session</returns>
        public static void SetValidity(string account, bool validity)
        {
            var session = GetSession(account);
            if (session == null) return;

            session.Validity = validity;
        }

        /// <summary>
        /// 根据登录账号在缓存中查找Session并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Session</returns>
        public static Session GetSession(string account)
        {
            return Sessions.SingleOrDefault(s => Util.StringCompare(s.LoginName, account));
        }

        /// <summary>
        /// 根据SessionID获取缓存中的Session并返回
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Session</returns>
        public static Session GetSession(Session session)
        {
            var fast = session.ID < Sessions.Count && session.SessionId == Sessions[session.ID].SessionId;
            return fast ? Sessions[session.ID] : FindSession(session);
        }

        /// <summary>
        /// 根据登录账号在缓存中查找Session并返回
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>Session</returns>
        private static Session FindSession(Session session)
        {
            Mutex.WaitOne();
            var obj = Sessions.SingleOrDefault(s => Util.StringCompare(s.LoginName, session.LoginName)) ?? AddSession(session);
            Mutex.ReleaseMutex();
            return obj;
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息更新Session加入缓存并返回
        /// </summary>
        /// <param name="session">Session</param>
        private static Session AddSession(Session session)
        {
            var user = DataAccess.GetUser(session.LoginName);
            if (user == null) return null;

            session.ID = Sessions.Count;
            session.SessionId = Guid.NewGuid();
            session.UserId = user.ID;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.Validity = user.Validity;
            session.Expired = DateTime.Now.AddHours(Util.Expired);
            Sessions.Add(session);
            return session;
        }

    }
}
