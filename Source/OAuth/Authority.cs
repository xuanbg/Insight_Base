using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.Base.OAuth
{
    public class Authority
    {
        private List<Guid> _RoleList;
        private List<SYS_ModuleGroup> _Groups;
        private List<SYS_Module> _Modules;
        private List<SYS_ModuleAction> _Actions;
        private List<SYS_Role_Action> _RoleActions;
        private List<SYS_Role_Data> _RoleDatas;
        private List<SYS_Data> _Datas;
        private List<ModuleInfo> _ActionModules;
        private List<ModuleInfo> _DataModules;

        /// <summary>
        /// 构造函数，根据初始化等级初始化数据
        /// 默认只初始化用户对应角色集合
        /// </summary>
        /// <param name="uid">登录用户ID</param>
        /// <param name="did">登录部门ID</param>
        /// <param name="grade">初始化等级</param>
        /// <param name="all">是否包含用户的全部角色（忽略登录部门）</param>
        public Authority(Guid uid, Guid? did, InitType grade = 0, bool all = false)
        {
            Init(uid, did, grade, all);
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <returns>bool 是否授权</returns>
        public bool Identify(Guid aid)
        {
            var action = (from a in _RoleActions
                          join r in _RoleList on a.RoleId equals r
                          where a.ActionId == aid
                          orderby a.Action
                          select a).FirstOrDefault();
            return (action?.Action ?? 0) > 0;
        }

        /// <summary>
        /// 登录用户被授权的模块组集合
        /// </summary>
        /// <returns>模块组集合</returns>
        public IEnumerable<object> PermModuleGroups()
        {
            var list = from gid in _ActionModules.Select(m => m.GroupId).Distinct()
                       join g in _Groups on gid equals g.ID
                       select new {g.ID, g.Index, g.Name, g.Icon};
            return list.OrderBy(g => g.Index).ToList();
        }

        /// <summary>
        /// 登录用户被授权的模块集合
        /// </summary>
        /// <returns>模块集合</returns>
        public IEnumerable<object> PermModules()
        {
            var list = from mid in _ActionModules.Select(m => m.ID).Distinct()
                       join m in _Modules on mid equals m.ID
                       select new
                       {
                           m.ID,
                           m.ModuleGroupId,
                           m.Index,
                           m.ProgramName,
                           m.NameSpace,
                           m.ApplicationName,
                           m.Location,
                           m.Default,
                           m.Icon
                       };
            return list.OrderBy(m => m.Index).ToList();
        }

        /// <summary>
        /// 指定业务模块中登录用户被授权的操作集合
        /// </summary>
        /// <param name="mid">业务模块ID</param>
        /// <returns>操作集合</returns>
        public IEnumerable<object> ModuleActions(Guid mid)
        {
            var list = from a in _Actions.Where(i => i.ModuleId == mid)
                       let perm = _RoleActions.Where(p => p.ActionId == a.ID).OrderBy(i => i.Action).FirstOrDefault()
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
                           Enable = (perm?.Action ?? 0) > 0,
                           a.Validity
                       };
            return list.OrderBy(a => a.Index).ToList();
        }


        /// <summary>
        /// 获取用户操作权限
        /// </summary>
        /// <returns>操作权限集合</returns>
        public IEnumerable<RoleAction> GetUserActions()
        {
            var groups = (from gid in _ActionModules.Select(m => m.GroupId).Distinct()
                          join g in _Groups on gid equals g.ID
                          select new RoleAction {ID = g.ID, Index = g.Index, NodeType = 0, Name = g.Name}).OrderBy(g => g.Index);
            var modules = (from mid in _ActionModules.Select(m => m.ID).Distinct()
                           join m in _Modules on mid equals m.ID
                           select new RoleAction {ID = m.ID, ParentId = m.ModuleGroupId, Index = m.Index, NodeType = 1, Name = m.ApplicationName})
                          .OrderBy(m => m.Index).ToList();
            var actions = from m in modules
                          join a in _Actions on m.ID equals a.ModuleId
                          let allows = _RoleActions.Where(p => p.ActionId == a.ID).OrderBy(i => i.Action).FirstOrDefault()
                          select new RoleAction
                          {
                              ID = a.ID,
                              ParentId = m.ID,
                              Action = allows?.Action,
                              Index = a.Index,
                              NodeType = 2 + (allows?.Action ?? 2),
                              Name = a.Alias,
                              Description = allows == null ? null : (allows.Action < 1 ? "拒绝" : "允许")
                          };
            return groups.Union(modules).Union(actions.OrderBy(a => a.Index)).ToList();
        }

        /// <summary>
        /// 获取用户数据权限
        /// </summary>
        /// <returns>数据权限集合</returns>
        public IEnumerable<RoleData> GetUserDatas()
        {
            var groups = (from gid in _DataModules.Select(m => m.GroupId).Distinct()
                          join g in _Groups on gid equals g.ID
                          select new RoleData {ID = g.ID, Index = g.Index, NodeType = 0, Name = g.Name}).OrderBy(g => g.Index);
            var modules = (from mid in _DataModules.Select(m => m.ID).Distinct()
                           join m in _Modules on mid equals m.ID
                           select new RoleData { ID = m.ID, ParentId = m.ModuleGroupId, Index = m.Index, NodeType = 1, Name = m.ApplicationName })
                          .OrderBy(m => m.Index).ToList();
            var datas = from m in modules
                        join p in _RoleDatas on m.ID equals p.ModuleId
                        join d in _Datas on p.ModeId equals d.ID
                        where p.Mode == 0
                        group p by new {p.ModuleId, p.Mode, d.Type, d.Alias} into g
                        select new RoleData
                        {
                            ID = Guid.NewGuid(),
                            ParentId = g.Key.ModuleId,
                            Mode = g.Key.Mode,
                            Permission = g.Min(i => i.Permission),
                            Index = g.Key.Type,
                            NodeType = g.Key.Type + 2,
                            Name = g.Key.Alias
                        };
            return groups.Union(modules).Union(datas.OrderBy(a => a.Index)).ToList();
        }

        /// <summary>
        /// 获取用户的角色集合/授权操作集合/授权数据集合
        /// </summary>
        private void Init(Guid uid, Guid? did, InitType grade, bool all)
        {
            using (var context = new BaseEntities())
            {
                // 通过用户和角色关系，读取指定用户对应的全部角色ID
                var rid_u = from r in context.SYS_Role_Member.Where(m => m.MemberId == uid)
                            where r.Type == 1
                            select r.RoleId;
                var rid_g = from m in context.SYS_UserGroupMember.Where(u => u.UserId == uid)
                            join r in context.SYS_Role_Member on m.GroupId equals r.MemberId
                            where r.Type == 2
                            select r.RoleId;
                var rid_t = from m in context.SYS_OrgMember.Where(u => u.UserId == uid)
                            join r in context.SYS_Role_Member on m.OrgId equals r.MemberId
                            join o in context.SYS_Organization on m.OrgId equals o.ID
                            where r.Type == 3 && (all || o.ParentId == did)
                            select r.RoleId;
                _RoleList = rid_u.Union(rid_g).Union(rid_t).ToList();

                // 读取用户对应全部角色的功能授权原始记录
                _RoleActions = (from a in context.SYS_Role_Action
                                join r in _RoleList on a.RoleId equals r
                                select a).ToList();

                switch (grade)
                {
                    // 功能鉴权，立即返回
                    case InitType.ActionIdentify:
                        return;

                    // 数据鉴权或获取授权信息，读取用户对应全部角色的数据授权原始记录
                    case InitType.DataIdentify:
                    case InitType.Permission:
                        _RoleDatas = (from d in context.SYS_Role_Data
                                      join r in _RoleList on d.RoleId equals r
                                      select d).ToList();

                        // 如数据鉴权，立即返回
                        if (grade == InitType.DataIdentify) return;

                        break;
                }

                // 读取功能/模块/模块组原始记录
                _Actions = context.SYS_ModuleAction.ToList();
                _Modules = context.SYS_Module.Where(m => m.Validity).ToList();
                _Groups = context.SYS_ModuleGroup.ToList();

                // 根据功能授权计算授权给用户的模块
                _ActionModules = (from r in _RoleActions
                                  join a in _Actions on r.ActionId equals a.ID
                                  join m in _Modules on a.ModuleId equals m.ID
                                  where m.Validity
                                  group m by new ModuleInfo { ID = m.ID, GroupId = m.ModuleGroupId } into g
                                  select g.Key).Distinct().ToList();

                // 如获取导航信息或工具栏信息，立即返回
                if (grade == InitType.Navigation || grade == InitType.ToolBar)return;

                // 读取数据授权模式原始记录
                _Datas = context.SYS_Data.ToList();

                // 根据数据授权计算授权给用户的模块
                _DataModules = (from r in _RoleDatas
                                join m in _Modules on r.ModuleId equals m.ID
                                where m.Validity
                                group m by new ModuleInfo { ID = m.ID, GroupId = m.ModuleGroupId } into g
                                select g.Key).Distinct().ToList();
            }
        }
    }
}