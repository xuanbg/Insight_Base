using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.Base.OAuth
{
    public class Authority
    {
        // 登录用户ID
        private readonly Guid _UserId;

        // 登录部门ID
        private readonly Guid? _DeptId;

        /// <summary>
        /// 构造函数，传入业务模块ID、登录用户ID和登录部门ID
        /// </summary>
        /// <param name="uid">登录用户ID</param>
        /// <param name="did">登录部门ID</param>
        public Authority(Guid uid, Guid? did)
        {
            _UserId = uid;
            _DeptId = did;
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
                var roles = UserRoles();
                var allows = context.SYS_Role_Action.Where(a => a.SYS_ModuleAction.ModuleId == mid && roles.Any(id => id == a.RoleId));
                var perm = allows.GroupBy(a => new {a.ActionId, a.Action}).Select(p => new {ID = p.Key.ActionId, Action = p.Min(a => a.Action)});
                var actions = context.SYS_ModuleAction.Where(a => a.ModuleId == mid);
                var list = actions.Select(a => new { a.ID, a.ModuleId, a.Index, a.Name, a.Alias, a.Icon, a.ShowText, a.BeginGroup, Enable = perm.Any(p => p.ID == a.ID && p.Action > 0), a.Validity });
                return list.OrderBy(a => a.Index).ToList();
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
                var mids = RoleModules().Select(m => m.ID).Distinct();
                var list = from m in context.SYS_Module.Where(g => g.Validity && mids.Any(id => id == g.ID))
                    select new {m.ID, m.ModuleGroupId, m.Index, m.ProgramName, m.NameSpace, m.ApplicationName, m.Location, m.Default, m.Icon};
                return list.OrderBy(m => m.Index).ToList();

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
                var gids = RoleModules().Select(m => m.GroupId).Distinct();
                var list = from g in context.SYS_ModuleGroup.Where(g => gids.Any(id => id == g.ID))
                           select new { g.ID, g.Index, g.Name, g.Icon };
                return list.OrderBy(g => g.Index).ToList();
            }
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <returns>bool 是否授权</returns>
        public bool Identify(Guid aid)
        {
            using (var context = new BaseEntities())
            {
                var roles = UserRoles();
                var actions = context.SYS_Role_Action.Where(a => a.ActionId == aid && roles.Any(id => id == a.RoleId));
                return actions.Min(a => a.Action) > 0;
            }
        }

        /// <summary>
        /// 登录用户被授权的模块集合
        /// </summary>
        /// <returns>模块集合</returns>
        private IEnumerable<ModuleInfo> RoleModules()
        {
            using (var context = new BaseEntities())
            {
                var roles = UserRoles();
                var list = from r in context.SYS_Role_Action.Where(r => roles.Any(id => id == r.RoleId))
                    join a in context.SYS_ModuleAction on r.ActionId equals a.ID
                    join m in context.SYS_Module on a.ModuleId equals m.ID
                    group m by new ModuleInfo {ID = m.ID, GroupId = m.ModuleGroupId}
                    into g
                    select g.Key;
                return list.ToList();
            }
        }

        /// <summary>
        /// 根据登录用户ID和登录部门ID，获取用户的角色集合
        /// </summary>
        /// <returns>角色ID集合</returns>
        private IEnumerable<Guid> UserRoles()
        {
            using (var context = new BaseEntities())
            {
                var rid_u = from r in context.SYS_Role_Member.Where(m => m.MemberId == _UserId)
                    where r.Type == 1
                    select r.RoleId;
                var rid_g = from m in context.SYS_UserGroupMember.Where(u => u.UserId == _UserId)
                    join r in context.SYS_Role_Member on m.GroupId equals r.MemberId
                    where r.Type == 2
                    select r.RoleId;
                var rid_t = from m in context.SYS_OrgMember.Where(u => u.UserId == _UserId)
                    join r in context.SYS_Role_Member on m.OrgId equals r.MemberId
                    join o in context.SYS_Organization on m.OrgId equals o.ID
                    where r.Type == 3 && o.ParentId == _DeptId
                    select r.RoleId;
                return rid_u.Union(rid_g).Union(rid_t).ToList();
            }
        }
    }

    public class ModuleInfo
    {
        public Guid ID { get; set; }
        public Guid GroupId { get; set; }
    }
}
