using System;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Modules : ServiceBase, IModules
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetNavigation()
        {
            if (!Verify()) return result;

            var permits = Core.GetNavigation(tenantId, appId, userId, deptId);

            return result.Success(permits);
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetAction(string id)
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                var list = from f in context.functions
                    join p in context.roleFunctions on f.id equals p.functionId
                    where f.navigatorId == id && f.isVisible
                    orderby f.index
                    select new {f.id, f.index, f.name, f.alias, f.icon, f.isBegin, f.isShowText, p.permit};

                return result.Success(list.ToList());
            }
        }

        public Result<object> GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }
    }
}