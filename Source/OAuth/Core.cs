using System;
using System.Linq;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Server;

namespace Insight.Base.OAuth
{
    public static class Core
    {
        // 进程同步基元
        private static readonly Mutex Mutex = new Mutex();

        /// <summary>
        /// 根据用户登录账号获取Account缓存中的用户ID
        /// </summary>
        /// <param name="account">登录账号(账号、手机号、E-mail)</param>
        /// <returns>用户ID</returns>
        public static string GetUserId(string account)
        {
            var key = "ID:" + account;
            var userId = RedisHelper.StringGet(key);
            if (!string.IsNullOrEmpty(userId)) return userId;

            Mutex.WaitOne();
            userId = RedisHelper.StringGet(account);
            if (!string.IsNullOrEmpty(userId))
            {
                Mutex.ReleaseMutex();
                return userId;
            }

            var user = GetUser(account);
            if (user == null)
            {
                Mutex.ReleaseMutex();
                return null;
            }

            // 缓存用户ID到Redis
            key = "ID:" + user.account;
            RedisHelper.StringSet(key, user.id);

            if (!string.IsNullOrEmpty(user.mobile))
            {
                key = "ID:" + user.mobile;
                RedisHelper.StringSet(key, user.id);
            }

            if (!string.IsNullOrEmpty(user.email))
            {
                key = "ID:" + user.email;
                RedisHelper.StringSet(key, user.id);
            }

            var token = new Token(user);
            SetTokenCache(token);
            Mutex.ReleaseMutex();

            return userId;
        }

        /// <summary>
        /// 生成Code,缓存后返回
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="account">登录账号</param>
        /// <param name="type">登录类型(0:密码登录、1:验证码登录)</param>
        /// <returns>Code</returns>
        public static string GenerateCode(Token token, string account, int type)
        {
            string key;
            var life = 3;
            switch (type)
            {
                case 0:
                    key = Util.Hash(account + token.password);
                    break;
                case 1:
                    // 生成短信验证码(5分钟内有效)并发送
                    var mobile = token.mobile;
                    if (string.IsNullOrEmpty(mobile)) return null;

                    life = 60 * 5;
                    var smsCode = GenerateSmsCode(4, mobile, 5, 4);
                    key = Util.Hash(mobile + Util.Hash(smsCode));
                    break;
                default:
                    // Invalid type! You guess, you guess, you guess. (≧∇≦)
                    key = Util.NewId("N");
                    break;
            }

            var code = Util.NewId("N");
            var signature = Util.Hash(key + code);

            // 缓存签名-Code,以及Code-用户ID.
            RedisHelper.StringSet(signature, code, new TimeSpan(0, 0, life));
            RedisHelper.StringSet(code, token.userId);

            return code;
        }

        /// <summary>
        /// 生成短信验证码
        /// </summary>
        /// <param name="type">验证码类型(0:验证手机号;1:注册用户账号;2:重置密码;3:修改支付密码;4:登录验证码)</param>
        /// <param name="mobile">手机号</param>
        /// <param name="life">验证码有效时长(分钟)</param>
        /// <param name="length">验证码长度</param>
        /// <returns></returns>
        public static string GenerateSmsCode(int type, string mobile, int life, int length)
        {
            var max = Math.Pow(10, length);
            var code = Params.Random.Next(0, (int) max).ToString("D" + length);
            var msg = $"为手机号 {mobile} 生成了类型为 {type} 的验证码 {code}, 有效时间 {life} 分钟.";
            Logger.Write("600101", msg);
            var key = Util.Hash(type + mobile + code);
            if (type == 4) return code;

            RedisHelper.StringSet(key, code, new TimeSpan(0, life, 0));
            return code;
        }

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="message">短信内容</param>
        public static void SendMessage(string mobile, string message)
        {
        }

        /// <summary>
        /// 验证短信验证码
        /// </summary>
        /// <param name="type">验证码类型(0:验证手机号;1:注册用户账号;2:重置密码;3:修改支付密码)</param>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="isCheck">是否检验模式(true:检验模式,验证后验证码不失效;false:验证模式,验证后验证码失效)</param>
        /// <returns>是否通过验证</returns>
        public static bool VerifySmsCode(int type, string mobile, string code, bool isCheck = false)
        {
            var key = Util.Hash(type + mobile + code);
            var isExisted = RedisHelper.HasKey(key);
            if (!isExisted || isCheck) return isExisted;

            RedisHelper.Delete(key);
            return true;
        }

        /// <summary>
        /// 通过签名获取Code
        /// </summary>
        /// <param name="sign">签名</param>
        /// <returns>签名对应的Code</returns>
        public static string GetCode(string sign)
        {
            var code = RedisHelper.StringGet(sign);
            if (string.IsNullOrEmpty(code)) return null;

            RedisHelper.Delete(sign);
            return code;
        }

        /// <summary>
        /// 根据用户ID获取Token缓存中的Token
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>Token(可能为null)</returns>
        public static Token GetToken(string userId)
        {
            var key = "Token:" + userId;
            var json = RedisHelper.StringGet(key);
            return string.IsNullOrEmpty(json) ? null : Util.Deserialize<Token>(json);
        }

        /// <summary>
        /// 查询指定ID的应用的令牌生命周期小时数
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns>应用的令牌生命周期小时数</returns>
        public static int GetTokenLife(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return 24;

            // 从缓存读取应用的令牌生命周期
            const string fiele = "TokenLife";
            var val = RedisHelper.HashGet(appId, fiele);
            if (!string.IsNullOrEmpty(val)) return Convert.ToInt32(val);

            // 从数据库读取应用的令牌生命周期
            using (var context = new BaseEntities())
            {
                var hours = context.applications.SingleOrDefault(i => i.id == appId)?.tokenLife ?? 24;
                RedisHelper.HashSet(appId, fiele, hours.ToString());

                return hours;
            }
        }

        /// <summary>
        /// 用户是否存在
        /// </summary>
        /// <param name="user">User数据</param>
        /// <returns>用户是否存在</returns>
        public static bool IsExisted(User user)
        {
            using (var context = new BaseEntities())
            {
                return context.users.Any(u => u.account == user.account || u.mobile == user.mobile || u.email == user.email);
            }
        }

        /// <summary>
        /// 保存Token数据到缓存
        /// </summary>
        /// <param name="token">Token</param>
        public static void SetTokenCache(Token token)
        {
            if (!token.isChanged) return;

            var key = "Token:" + token.userId;
            var json = Util.Serialize(token);
            RedisHelper.StringSet(key, json);
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