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
            var verify = new SessionVerify();
            if (!verify.Compare()) return verify.Result;

            var auth = new Authority(verify.Session.UserId, verify.Session.DeptId);
            var data = auth.PermModuleGroups();
            return data.Any() ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUserModule()
        {
            var verify = new SessionVerify();
            if (!verify.Compare()) return verify.Result;

            var auth = new Authority(verify.Session.UserId, verify.Session.DeptId);
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
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id)) return verify.Result;

            var auth = new Authority(verify.Session.UserId, verify.Session.DeptId);
            var data = auth.ModuleActions(verify.ID);
            return data.Any() ? verify.Result.Success(data) : verify.Result.NoContent();
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
