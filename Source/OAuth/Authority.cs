using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.Base.OAuth
{
    public class Authority
    {
        private readonly string _UserId;
        private string _DeptId;
        private List<string> _RoleList;
        private readonly List<string> _OrgList = new List<string>();
        private readonly List<string> _UserList = new List<string>();
        private List<Navigator> _Modules;
        private List<Function> _Actions;
        private List<RoleFunction> _RoleActions;
        private List<RoleData> _RoleDatas;
        private List<Organization> _Orgs;
        private List<User> _Users;
        private List<DataConfig> _Datas;
        private List<string> _ActionModules;
        private List<string> _DataModules;

        /// <summary>
        /// 角色集合
        /// </summary>
        public List<string> RoleList => _RoleList;

        /// <summary>
        /// 构造函数，根据角色ID初始化数据
        /// </summary>
        /// <param name="roleId">角色ID</param>
        public Authority(string roleId)
        {
            _RoleList = new List<string> {roleId};
            InitData(InitType.Permission);
        }

        /// <summary>
        /// 构造函数，根据初始化类型初始化数据
        /// 默认只初始化用户对应角色集合
        /// </summary>
        /// <param name="userId">登录用户ID</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="type">初始化等级</param>
        /// <param name="all">是否包含用户的全部角色（忽略登录部门）</param>
        public Authority(string userId, string deptId, InitType type = 0, bool all = false)
        {
            _UserId = userId;
            _DeptId = deptId;
            InitRoleList(userId, deptId, all);
            InitData(type);
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="functionId">操作ID</param>
        /// <returns>bool 是否授权</returns>
        public bool Identify(string functionId)
        {
            var action = (from a in _RoleActions
                          join r in _RoleList on a.roleId equals r
                          where a.functionId == functionId
                          orderby a.permit
                          select a).FirstOrDefault();
            return (action?.permit ?? 0) > 0;
        }

        /// <summary>
        /// 根据传入的创建部门ID或创建人ID返回数据鉴权结果
        /// </summary>
        /// <param name="moduleId">业务模块ID</param>
        /// <param name="deptId">创建部门ID</param>
        /// <param name="userId">创建人ID</param>
        /// <returns></returns>
        public bool Identify(string moduleId, string deptId, string userId)
        {
            if (string.IsNullOrEmpty(deptId) && string.IsNullOrEmpty(userId)) return false;

            var acls = _RoleDatas.Where(i => i.moduleId == moduleId && i.permit == 1);
            foreach (var acl in acls)
            {
                switch (acl.mode)
                {
                    case 0:
                        GetOrgIds(acl.modeId);
                        break;
                    case 1:
                        _UserList.Add(acl.modeId);
                        break;
                    case 2:
                        _OrgList.Add(acl.modeId);
                        break;
                }
            }
            return _OrgList.Any(i => i == deptId) || _UserList.Any(i => i == userId);
        }

        /// <summary>
        /// 登录用户被授权的模块集合
        /// </summary>
        /// <returns>模块集合</returns>
        public IEnumerable<Navigator> GetModules()
        {
            return _Modules;
        }

        /// <summary>
        /// 指定业务模块中登录用户被授权的操作集合
        /// </summary>
        /// <param name="mid">业务模块ID</param>
        /// <returns>操作集合</returns>
        public IEnumerable<object> ModuleActions(string mid)
        {
            var list = from a in _Actions.Where(i => i.navigatorId == mid)
                       let action = _RoleActions.Where(p => p.functionId == a.id).OrderBy(i => i.permit).FirstOrDefault()?.permit ?? 0
                       select new {a.id, a.navigatorId, a.index, a.name, a.alias, a.iconurl, a.icon, a.isHideText, a.isBegin, Enable = action > 0, a.isVisible};
            return list.OrderBy(a => a.index).ToList();
        }

        /// <summary>
        /// 获取用户操作权限
        /// </summary>
        /// <returns>操作权限集合</returns>
        public List<RoleFunction> GetActions()
        {
            return null;
        }

        /// <summary>
        /// 获取用户数据权限
        /// </summary>
        /// <returns>数据权限集合</returns>
        public List<RoleData> GetDatas()
        {
            return null;
        }

        /// <summary>
        /// 获取角色全部可用功能权限
        /// </summary>
        /// <returns></returns>
        public List<RoleFunction> GetAllActions()
        {

            return null;
        }

        /// <summary>
        /// 获取角色全部可用数据权限
        /// </summary>
        /// <returns></returns>
        public List<RoleData> GetAllDatas()
        {
            return null;
        }

        /// <summary>
        /// 获取用户的角色集合
        /// </summary>
        private void InitRoleList(string uid, string did, bool all)
        {
            using (var context = new BaseEntities())
            {
                // 读取指定用户对应的全部角色ID
                _RoleList = context.userRoles.Where(r => r.userId == uid && (all || r.deptId == null || r.deptId == did)).Select(i => i.roleId).Distinct().ToList();
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
                _RoleActions = (from a in context.roleFunctions
                                join r in _RoleList on a.roleId equals r
                                select a).ToList();

                switch (type)
                {
                    // 如功能鉴权，立即返回
                    case InitType.ActionIdentify:
                        return;

                    case InitType.DataIdentify:
                    case InitType.Permission:
                        // 数据鉴权或获取授权信息，读取用户对应全部角色的数据授权原始记录
                        _RoleDatas = (from d in context.roleDatas
                                      join r in _RoleList on d.roleId equals r
                                      select d).ToList();

                        // 如数据鉴权，立即返回
                        if (type == InitType.DataIdentify) return;
                        break;
                }

                // 读取功能原始记录，如获取工具栏信息，立即返回
                _Actions = context.functions.ToList();
                if (type == InitType.ToolBar) return;

                // 读取模块/模块组原始记录，根据功能授权计算授权给用户的模块
                _Modules = context.navigators.ToList();
                _ActionModules = (from r in _RoleActions
                                  join a in _Actions on r.functionId equals a.id
                                  join m in _Modules on a.navigatorId equals m.id
                                  where a.isVisible
                                  select a.navigatorId).Distinct().ToList();

                // 如获取导航信息，立即返回
                if (type == InitType.Navigation) return;

                // 读取数据授权模式原始记录，根据数据授权计算授权给用户的模块
                _Users = context.users.ToList();
                _Orgs = context.organizations.Where(o => o.NodeType == 2).ToList();
                _Datas = context.dataConfigs.ToList();
                _DataModules = (from r in _RoleDatas
                                join m in _Modules on r.moduleId equals m.id
                                select m.id).Distinct().ToList();
            }
        }

        /// <summary>
        /// 获取用户的数据授权
        /// </summary>
        /// <param name="id"></param>
        private void GetOrgIds(string id)
        {
            using (var context = new BaseEntities())
            {
                var mod = context.dataConfigs.Single(i => i.id == id);
                switch (mod.dataType)
                {
                    case 0:
                        _UserList.Add(Guid.Empty.ToString("D"));
                        break;
                    case 1:
                        _UserList.Add(_UserId);
                        break;
                }
            }
        }
    }
}