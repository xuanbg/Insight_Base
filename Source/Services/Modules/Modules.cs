using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Modules:IModules
    {
        /// <summary>
        /// 获取用户获得授权的所有模块的组信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetModuleGroup()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var auth = new Authority(verify.Basis.UserId, verify.Basis.DeptId);
            var data = auth.PermModuleGroups();
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUserModule()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var auth = new Authority(verify.Basis.UserId, verify.Basis.DeptId);
            var data = auth.PermModules();
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetAction(string id)
        {
            Guid mid;
            var result = new JsonResult();
            if (!Guid.TryParse(id, out mid))
            {
                result.InvalidGuid();
                return result;
            }

            var verify = new Compare();
            result = verify.Result;
            if (!result.Successful) return result;

            var auth = new Authority(verify.Basis.UserId, verify.Basis.DeptId);
            var data = auth.ModuleActions(mid);
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
