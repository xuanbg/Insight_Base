using System;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    public partial class Logs
    {
        /// <summary>
        /// 保存日志规则到数据库
        /// </summary>
        private bool Insert(SYS_Logs_Rules rule)
        {
            using (var context = new BaseEntities())
            {
                context.SYS_Logs_Rules.Add(rule);
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300601");
                    log.Write();
                    return false;
                }
                Parameters.Rules.Add(rule);
                return true;
            }
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        private bool DeleteRule(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var rule = context.SYS_Logs_Rules.SingleOrDefault(r => r.ID == id);
                if (rule == null) return false;

                context.SYS_Logs_Rules.Remove(rule);
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300602");
                    log.Write();
                    return false;
                }

                Parameters.Rules.RemoveAll(r => r.ID == id);
                return true;
            }
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        private bool Update(SYS_Logs_Rules rule)
        {
            using (var context = new BaseEntities())
            {
                var data = context.SYS_Logs_Rules.SingleOrDefault(r => r.ID == rule.ID);
                if (data == null) return false;

                data.ToDataBase = rule.ToDataBase;
                data.Code = rule.Code;
                data.Level = rule.Level;
                data.Source = rule.Source;
                data.Action = rule.Action;
                data.Message = rule.Message;
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300603");
                    log.Write();
                    return false;
                }
            }

            Parameters.Rules.RemoveAll(r => r.ID == rule.ID);
            Parameters.Rules.Add(rule);
            return true;
        }
    }
}
