using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base
{
    public partial class Modules
    {
        /// <summary>
        /// 获取用户被授权的模块组列表
        /// </summary>
        /// <param name="us">用户会话</param>
        /// <returns>DataTable</returns>
        private IEnumerable<object> GetModuleGroup(Session us)
        {
            using (var context = new BaseEntities())
            {
                var gids = (from p in context.Get_PermModule(us.UserId, us.DeptId)
                            join m in context.SYS_Module on p.Value equals m.ID
                            select m.ModuleGroupId).Distinct();
                var list = from g in context.SYS_ModuleGroup
                           join m in gids on g.ID equals m.Value
                           select new {g.ID, g.Index, g.Name, g.Icon};
                return list.OrderBy(g => g.Index).ToList();
            }
        }

        /// <summary>
        /// 获取用户被授权的模块列表
        /// </summary>
        /// <param name="us">用户会话</param>
        /// <returns>DataTable</returns>
        private IEnumerable<object> GetUserModule(Session us)
        {
            using (var context = new BaseEntities())
            {
                var list = from p in context.Get_PermModule(us.UserId, us.DeptId)
                           join m in context.SYS_Module on p.Value equals m.ID
                           where m.Validity
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
                return list.OrderBy(m => m.Index).ToList();
            }
        }

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_Module</returns>
        private SYS_Module GetModuleInfo(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Module.SingleOrDefault(m => m.ID == id);
            }
        }

        /// <summary>
        /// 获取用户被授权的操作列表
        /// </summary>
        /// <param name="us">用户会话</param>
        /// <param name="id">模块ID</param>
        /// <returns>DataTable</returns>
        private IEnumerable<object> GetAction(Session us, Guid id)
        {
            using (var context = new BaseEntities())
            {
                var list = from p in context.Get_PermAction(id, us.UserId, us.DeptId)
                           join a in context.SYS_ModuleAction on p.Value equals a.ID into temp
                           from t in temp.DefaultIfEmpty()
                           select new
                           {
                               t.ID,
                               t.ModuleId,
                               t.Index,
                               t.Name,
                               t.Alias,
                               t.Icon,
                               t.ShowText,
                               t.BeginGroup,
                               Enable = p.HasValue,
                               t.Validity
                           };
                return list.OrderBy(m => m.Index).ToList();
            }
        }
    }
}
