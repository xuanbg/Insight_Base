using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Common
{
    public static class TokenManage
    {
        /// <summary>
        /// Token缓存
        /// </summary>
        private static readonly List<Session> Tokens;

        /// <summary>
        /// 进程同步基元
        /// </summary>
        private static readonly Mutex Mutex = new Mutex();

        /// <summary>
        /// 构造方法，初始化Token缓存
        /// </summary>
        static TokenManage()
        {
            Tokens = new List<Session>();
        }

        /// <summary>
        /// 获取指定类型的所有在线用户
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Session> GetOnlineUsers(int type)
        {
            return Tokens.Where(s => s.UserType == type && s.OnlineStatus).ToList();
        }

        /// <summary>
        /// 根据传入的用户数据更新Token
        /// </summary>
        /// <param name="user">SYS_User</param>
        public static void Update(SYS_User user)
        {
            var token = Get(user.LoginName);
            if (token == null) return;

            token.UserName = user.Name;
            token.UserType = user.Type;
        }

        /// <summary>
        /// 设置指定账号的登录状态为离线
        /// </summary>
        /// <param name="account">登录账号</param>
        public static void Offline(string account)
        {
            var token = Get(account);
            if (token == null) return;

            token.OnlineStatus = false;
        }

        /// <summary>
        /// 设置指定账号的Validity状态
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="validity">bool 是否有效</param>
        /// <returns>Session</returns>
        public static void SetValidity(string account, bool validity)
        {
            var token = Get(account);
            if (token == null) return;

            token.Validity = validity;
        }

        /// <summary>
        /// 根据登录账号在缓存中查找Session并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>Token</returns>
        public static Session Get(string account)
        {
            return Tokens.SingleOrDefault(s => Util.StringCompare(s.Account, account));
        }

        /// <summary>
        /// 根据SessionID获取缓存中的Token并返回
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token</returns>
        public static Session Get(AccessToken token)
        {
            var fast = token.ID < Tokens.Count && Util.StringCompare(token.Account, Tokens[token.ID].Account);
            return fast ? Tokens[token.ID] : Find(token);
        }

        /// <summary>
        /// 生成用于验证的Key
        /// </summary>
        /// <param name="session">Session</param>
        /// <returns>string 用于验证的Key</returns>
        public static string CreateKey(Session session)
        {
            var token = new AccessToken
            {
                ID = session.ID,
                UserType = session.UserType,
                OpenId = session.OpenId,
                Account = session.Account,
                Signature = session.Signature,
                UserId = session.UserId,
                UserName = session.UserName,
                DeptId = session.DeptId,
                DeptName = session.DeptName,
                MachineId = session.MachineId,
                Secret = session.Secret,
                FailureTime = session.FailureTime
            };
            return Util.Base64(token);
        }

        /// <summary>
        /// 在缓存中查找Token并返回
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token</returns>
        private static Session Find(AccessToken token)
        {
            Mutex.WaitOne();
            var obj = Tokens.SingleOrDefault(s => Util.StringCompare(s.Account, token.Account)) ?? Add(token.Account);
            Mutex.ReleaseMutex();
            return obj;
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息更新Session加入缓存并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        private static Session Add(string account)
        {
            var user = DataAccess.GetUser(account);
            if (user == null) return null;

            var secret = Util.Hash(Guid.NewGuid().ToString());
            var expi = DateTime.Now.AddHours(user.Type == 0 ? 24 : Parameters.Expired);
            var token = new Session
            {
                ID = Tokens.Count,
                OpenId = user.OpenId,
                Account = user.LoginName,
                Signature = user.Password,
                UserId = user.ID,
                UserName = user.Name,
                UserType = user.Type,
                Validity = user.Validity,
                Secret = secret,
                FailureTime = expi
            };
            Tokens.Add(token);
            return token;
        }
    }
}
