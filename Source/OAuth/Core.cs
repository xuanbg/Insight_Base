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
            new Thread(() => Logger.Write("600501", msg)).Start();
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
                return context.users.Any(u => u.account == user.account 
                || !string.IsNullOrEmpty(user.mobile) && u.mobile == user.mobile
                || !string.IsNullOrEmpty(user.email) && u.email == user.email);
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
        /// 获取用户的全部已授权功能ID集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="getInfo">是否获取信息，默认为否</param>
        /// <returns>功能ID集合</returns>
        public static List<PermitFunc> GetPermitFuncs(string tenantId, string userId, string deptId,
            bool getInfo = false)
        {
            using (var context = new Entities())
            {
                var funcs = from f in context.roleFunctions
                    join r in context.userRoles on f.roleId equals r.roleId
                    where r.tenantId == tenantId && r.userId == userId &&
                          (getInfo || r.deptId == null || r.deptId == deptId)
                    group f by f.functionId
                    into g
                    select new PermitFunc {id = g.Key, permit = g.Min(i => i.permit)};

                return funcs.ToList();
            }
        }

        /// <summary>
        /// 获取用户的全部已授权功能ID集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="getInfo">是否获取信息，默认为否</param>
        /// <returns>数据ID集合</returns>
        public static List<PermitData> GetPermitDatas(string tenantId, string userId, string deptId,
            bool getInfo = false)
        {
            using (var context = new Entities())
            {
                var datas = from d in context.roleDatas
                    join r in context.userRoles on d.roleId equals r.roleId
                    where r.tenantId == tenantId && r.userId == userId &&
                          (getInfo || r.deptId == null || r.deptId == deptId)
                    group d by new {d.modeId, d.moduleId, d.mode}
                    into g
                    select new PermitData
                    {
                        id = g.Key.modeId,
                        parentId = g.Key.moduleId,
                        mode = g.Key.mode,
                        permit = g.Max(i => i.permit)
                    };

                return datas.ToList();
            }
        }

        /// <summary>
        /// 获取用户的全部可用导航集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="appId">应用ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <returns>导航集合</returns>
        public static List<Permit> GetNavigation(string tenantId, string appId, string userId, string deptId)
        {
            var permits = GetPermitFuncs(tenantId, userId, deptId).Where(i => i.permit > 0);
            using (var context = new Entities())
            {
                var navigators = context.navigators.Where(i => i.appId == appId).ToList();
                var functions = context.functions.Where(i => i.isVisible).ToList();
                var mids = functions.Join(permits, f => f.id, p => p.id, (f, p) => f.navigatorId).ToList();
                var gids = from n in navigators
                    join m in mids on n.id equals m
                    select n.parentId;
                var ids = gids.Union(mids).ToList();
                var list = from n in navigators
                    join id in ids on n.id equals id
                    orderby n.parentId, n.index
                    select new Permit
                    {
                        id = n.id,
                        parentId = n.parentId,
                        index = n.index,
                        name = n.name,
                        alias = n.alias,
                        filePath = n.filePath,
                        icon = n.icon,
                        isDefault = n.isDefault
                    };

                return list.ToList();
            }
        }

        /// <summary>
        /// 获取指定模块的用户已授权功能集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="moduleId">模块ID</param>
        /// <returns>功能集合</returns>
        public static List<Permit> GetFunctions(string tenantId, string userId, string deptId, string moduleId)
        {
            var permits = GetPermitFuncs(tenantId, userId, deptId).Where(i => i.permit > 0);
            using (var context = new Entities())
            {
                var functions = context.functions.Where(i => i.isVisible && i.navigatorId == moduleId).ToList();
                var list = from f in functions
                    join p in permits on f.id equals p.id
                    orderby f.index
                    select new Permit
                    {
                        id = f.id,
                        index = f.index,
                        name = f.name,
                        alias = f.alias,
                        icon = f.icon,
                        isBegin = f.isBegin,
                        isShowText = f.isShowText,
                        permit = true
                    };

                return list.ToList();
            }
        }

        /// <summary>
        /// 获取用户的应用功能树
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>应用功能树</returns>
        public static List<AppTree> GetPermitAppTree(string tenantId, string userId)
        {
            var permits = GetPermitFuncs(tenantId, userId, null, true);
            using (var context = new Entities())
            {
                var funList = context.functions.ToList();
                var navList = context.navigators.ToList();
                var appList = context.applications.ToList();
                var functions = from f in funList
                    join p in permits on f.id equals p.id
                    select new AppTree
                    {
                        id = f.id,
                        parentId = f.navigatorId,
                        index = f.index,
                        nodeType = p.permit + 3,
                        name = f.name,
                        remark = p.permit == 1 ? "允许" : "拒绝"
                    };
                var mids = funList.Join(permits, f => f.id, p => p.id, (f, p) => f.navigatorId).Distinct().ToList();
                var modules = from n in navList
                    join p in mids on n.id equals p
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.parentId,
                        index = n.index,
                        nodeType = 2,
                        name = n.name
                    };
                var gids = navList.Join(mids, f => f.id, p => p, (f, p) => f.parentId).Distinct().ToList();
                var groups = from n in navList
                    join p in gids on n.id equals p
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.appId,
                        index = n.index,
                        nodeType = 1,
                        name = n.name
                    };
                var aids = navList.Join(gids, f => f.id, p => p, (f, p) => f.appId).Distinct().ToList();
                var apps = from a in appList
                    join p in aids on a.id equals p
                    select new AppTree {id = a.id, index = a.index, nodeType = 0, name = a.alias};
                var list = apps.Union(groups).Union(modules).Union(functions);

                return list.OrderBy(i => i.nodeType).ThenBy(i => i.parentId).ThenBy(i => i.index).ToList();
            }
        }

        /// <summary>
        /// 验证用户是否拥有指定功能的授权
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="key">操作码</param>
        /// <returns>bool 是否通过验证</returns>
        public static bool VerifyKey(string tenantId, string userId, string deptId, string key)
        {
            var permits = GetPermitFuncs(tenantId, userId, deptId).Where(i => i.permit > 0);
            using (var context = new Entities())
            {
                var list = from f in context.functions.ToList()
                    join p in permits on f.id equals p.id
                    select f.alias;

                return list.Any(i => i.Contains(key));
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
                return context.users.SingleOrDefault(u => u.account == account || u.mobile == account ||
                                                          u.email == account);
            }
        }
    }
}