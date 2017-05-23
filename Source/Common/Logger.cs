using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.Common
{
    public class Logger
    {
        /// <summary>
        /// 事件代码
        /// </summary>
        private readonly string Code;

        /// <summary>
        /// 事件消息
        /// </summary>
        private readonly string Message;

        /// <summary>
        /// 事件源
        /// </summary>
        private readonly string Source;

        /// <summary>
        /// 事件名称
        /// </summary>
        private readonly string Action;

        /// <summary>
        /// 查询关键字
        /// </summary>
        private readonly string Key;

        /// <summary>
        /// 事件源用户ID
        /// </summary>
        private readonly Guid? UserId;

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        /// <param name="source">事件来源</param>
        /// <param name="action">操作名称</param>
        /// <param name="key">查询关键字</param>
        /// <param name="uid">事件源用户ID</param>
        /// <returns>bool 是否写入成功</returns>
        public Logger(string code, string message = null, string source = null, string action = null, string key = null, Guid? uid = null)
        {
            Code = code;
            Message = message;
            Source = source;
            Action = action;
            Key = key;
            UserId = uid;
        }

        /// <summary>
        /// 构造SYS_Logs数据并写入
        /// </summary>
        /// <returns>bool 是否写入成功</returns>
        public bool? Write()
        {
            if (string.IsNullOrEmpty(Code) || !Regex.IsMatch(Code, @"^\d{6}$")) return null;

            var level = Convert.ToInt32(Code.Substring(0, 1));
            var rule = Params.Rules.SingleOrDefault(r => r.Code == Code);
            if (level > 1 && level < 7 && rule == null) return null;

            var log = new SYS_Logs
            {
                ID = Guid.NewGuid(),
                Code = Code,
                Level = level,
                Source = rule?.Source ?? Source,
                Action = rule?.Action ?? Action,
                Message = string.IsNullOrEmpty(Message) ? rule?.Message : Message,
                Key = Key,
                SourceUserId = UserId,
                CreateTime = DateTime.Now
            };

            return (rule?.ToDataBase ?? false) ? WriteToDB(log) : WriteToFile(log);
        }

        /// <summary>
        /// 将日志写入数据库
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        public static bool WriteToDB(SYS_Logs log)
        {
            using (var context = new BaseEntities())
            {
                context.SYS_Logs.Add(log);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        public static bool WriteToFile(SYS_Logs log)
        {
            Params.Mutex.WaitOne();
            var path = $"{Util.GetAppSetting("LogLocal")}\\{GetLevelName(log.Level)}\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += $"{DateTime.Today:yyyy-MM-dd}.log";
            var time = log.CreateTime.ToString("O");
            var text = $"[{log.CreateTime.Kind} {time}] [{log.Code}] [{log.Source}] [{log.Action}] Message:{log.Message}\r\n";
            var buffer = Encoding.UTF8.GetBytes(text);
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
                Params.Mutex.ReleaseMutex();
                return true;
            }
            catch (Exception)
            {
                Params.Mutex.ReleaseMutex();
                return false;
            }
        }

        /// <summary>
        /// 获取事件等级名称
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static string GetLevelName(int level)
        {
            var name = (Level)level;
            return name.ToString();
        }
    }
}