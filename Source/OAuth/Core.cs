using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Redis;

namespace Insight.Base.OAuth
{
    public static class Core
    {
        // 进程同步基元
        private static readonly Mutex mutex = new Mutex();

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

            mutex.WaitOne();
            userId = RedisHelper.StringGet(account);
            if (!string.IsNullOrEmpty(userId))
            {
                mutex.ReleaseMutex();
                return userId;
            }

            var user = GetUser(account);
            if (user == null)
            {
                mutex.ReleaseMutex();
                return null;
            }

            // 缓存用户ID到Redis
            userId = user.id;
            key = "ID:" + user.account;
            RedisHelper.StringSet(key, userId);

            if (!string.IsNullOrEmpty(user.mobile))
            {
                key = "ID:" + user.mobile;
                RedisHelper.StringSet(key, userId);
            }

            if (!string.IsNullOrEmpty(user.email))
            {
                key = "ID:" + user.email;
                RedisHelper.StringSet(key, userId);
            }

            var token = new Token(user);
            SetTokenCache(token);
            mutex.ReleaseMutex();

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
            var life = 5;
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
            RedisHelper.StringSet(signature, code, TimeSpan.FromSeconds(life));
            RedisHelper.StringSet(code, token.userId, TimeSpan.FromSeconds(life));

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
            Logger.Write("600501", msg);
            var key = Util.Hash(type + mobile + code);
            if (type == 4) return code;

            RedisHelper.StringSet(key, code, TimeSpan.FromMinutes(life));
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
        /// 用户是否存在
        /// </summary>
        /// <param name="user">User数据</param>
        /// <returns>用户是否存在</returns>
        public static bool IsExisted(User user)
        {
            using (var context = new Entities())
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
            if (!token.IsChanged()) return;

            var key = "Token:" + token.userId;
            var json = Util.Serialize(token);
            RedisHelper.StringSet(key, json);
        }

        /// <summary>
        /// 根据用户ID获取用户实体
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户实体</returns>
        public static User GetUserById(string userId)
        {
            using (var context = new Entities())
            {
                return context.users.SingleOrDefault(u => u.id == userId);
            }
        }

        /// <summary>
        /// 获取用户的全部已授权功能集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <returns>功能集合</returns>
        public static List<Permit> GetPermits(string tenantId, string userId, string deptId)
        {
            using (var context = new Entities())
            {
                var list = from f in context.functions
                    join p in context.roleFunctions on f.id equals p.functionId
                    join r in context.userRoles on p.roleId equals r.roleId
                    where r.tenantId == tenantId && r.userId == userId && (r.deptId == null || r.deptId == deptId)
                    group p by new {f.id, f.navigatorId, f.alias}
                    into g
                    let k = g.Key
                    select new Permit { id = k.id, parentId = k.navigatorId, alias = k.alias, permit = g.Min(i => i.permit)};
                return list.Where(i => i.permit > 0).ToList();
            }
        }

        /// <summary>
        /// 根据登录账号获取用户实体
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <returns>用户实体</returns>
        private static User GetUser(string account)
        {
            using (var context = new Entities())
            {
                return context.users.SingleOrDefault(u => u.account == account || u.mobile == account || u.email == account);
            }
        }
    }
}