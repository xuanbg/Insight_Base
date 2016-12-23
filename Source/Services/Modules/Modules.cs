using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Modules:IModules
    {
        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        public Result GetNavigation()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var auth = new Authority(verify.Basis.UserId, verify.Basis.DeptId, InitType.Navigation);
            var data = new {Groups = auth.PermModuleGroups(), Modules = auth.PermModules()};
            result.Success(data);

            return result;
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result GetAction(string id)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var mid = new GuidParse(id).Guid;
            if (!mid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var auth = new Authority(verify.Basis.UserId, verify.Basis.DeptId, InitType.ToolBar);
            var data = auth.ModuleActions(mid.Value);
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        public JsonResult GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult SaveModuleParam(string id, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
        {
            throw new NotImplementedException();
        }
    }
}
