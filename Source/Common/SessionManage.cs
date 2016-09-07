using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Base.Common.Entity;

namespace Insight.Base.Common
{
    public static class SessionManage
    {
        /// <summary>
        /// Token缓存
        /// </summary>
        private static readonly List<Session> Sessions = new List<Session>();

        /// <summary>
        /// 进程同步基元
        /// </summary>
        private static readonly Mutex Mutex = new Mutex();

        /// <summary>
        /// 获取指定类型的所有在线用户
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Session> GetOnlineUsers(int type)
        {
            return Sessions.Where(s => s.UserType == type && s.OnlineStatus).ToList();
        }

        /// <summary>
        /// 根据登录账号在缓存中查找Session
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Session(可能为null)</returns>
        public static Session Get(string account)
        {
            return Sessions.SingleOrDefault(s => Util.StringCompare(s.Account, account));
        }

        /// <summary>
        /// 根据AccessToken获取Session
        /// </summary>
        /// <param name="token">AccessToken</param>
        /// <returns>Session</returns>
        public static Session Get(AccessToken token)
        {
            var fast = token.ID < Sessions.Count && Util.StringCompare(token.Account, Sessions[token.ID].Account);
            return fast ? Sessions[token.ID] : Find(token);
        }

        /// <summary>
        /// 在缓存中查找Token并返回
        /// </summary>
        /// <param name="token">AccessToken</param>
        /// <returns>Session</returns>
        private static Session Find(AccessToken token)
        {
            Mutex.WaitOne();
            var session = Sessions.SingleOrDefault(s => Util.StringCompare(s.Account, token.Account)) ?? Add(token.Account);
            Mutex.ReleaseMutex();
            return session;
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息更新Session加入缓存并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Session</returns>
        private static Session Add(string account)
        {
            var user = DataAccess.GetUser(account);
            if (user == null) return null;

            var session = new Session
            {
                ID = Sessions.Count,
                UserType = user.Type,
                Account = user.LoginName,
                Mobile = user.Mobile,
                UserId = user.ID,
                UserName = user.Name,
                Validity = user.Validity,
            };
            session.Sign(user.Password);
            session.MakeStamp();
            Sessions.Add(session);
            return session;
        }
    }
}
