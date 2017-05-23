using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.OAuth
{
    public static class Core
    {
        // Session缓存
        private static readonly Dictionary<Guid, Session> _Sessions = new Dictionary<Guid, Session>();

        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        /// <summary>
        /// Token过期时间（小时）
        /// </summary>
        public static int Expired { get; } = int.Parse(Util.GetAppSetting("Expired"));

        /// <summary>
        /// 根据登录账号获取用户ID
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Guid 用户ID</returns>
        public static Guid? GetUserId(string account)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(u => u.LoginName == account || u.Mobile == account)?.ID;
            }
        }

        /// <summary>
        /// 根据key(UserID)在缓存中查找Session
        /// </summary>
        /// <param name="uid">UserID</param>
        /// <returns>Session(可能为null)</returns>
        public static Session GetSession(Guid uid)
        {
            return _Sessions.ContainsKey(uid) ? _Sessions[uid] : FindSession(uid);
        }

        /// <summary>
        /// 根据用户ID查询数据库构建Session并缓存
        /// </summary>
        /// <param name="uid">UserID</param>
        /// <returns>Session(可能为null)</returns>
        internal static Session FindSession(Guid uid)
        {
            try
            {
                _Mutex.WaitOne();
                if (_Sessions.ContainsKey(uid))
                {
                    _Mutex.ReleaseMutex();
                    return _Sessions[uid];
                }

                var session = new Session(uid);
                if (session.id == Guid.Empty)
                {
                    _Mutex.ReleaseMutex();
                    return null;
                }

                _Sessions.Add(uid, session);
                _Mutex.ReleaseMutex();
                return session;
            }
            catch (Exception ex)
            {
                new Logger("000000", $"UserId is {uid}, Exception:{ex.Message}", "OAuth", "GetSession").Write();
                _Mutex.ReleaseMutex();
                return null;
            }
        }
    }
}