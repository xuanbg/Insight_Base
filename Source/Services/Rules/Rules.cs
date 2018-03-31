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
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取所有报表分期规则
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> GetRules(int rows, int page)
        {
            if (!Verify("getRules")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.rules.Where(i => !i.isInvalid && (i.tenantId == null || i.tenantId == tenantId));
                var rules = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(rules, list.Count());
            }
        }

        /// <summary>
        /// 获取报表分期
        /// </summary>
        /// <param name="id">分期ID</param>
        /// <returns>Result</returns>
        public Result<object> GetRule(string id)
        {
            if (!Verify("getRules")) return result;

            var data = DbHelper.Find<Rule>(id);

            return data == null ? result.NotFound() : result.Success(data);
        }

        /// <summary>
        /// 新建报表分期
        /// </summary>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> AddRule(Rule rule)
        {
            if (!Verify("newRule")) return result;

            rule.id = Util.NewId();
            rule.tenantId = tenantId;
            rule.isBuiltin = false;
            rule.isInvalid = false;
            rule.creatorDeptId = deptId;
            rule.creator = userName;
            rule.creatorId = userId;
            rule.createTime = DateTime.Now;

            return DbHelper.Insert(rule) ? result.Created(rule) : result.DataBaseError();
        }

        /// <summary>
        /// 编辑报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> EditRule(string id, Rule rule)
        {
            if (!Verify("editRule")) return result;

            var data = DbHelper.Find<Rule>(id);
            if (data == null) return result.NotFound();

            if (data.isBuiltin) return result.NotBeModified();

            data.cycleType = rule.cycleType;
            data.name = rule.name;
            data.cycle = rule.cycle;
            data.startTime = rule.startTime;
            data.remark = rule.remark;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 删除报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteRule(string id)
        {
            if (!Verify("deleteRule")) return result;

            var data = DbHelper.Find<Rule>(id);
            if (data == null) return result.NotFound();

            if (data.isBuiltin) return result.NotBeDeleted();

            data.isInvalid = true;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }
    }
}