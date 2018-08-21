using System.Linq;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    public partial class Logs
    {
        /// <summary>
        /// 保存日志规则到数据库
        /// </summary>
        private static bool insert(LogRule rule)
        {
            using (var context = new Entities())
            {
                context.logRules.Add(rule);
                if (context.SaveChanges() <= 0)
                {
                    new Thread(() => Logger.write("300601")).Start();

                    return false;
                }
                Params.rules.Add(rule);
                return true;
            }
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        private bool deleteRule(string id)
        {
            using (var context = new Entities())
            {
                var rule = context.logRules.SingleOrDefault(r => r.id == id);
                if (rule == null) return false;

                context.logRules.Remove(rule);
                if (context.SaveChanges() <= 0)
                {
                    new Thread(() => Logger.write("300602")).Start();

                    return false;
                }

                Params.rules.RemoveAll(r => r.id == id);
                return true;
            }
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        private bool update(LogRule rule)
        {
            using (var context = new Entities())
            {
                var data = context.logRules.SingleOrDefault(r => r.id == rule.id);
                if (data == null) return false;

                data.isFile = rule.isFile;
                data.code = rule.code;
                data.level = rule.level;
                data.source = rule.source;
                data.action = rule.action;
                data.message = rule.message;
                if (context.SaveChanges() <= 0)
                {
                    new Thread(() => Logger.write("300603")).Start();

                    return false;
                }
            }

            Params.rules.RemoveAll(r => r.id == rule.id);
            Params.rules.Add(rule);
            return true;
        }
    }
}
