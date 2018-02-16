using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Server;
using StackExchange.Redis;

namespace Insight.Base.Common
{
    public static class Params
    {
        private static List<LogRule> _Rules;
        private static readonly string _RedisConn = Util.GetAppSetting("Redis");

        // 日志进程同步基元
        public static readonly Mutex Mutex = new Mutex();

        // Redis链接对象
        public static readonly ConnectionMultiplexer Redis = ConnectionMultiplexer.Connect(_RedisConn);

        // 访问管理器
        public static CallManage CallManage = new CallManage(Redis);

        // RSA私钥
        public static string RSAKey = Util.Base64Decode(Util.GetAppSetting("RSAKey"));

        /// <summary>
        /// 规则缓存
        /// </summary>
        public static List<LogRule> Rules
        {
            get
            {
                if (_Rules != null) return _Rules;

                using (var context = new BaseEntities())
                {
                    _Rules = context.logRules.ToList();
                }
                return _Rules;
            }
        }

        /// <summary>
        /// 用于生成短信验证码的随机数发生器
        /// </summary>
        public static readonly Random Random = new Random(Environment.TickCount);

        /// <summary>
        /// 日志接口URL
        /// </summary>
        public static string LogUrl;

        /// <summary>
        /// 数据库连结字符串
        /// </summary>
        public static string Database = new BaseEntities().Database.Connection.ConnectionString;

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="key">验证码类型 + 手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>bool 验证码是否正确</returns>
        public static bool VerifySmsCode(string key, string code, bool remove = true)
        {
            var db = Redis.GetDatabase();
            var result = db.SetContains(key, code);

            if (result && remove) db.KeyDelete(key);

            return result;
        }
    }
}