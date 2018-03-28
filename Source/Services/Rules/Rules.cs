using System;
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
        public Result<object> GetRules(string rows, string page)
        {
            if (!Verify("17cb33e9-e9d4-4f22-824c-55cf627cd42a")) return result;

            return null;
        }

        /// <summary>
        /// 获取报表分期
        /// </summary>
        /// <param name="id">分期ID</param>
        /// <returns>Result</returns>
        public Result<object> GetRule(string id)
        {
            if (!Verify("17cb33e9-e9d4-4f22-824c-55cf627cd42a")) return result;

            return null;
        }

        /// <summary>
        /// 新建报表分期
        /// </summary>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> AddRule(Rule rule)
        {
            if (!Verify("49ca6a0f-00f8-40f8-aca4-4879cac9eb50")) return result;

            return null;
        }

        /// <summary>
        /// 编辑报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        public Result<object> EditRule(string id, Rule rule)
        {
            if (!Verify("37a8b851-4a97-4223-902f-2c08c737ba06")) return result;

            return null;
        }

        /// <summary>
        /// 删除报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteRule(string id)
        {
            if (!Verify("e4790841-7893-4b28-8d48-aa3f06a42675")) return result;

            return null;
        }
    }
}