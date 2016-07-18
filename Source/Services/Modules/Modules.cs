using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

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

            var auth = new Authority(verify.Token.UserId, verify.Token.DeptId);
            var data = auth.PermModuleGroups();
            return data.Any() ? verify.Result.Success(data) : verify.Result.NoContent();
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

            var auth = new Authority(verify.Token.UserId, verify.Token.DeptId);
            var data = auth.PermModules();
            return data.Any() ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetAction(string id)
        {
            Guid mid;
            if (Guid.TryParse(id, out mid)) return new JsonResult().InvalidGuid();

            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var auth = new Authority(verify.Token.UserId, verify.Token.DeptId);
            var data = auth.ModuleActions(mid);
            return data.Any() ? result.Success(data) : result.NoContent();
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
