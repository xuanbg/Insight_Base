using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Common
{
    public static class TokenManage
    {
        /// <summary>
        /// Token缓存
        /// </summary>
        private static readonly List<Token> Tokens;

        /// <summary>
        /// 进程同步基元
        /// </summary>
        private static readonly Mutex Mutex = new Mutex();

        /// <summary>
        /// 构造方法，初始化Token缓存
        /// </summary>
        static TokenManage()
        {
            Tokens = new List<Token>();
        }

        /// <summary>
        /// 获取指定类型的所有在线用户
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<Token> GetOnlineUsers(int type)
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
        public static Token Get(string account)
        {
            return Tokens.SingleOrDefault(s => Util.StringCompare(s.LoginName, account));
        }

        /// <summary>
        /// 根据SessionID获取缓存中的Token并返回
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token</returns>
        public static Token Get(Token token)
        {
            var fast = token.ID < Tokens.Count && Util.StringCompare(token.LoginName, Tokens[token.ID].LoginName);
            return fast ? Tokens[token.ID] : Find(token);
        }

        /// <summary>
        /// 在缓存中查找Token并返回
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token</returns>
        private static Token Find(Token token)
        {
            Mutex.WaitOne();
            var obj = Tokens.SingleOrDefault(s => Util.StringCompare(s.LoginName, token.LoginName)) ?? Add(token.LoginName);
            Mutex.ReleaseMutex();
            return obj;
        }

        /// <summary>
        /// 根据登录账号从数据库读取用户信息更新Session加入缓存并返回
        /// </summary>
        /// <param name="account">登录账号</param>
        private static Token Add(string account)
        {
            var user = DataAccess.GetUser(account);
            if (user == null) return null;

            var sign = Util.Hash(Guid.NewGuid().ToString());
            var expi = DateTime.Now.AddHours(user.Type == 0 ? 24 : Util.Expired);
            var token = new Token
            {
                ID = Tokens.Count,
                OpenId = user.OpenId,
                LoginName = user.LoginName,
                Signature = sign,
                UserId = user.ID,
                UserName = user.Name,
                UserType = user.Type,
                Validity = user.Validity,
                Expired = expi
            };
            Tokens.Add(token);
            return token;
        }

    }
}
