using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public static class Common
    {
        // Token缓存
        private static readonly List<Session> _Sessions = new List<Session>();

        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        /// <summary>
        /// 最大授权数
        /// </summary>
        public static int MaxAuth { get; } = 999999999;

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
            return _Sessions.SingleOrDefault(s => Util.StringCompare(s.Account, account));
        }

        /// <summary>
        /// 根据AccessToken获取Session
        /// </summary>
        /// <param name="token">AccessToken</param>
        /// <returns>Session</returns>
        public static Session GetSession(AccessToken token)
        {
            var fast = token.ID < _Sessions.Count && Util.StringCompare(token.Account, _Sessions[token.ID].Account);
            return fast ? _Sessions[token.ID] : FindSession(token);
        }

        /// <summary>
        /// 在缓存中查找Token并返回
        /// </summary>
        /// <param name="token">AccessToken</param>
        /// <returns>Session</returns>
        private static Session FindSession(AccessToken token)
        {
            _Mutex.WaitOne();
            var session = _Sessions.SingleOrDefault(s => Util.StringCompare(s.Account, token.Account)) ?? AddSession(token.Account);
            _Mutex.ReleaseMutex();
            return session;
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息更新Session加入缓存并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Session</returns>
        private static Session AddSession(string account)
        {
            var session = new Session(account, _Sessions.Count);
            if (session.Stamp == null) return null;

            _Sessions.Add(session);
            return session;
        }
    }
}
