using System;
using System.Collections.Generic;
using System.Threading;
using Insight.Base.Common;
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
            if (_Sessions.ContainsKey(key)) return _Sessions[key];

            try
            {
                _Mutex.WaitOne();
                if (_Sessions.ContainsKey(key))
                {
                    _Mutex.ReleaseMutex();
                    return _Sessions[key];
                }

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
            catch (Exception ex)
            {
                new Logger("000000", $"Account is {account}, Exception:{ex.Message}", "OAuth", "GetSession").Write();
                _Mutex.ReleaseMutex();
                return null;
            }
        }
    }
}