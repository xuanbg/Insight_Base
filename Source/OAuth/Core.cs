using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.OAuth
{
    public static class Core
    {
        // Account缓存
        private static readonly Dictionary<string, Guid> _Accounts = new Dictionary<string, Guid>();

        // Session缓存
        private static readonly Dictionary<Guid, Session> _Sessions = new Dictionary<Guid, Session>();

        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        /// <summary>
        /// Token过期时间（小时）
        /// </summary>
        public static int Expired { get; } = int.Parse(Util.GetAppSetting("Expired"));

        /// <summary>
        /// 在账号缓存中获取UserID
        /// </summary>
        /// <param name="key">登录账号</param>
        /// <param name="initSession">是否初始化Session，默认为是</param>
        /// <returns>Guid? UserID(未查询到用户数据时返回Null)</returns>
        public static Guid? GetUserId(string key, bool initSession = true)
        {
            if (_Accounts.ContainsKey(key)) return _Accounts[key];

            if (!initSession) return null;

            _Mutex.WaitOne();
            if (_Accounts.ContainsKey(key))
            {
                _Mutex.ReleaseMutex();
                return _Accounts[key];
            }

            var user = GetUser(key);
            if (user == null)
            {
                _Mutex.ReleaseMutex();
                return null;
            }

            _Accounts.Add(key, user.ID);
            if (_Sessions.ContainsKey(user.ID))
            {
                _Mutex.ReleaseMutex();
                return user.ID;
            }

            var session = new Session(user);
            _Sessions.Add(user.ID, session);

            _Mutex.ReleaseMutex();
            return user.ID;
        }

        /// <summary>
        /// 根据key(UserID)在缓存中查找Session
        /// </summary>
        /// <param name="uid">UserID</param>
        /// <returns>Session(可能为null)</returns>
        public static Session GetSession(Guid uid)
        {
            return _Sessions.ContainsKey(uid) ? _Sessions[uid] : null;
        }

        /// <summary>
        /// 根据登录账号获取用户实体
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>用户实体</returns>
        private static SYS_User GetUser(string account)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(u => u.LoginName == account || u.Mobile == account);
            }
        }
    }
}