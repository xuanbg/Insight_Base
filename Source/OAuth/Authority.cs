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
        /// 构造函数，根据角色ID初始化数据
        /// </summary>
        /// <param name="rid">角色ID</param>
        public Authority(Guid rid)
        {
            _RoleList = new List<Guid> {rid};
            InitData(InitType.Permission);
        }

        /// <summary>
        /// 构造函数，根据初始化类型初始化数据
        /// 默认只初始化用户对应角色集合
        /// </summary>
        /// <param name="uid">登录用户ID</param>
        /// <param name="did">登录部门ID</param>
        /// <param name="type">初始化等级</param>
        /// <param name="all">是否包含用户的全部角色（忽略登录部门）</param>
        public Authority(Guid uid, Guid? did, InitType type = 0, bool all = false)
        {
            InitRoleActions(uid, did, all);
            InitData(type);
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
        public IEnumerable<object> GetModuleGroups()
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
        public IEnumerable<object> GetModules()
        {
            var list = from mid in _ActionModules.Select(m => m.ID).Distinct()
                       join m in _Modules on mid equals m.ID
                       select new {m.ID, m.ModuleGroupId, m.Index, m.ProgramName, m.NameSpace, m.ApplicationName, m.Location, m.Default, m.Icon};
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
                       let action = _RoleActions.Where(p => p.ActionId == a.ID).OrderBy(i => i.Action).FirstOrDefault()?.Action ?? 0
                       select new {a.ID, a.ModuleId, a.Index, a.Name, a.Alias, a.Icon, a.ShowText, a.BeginGroup, Enable = action > 0, a.Validity};
            return list.OrderBy(a => a.Index).ToList();
        }

        /// <summary>
        /// 获取用户操作权限
        /// </summary>
        /// <returns>操作权限集合</returns>
        public List<RoleAction> GetActions()
        {
            var groups = (from gid in _ActionModules.Select(m => m.GroupId).Distinct()
                          join g in _Groups on gid equals g.ID
                          select new RoleAction {ID = g.ID, Index = g.Index ?? 0, NodeType = 0, Name = g.Name}).OrderBy(g => g.Index);
            var modules = (from mid in _ActionModules.Select(m => m.ID).Distinct()
                           join m in _Modules on mid equals m.ID
                           select new RoleAction {ID = m.ID, ParentId = m.ModuleGroupId, Index = m.Index ?? 0, NodeType = 1, Name = m.ApplicationName})
                          .OrderBy(m => m.Index).ToList();
            var actions = from m in modules
                          join a in _Actions on m.ID equals a.ModuleId
                          let allows = _RoleActions.Where(p => p.ActionId == a.ID).OrderBy(i => i.Action).FirstOrDefault()
                          select new RoleAction
                          {
                              ID = a.ID,
                              ParentId = m.ID,
                              Action = allows?.Action,
                              Index = a.Index ?? 0,
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
        public List<RoleData> GetDatas()
        {
            var groups = (from gid in _DataModules.Select(m => m.GroupId).Distinct()
                          join g in _Groups on gid equals g.ID
                          select new RoleData {ID = g.ID, Index = g.Index ?? 0, NodeType = 0, Name = g.Name}).OrderBy(g => g.Index);
            var modules = (from mid in _DataModules.Select(m => m.ID).Distinct()
                           join m in _Modules on mid equals m.ID
                           select new RoleData { ID = m.ID, ParentId = m.ModuleGroupId, Index = m.Index ?? 0, NodeType = 1, Name = m.ApplicationName })
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
        private void InitRoleActions(Guid uid, Guid? did, bool all)
        {
            using (var context = new BaseEntities())
            {
                // 读取指定用户对应的全部角色ID
                _RoleList = context.UserRole.Where(r => r.UserId == uid && (all || !r.DiptId.HasValue || r.DiptId == did)).Select(i => i.RoleId).Distinct().ToList();
            }
        }

        /// <summary>
        /// 获取用户的角色集合/授权操作集合/授权数据集合
        /// </summary>
        /// <param name="type">初始化类型</param>
        private void InitData(InitType type)
        {
            using (var context = new BaseEntities())
            {
                // 读取全部角色对应的功能授权原始记录
                _RoleActions = (from a in context.SYS_Role_Action
                                join r in _RoleList on a.RoleId equals r
                                select a).ToList();

                switch (type)
                {
                    // 如功能鉴权，立即返回
                    case InitType.ActionIdentify:
                        return;

                    case InitType.DataIdentify:
                    case InitType.Permission:
                        // 数据鉴权或获取授权信息，读取用户对应全部角色的数据授权原始记录
                        _RoleDatas = (from d in context.SYS_Role_Data
                                      join r in _RoleList on d.RoleId equals r
                                      select d).ToList();

                        // 如数据鉴权，立即返回
                        if (type == InitType.DataIdentify) return;
                        break;
                }

                // 读取功能原始记录，如获取工具栏信息，立即返回
                _Actions = context.SYS_ModuleAction.ToList();
                if (type == InitType.ToolBar) return;

                // 读取模块/模块组原始记录，根据功能授权计算授权给用户的模块
                _Modules = context.SYS_Module.Where(m => m.Validity).ToList();
                _Groups = context.SYS_ModuleGroup.ToList();
                _ActionModules = (from r in _RoleActions
                                  join a in _Actions on r.ActionId equals a.ID
                                  join m in _Modules on a.ModuleId equals m.ID
                                  where m.Validity
                                  group m by new ModuleInfo {ID = m.ID, GroupId = m.ModuleGroupId} into g
                                  select g.Key).Distinct().ToList();

                // 如获取导航信息，立即返回
                if (type == InitType.Navigation) return;

                // 读取数据授权模式原始记录，根据数据授权计算授权给用户的模块
                _Datas = context.SYS_Data.ToList();
                _DataModules = (from r in _RoleDatas
                                join m in _Modules on r.ModuleId equals m.ID
                                where m.Validity
                                group m by new ModuleInfo {ID = m.ID, GroupId = m.ModuleGroupId} into g
                                select g.Key).Distinct().ToList();
            }
        }
    }
}