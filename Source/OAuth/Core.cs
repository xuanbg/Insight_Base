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
        private static readonly Dictionary<string, string> _Accounts = new Dictionary<string, string>();

        // Session缓存
        private static readonly Dictionary<string, Session> _Sessions = new Dictionary<string, Session>();

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
        public static string GetUserId(string key, bool initSession = true)
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

            _Accounts.Add(key, user.id);
            if (_Sessions.ContainsKey(user.id))
            {
                _Mutex.ReleaseMutex();
                return user.id;
            }

            var session = new Session(user);
            _Sessions.Add(user.id, session);

            _Mutex.ReleaseMutex();
            return user.id;
        }

        /// <summary>
        /// 根据key(UserID)在缓存中查找Session
        /// </summary>
        /// <param name="userId">UserID</param>
        /// <returns>Session(可能为null)</returns>
        public static Session GetSession(string userId)
        {
            return _Sessions.ContainsKey(userId) ? _Sessions[userId] : null;
        }

        /// <summary>
        /// 根据登录账号获取用户实体
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>用户实体</returns>
        private static User GetUser(string account)
        {
            using (var context = new BaseEntities())
            {
                return context.users.SingleOrDefault(u => u.account == account || u.mobile == account || u.email == account);
            }
        }
    }
}