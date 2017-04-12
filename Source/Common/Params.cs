using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.Common
{
    public static class Params
    {
        private static List<SYS_Logs_Rules> _Rules;
        public static bool LockAccount = bool.Parse(Util.GetAppSetting("LockAccount"));
        public static bool SignOut = bool.Parse(Util.GetAppSetting("SignOut"));

        /// <summary>
        /// 进程同步基元
        /// </summary>
        public static readonly Mutex Mutex = new Mutex();

        /// <summary>
        /// 规则缓存
        /// </summary>
        public static List<SYS_Logs_Rules> Rules
        {
            get
            {
                if (_Rules != null) return _Rules;

                using (var context = new BaseEntities())
                {
                    _Rules = context.SYS_Logs_Rules.ToList();
                }
                return _Rules;
            }
        }

        /// <summary>
        /// 短信验证码的缓存列表
        /// </summary>
        public static readonly List<VerifyRecord> SmsCodes = new List<VerifyRecord>();

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
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>bool 验证码是否正确</returns>
        public static bool VerifySmsCode(string mobile, string code, int type, bool remove = true)
        {
            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null) return false;

            if (remove) SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type);

            return true;
        }
    }
}