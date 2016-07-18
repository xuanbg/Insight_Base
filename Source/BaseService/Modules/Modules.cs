using System;
using System.Collections.Generic;
using System.Linq;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    public partial class BaseService : IModules
    {
        /// <summary>
        /// 获取用户获得授权的所有模块的组信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetModuleGroup()
        {
            var verify = new Verify();
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var mids = context.Get_PermModule(verify.Basis.UserId, verify.Basis.DeptId);
                var list = from g in context.SYS_ModuleGroup.Where(g => mids.Any(id => id == g.ID))
                           select new
                           {
                               g.ID,
                               g.Index,
                               g.Name,
                               g.Icon
                           };
                return list.Any() ? verify.Result.Success(Util.Serialize(list)) : verify.Result.NoContent();
            }
        }

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUserModule()
        {
            var verify = new Verify();
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var mids = context.Get_PermModule(verify.Basis.UserId, verify.Basis.DeptId);
                var list = from m in context.SYS_Module.Where(g => g.Validity && mids.Any(id => id == g.ID))
                           select new
                           {
                               m.ID,
                               m.ModuleGroupId,
                               m.Index,
                               m.ProgramName,
                               m.MainFrom,
                               m.ApplicationName,
                               m.Location,
                               m.Default,
                               m.Icon
                           };
                return list.Any() ? verify.Result.Success(Util.Serialize(list)) : verify.Result.NoContent();
            }
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetAction(string id)
        {
            var verify = new Verify();
            if (!verify.ParseIdAndCompare(id)) return verify.Result;

            using (var context = new BaseEntities())
            {
                var aids = context.Get_PermAction(verify.Guid, verify.Basis.UserId, verify.Basis.DeptId);
                var list = from a in context.SYS_ModuleAction.Where(a => a.ModuleId == verify.Guid)
                           select new
                           {
                               a.ID,
                               a.ModuleId,
                               a.Index,
                               a.Name,
                               a.Alias,
                               a.Icon,
                               a.ShowText,
                               a.BeginGroup,
                               Enable = aids.Any(p => p == a.ID),
                               a.Validity
                           };
                return list.Any() ? verify.Result.Success(Util.Serialize(list)) : verify.Result.NoContent();
            }
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
