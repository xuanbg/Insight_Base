using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Logs : ServiceBase, ILogs
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="code">事件代码（必须有）</param>
        /// <param name="message">事件消息，为空则使用默认消息文本</param>
        /// <param name="source">来源（可为空）</param>
        /// <param name="action">操作（可为空）</param>
        /// <param name="key">查询用的关键字段</param>
        /// <param name="userid">事件源用户ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> WriteToLog(string code, string message, string source, string action, string key, string userid)
        {
            if (!Verify()) return result;

            new Thread(() => Logger.Write(code, message, source, action, key, userid)).Start();

            return result;
        }

        /// <summary>
        /// 新增日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>Result</returns>
        public Result<object> AddRule(LogRule rule)
        {
            if (!Verify("60A97A33-0E6E-4856-BB2B-322FEEEFD96A")) return result;

            if (string.IsNullOrEmpty(rule.code) || !Regex.IsMatch(rule.code, @"^\d{6}$")) return result.InvalidEventCode();

            var level = Convert.ToInt32(rule.code.Substring(0, 1));
            if (level <= 1 || level == 7) return result.EventWithoutConfig();

            if (Params.rules.Any(r => r.code == rule.code)) return result.EventCodeUsed();

            rule.creatorId = userId;
            if (!Insert(rule)) return result.DataBaseError();

            var log = new
            {
                UserID = userId,
                Message = $"事件代码【{rule.code}】已由{userName}创建和配置为：{Util.Serialize(rule)}"
            };
            new Thread(() => Logger.Write("600601", Util.Serialize(log))).Start();

            return result;
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveRule(string id)
        {
            if (!Verify("BBC43098-A030-46CA-A681-0C3D1ECC15AB")) return result;

            if (!DeleteRule(id)) return result.DataBaseError();

            var log = new
            {
                UserID = userId,
                Message = $"事件配置【{id}】已被{userName}删除"
            };
            new Thread(() => Logger.Write("600602", Util.Serialize(log))).Start();

            return result;
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>Result</returns>
        public Result<object> EditRule(LogRule rule)
        {
            if (!Verify("9FF1547D-2E3F-4552-963F-5EA790D586EA")) return result;

            if (!Update(rule)) return result.DataBaseError();

            var log = new
            {
                UserID = userId,
                Message = $"事件代码【{rule.code}】已被{userName}修改为：{Util.Serialize(rule)}"
            };
            new Thread(() => Logger.Write("600603", Util.Serialize(log))).Start();

            return result;
        }

        /// <summary>
        /// 获取日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>Result</returns>
        public Result<object> GetRule(string id)
        {
            if (!Verify("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F")) return result;

            var rule = Params.rules.SingleOrDefault(r => r.id == id);
            return rule == null ? result.NotFound() : result.Success(rule);
        }

        /// <summary>
        /// 获取全部日志规则
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetRules()
        {
            if (!Verify("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F")) return result;

            return Params.rules.Any() ? result.Success(Params.rules) : result.NoContent(new List<object>());
        }
    }
}