using System;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Rules : ServiceBase, IRules
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void responseOptions()
        {
        }

        /// <summary>
        /// 获取所有报表分期规则
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> getRules(int rows, int page)
        {
            if (!verify("getRules")) return result;

            if (page < 1 || rows > 100) return result.badRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.rules.Where(i => !i.isInvalid && (i.tenantId == null || i.tenantId == tenantId));
                var rules = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.success(rules, list.Count());
            }
        }

        /// <summary>
        /// 获取报表分期
        /// </summary>
        /// <param name="id">分期ID</param>
        /// <returns>Result</returns>
        public Result<object> getRule(string id)
        {
            if (!verify("getRules")) return result;

            var data = DbHelper.find<ReportRule>(id);

            return data == null ? result.notFound() : result.success(data);
        }

        /// <summary>
        /// 新建报表分期
        /// </summary>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> addRule(ReportRule rule)
        {
            if (!verify("newRule")) return result;

            rule.id = Util.newId();
            rule.tenantId = tenantId;
            rule.isBuiltin = false;
            rule.isInvalid = false;
            rule.creatorDeptId = deptId;
            rule.creator = userName;
            rule.creatorId = userId;
            rule.createTime = DateTime.Now;
            if (existed(rule)) return result.dataAlreadyExists();

            return DbHelper.insert(rule) ? result.created(rule) : result.dataBaseError();
        }

        /// <summary>
        /// 编辑报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> editRule(string id, ReportRule rule)
        {
            if (!verify("editRule")) return result;

            var data = DbHelper.find<ReportRule>(id);
            if (data == null) return result.notFound();

            if (data.isBuiltin) return result.notBeModified();

            data.cycleType = rule.cycleType;
            data.name = rule.name;
            data.cycle = rule.cycle;
            data.startTime = rule.startTime;
            data.remark = rule.remark;
            if (existed(data)) return result.dataAlreadyExists();

            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 删除报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteRule(string id)
        {
            if (!verify("deleteRule")) return result;

            var data = DbHelper.find<ReportRule>(id);
            if (data == null) return result.notFound();

            if (data.isBuiltin) return result.notBeDeleted();

            data.isInvalid = true;

            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 规则是否存在
        /// </summary>
        /// <param name="rule"></param>
        /// <returns>bool 是否存在</returns>
        public static bool existed(ReportRule rule)
        {
            using (var context = new Entities())
            {
                return context.rules.Any(i => i.id != rule.id && i.tenantId == rule.tenantId && i.name == rule.name);
            }
        }
    }
}