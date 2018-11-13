using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Redis;

namespace Insight.Base.Common
{
    public static class Params
    {
        private static List<LogRule> ruleList;

        // 客户端文件信息集合
        public static readonly Dictionary<string, ClientFile> fileList = new Dictionary<string, ClientFile>();

        // 日志进程同步基元
        public static readonly Mutex mutex = new Mutex();

        // 每日每用户Token配额
        public static readonly int tokenLimit = Convert.ToInt32(Util.getAppSetting("TokenLimit"));

        // 访问管理器
        public static CallManage callManage { get; } = new CallManage();

        /// <summary>
        /// 规则缓存
        /// </summary>
        public static List<LogRule> rules
        {
            get
            {
                if (ruleList != null) return ruleList;

                using (var context = new Entities())
                {
                    ruleList = context.logRules.ToList();
                }

                return ruleList;
            }
        }

        /// <summary>
        /// 用于生成短信验证码的随机数发生器
        /// </summary>
        public static readonly Random random = new Random(Environment.TickCount);

        /// <summary>
        /// 日志接口URL
        /// </summary>
        public static string logUrl;
    }
}