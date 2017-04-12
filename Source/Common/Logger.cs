using System;
using System.Linq;
using System.Text.RegularExpressions;
using Insight.Base.Common.Entity;

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

            return (rule?.ToDataBase ?? false) ? DataAccess.WriteToDB(log) : DataAccess.WriteToFile(log);
        }
    }
}