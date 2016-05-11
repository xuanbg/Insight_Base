using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.Base.Common
{
    public class Authority
    {
        /// <summary>
        /// 登录用户ID
        /// </summary>
        private readonly Guid UserId;

        /// <summary>
        /// 登录部门ID
        /// </summary>
        private readonly Guid? DeptId;

        /// <summary>
        /// 构造函数，传入业务模块ID、登录用户ID和登录部门ID
        /// </summary>
        /// <param name="uid">登录用户ID</param>
        /// <param name="did">登录部门ID</param>
        public Authority(Guid uid, Guid? did)
        {
            UserId = uid;
            DeptId = did;
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <returns>bool 是否授权</returns>
        public bool Identify(Guid aid)
        {
            return UserPermActions(aid).Any(p => p.Authority > 0);
        }

        /// <summary>
        /// 指定业务模块中登录用户被授权的操作集合
        /// </summary>
        /// <param name="mid">业务模块ID</param>
        /// <returns>操作集合</returns>
        public IEnumerable<object> ModuleActions(Guid mid)
        {
            using (var context = new BaseEntities())
            {
                var list = from a in context.SYS_ModuleAction.Where(a => a.ModuleId == mid)
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
                               Enable = UserPermActions(a.ID).Any(p => p.Authority > 0),
                               a.Validity
                           };
                return list.OrderBy(a => a.Index);
            }
        }

        /// <summary>
        /// 登录用户被授权的模块集合
        /// </summary>
        /// <returns>模块集合</returns>
        public IEnumerable<object> PermModules()
        {
            using (var context = new BaseEntities())
            {
                var list = from r in context.SYS_Role_Action.Where(r => UserRoles().Any(id => id == r.RoleId))
                           join a in context.SYS_ModuleAction on r.ActionId equals a.ID
                           join m in context.SYS_Module on a.ModuleId equals m.ID
                           select new { m.ID, m.ModuleGroupId, m.Index, m.Name, m.Icon };
                return list.OrderBy(m => m.Index);
            }
        }

        /// <summary>
        /// 登录用户被授权的模块组集合
        /// </summary>
        /// <returns>模块组集合</returns>
        public IEnumerable<object> PermModuleGroups()
        {
            using (var context = new BaseEntities())
            {
                var list = from r in context.SYS_Role_Action.Where(r => UserRoles().Any(id => id == r.RoleId))
                           join a in context.SYS_ModuleAction on r.ActionId equals a.ID
                           join m in context.SYS_Module on a.ModuleId equals m.ID
                           join g in context.SYS_ModuleGroup on m.ModuleGroupId equals g.ID
                           select new { g.ID, g.Index, g.Name, g.Icon };
                return list.OrderBy(g => g.Index);
            }
        }

        /// <summary>
        /// 获取指定功能操作对登录用户的授权情况
        /// </summary>
        /// <param name="aid">功能操作ID</param>
        /// <returns>操作授权情况</returns>
        private IQueryable<ActionAuth> UserPermActions(Guid aid)
        {
            using (var context = new BaseEntities())
            {
                return from p in context.SYS_Role_Action.Where(a => a.ActionId == aid && UserRoles().Any(id => id == a.RoleId))
                       group p by p.ActionId into g
                       select new ActionAuth { ID = g.Key, Authority = g.Min(p => p.Action) };
            }
        }

        /// <summary>
        /// 根据登录用户ID和登录部门ID，获取用户的角色集合
        /// </summary>
        /// <returns>角色ID集合</returns>
        private IQueryable<Guid> UserRoles()
        {
            using (var context = new BaseEntities())
            {
                var rid_u = from r in context.SYS_Role_Member.Where(m => m.MemberId == UserId)
                            where r.Type == 1
                            select r.RoleId;
                var rid_g = from m in context.SYS_UserGroupMember.Where(u => u.UserId == UserId)
                            join r in context.SYS_Role_Member on m.GroupId equals r.MemberId
                            where r.Type == 2
                            select r.RoleId;
                var rid_t = from m in context.SYS_OrgMember.Where(u => u.UserId == UserId)
                            join r in context.SYS_Role_Member on m.OrgId equals r.MemberId
                            join o in context.SYS_Organization on m.OrgId equals o.ID
                            where r.Type == 3 && o.ParentId == DeptId
                            select r.RoleId;
                return rid_u.Union(rid_g).Union(rid_t);
            }
        }
    }

    /// <summary>
    /// 用户操作权限
    /// </summary>
    internal class ActionAuth
    {
        /// <summary>
        /// 功能操作ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 权限
        /// </summary>
        public int Authority { get; set; }
    }
}
