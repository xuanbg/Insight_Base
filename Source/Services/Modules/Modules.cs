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

            var permits = Core.GetPermits(tenantId, userId, deptId);
            var mids = permits.Select(i => i.parentId).Distinct().ToList();
            using (var context = new Entities())
            {
                var navigators = context.navigators.Where(i => i.appId == appId).ToList();
                var gids = from n in navigators
                    join m in mids on n.id equals m
                    select n.parentId;
                var ids = gids.Distinct().Union(mids).ToList();
                var list = from n in navigators
                    join id in ids on n.id equals id
                    orderby n.parentId, n.index
                    select new { n.id, n.parentId, n.index, n.name, n.alias, n.className, n.filePath, n.icon };

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetAction(string id)
        {
            if (!Verify()) return result;


            return result.Success();
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