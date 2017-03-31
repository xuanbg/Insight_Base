using System;
using System.Collections.Generic;
using System.Threading;
using Insight.Utils.Common;

namespace Insight.Base.OAuth
{
    public static class Common
    {
        // Token缓存
        private static readonly Dictionary<string, Session> _Sessions = new Dictionary<string, Session>();

        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        /// <summary>
        /// Token过期时间（小时）
        /// </summary>
        public static int Expired { get; } = int.Parse(Util.GetAppSetting("Expired"));

        /// <summary>
        /// 根据登录账号在缓存中查找Session
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Session(可能为null)</returns>
        public static Session GetSession(string account)
        {
            var key = account.ToUpper();
            try
            {
                return _Sessions[key];
            }
            catch
            {
                _Mutex.WaitOne();
                var session = new Session(key);
                if (session.id == Guid.Empty)
                {
                    _Mutex.ReleaseMutex();
                    return null;
                }

                _Sessions.Add(key, session);
                _Mutex.ReleaseMutex();
                return session;
            }
        }
    }
}