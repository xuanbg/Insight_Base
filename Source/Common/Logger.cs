using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Common
{
    public static class Logger
    {
        /// <summary>
        /// 构造SYS_Logs数据并写入
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        /// <param name="source">事件来源</param>
        /// <param name="action">操作名称</param>
        /// <param name="key">查询关键字</param>
        /// <param name="userId">事件源用户ID</param>
        /// <returns>bool 是否写入成功</returns>
        public static bool? write(string code, string message = null, string source = null, string action = null,
            string key = null, string userId = null)
        {
            if (string.IsNullOrEmpty(code) || !Regex.IsMatch(code, @"^\d{6}$")) return null;

            var level = Convert.ToInt32(code.Substring(0, 1));
            var rule = Params.rules.SingleOrDefault(r => r.code == code);
            if (level > 1 && level < 7 && rule == null) return null;

            var log = new Log
            {
                id = Util.newId(),
                code = code,
                level = level,
                source = rule?.source ?? source,
                action = rule?.action ?? action,
                message = string.IsNullOrEmpty(message) ? rule?.message : message,
                key = key,
                userId = userId,
                createTime = DateTime.Now
            };

            return (rule?.isFile ?? true) ? writeToFile(log) : writeToDb(log);
        }

        /// <summary>
        /// 将日志写入数据库
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        private static bool writeToDb(Log log)
        {
            using (var context = new Entities())
            {
                context.logs.Add(log);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        private static bool writeToFile(Log log)
        {
            Params.mutex.WaitOne();
            var path = $"{Util.getAppSetting("LogLocal")}\\{getLevelName(log.level)}\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += $"{DateTime.Today:yyyy-MM-dd}.log";
            var time = log.createTime.ToString("O");
            var text =
                $"[{log.createTime.Kind} {time}] [{log.code}] [{log.source}] [{log.action}] Message:{log.message}\r\n";
            var buffer = Encoding.UTF8.GetBytes(text);
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }

                Params.mutex.ReleaseMutex();
                return true;
            }
            catch (Exception)
            {
                Params.mutex.ReleaseMutex();
                return false;
            }
        }

        /// <summary>
        /// 获取事件等级名称
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static string getLevelName(int level)
        {
            var name = (Level) level;
            return name.ToString();
        }
    }
}