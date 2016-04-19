using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using Insight.WS.Base.Common.Utils;

namespace Insight.WS.Base
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

            var data = GetModuleGroup(verify.Session);
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

            var data = GetUserModule(verify.Session);
            return data.Any() ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetModuleInfo(string id)
        {
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id)) return verify.Result;

            var data = GetModuleInfo(verify.Guid);
            if (data == null) return verify.Result.NotFound();

            var info = new
            {
                data.ID,
                data.ApplicationName,
                data.ProgramName,
                data.MainFrom,
                data.Location,
                data.Icon
            };
            return verify.Result.Success(info);
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

            var data = GetAction(verify.Session, verify.Guid);
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
