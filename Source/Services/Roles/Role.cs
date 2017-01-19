using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class Role
    {
        private readonly SYS_Role _Role;
        private IEnumerable<SYS_Role_Action> _AddActions;
        private IEnumerable<SYS_Role_Action> _UpActions;
        private IEnumerable<SYS_Role_Action> _DelActions;
        private IEnumerable<SYS_Role_Data> _AddDatas;
        private IEnumerable<SYS_Role_Data> _UpDatas;
        private IEnumerable<SYS_Role_Data> _DelDatas;
        private List<RoleAction> _RoleActions;
        private List<RoleData> _RoleDatas;
        internal Result Result = new Result();
        internal Authority Authority;

        /// <summary>
        /// 角色是否已存在(按名称)
        /// </summary>
        public bool Existed
        {
            get
            {
                using (var context = new BaseEntities())
                {
                    var role = context.SYS_Role.SingleOrDefault(r => r.Name == _Role.Name);
                    var existed = role != null && role.ID != _Role.ID;
                    if (existed) Result.DataAlreadyExists();

                    return existed;
                }
            }
        }

        /// <summary>
        /// 角色唯一ID
        /// </summary>
        public Guid ID
        {
            get { return _Role.ID; }
            set { _Role.ID = value; }
        }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name
        {
            get { return _Role.Name; }
            set { _Role.Name = value; }
        }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description
        {
            get { return _Role.Description; }
            set { _Role.Description = value; }
        }

        /// <summary>
        /// 是否内置
        /// </summary>
        public bool BuiltIn
        {
            get { return _Role.BuiltIn; }
            set { _Role.BuiltIn = value; }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Validity
        {
            get { return _Role.Validity; }
            set { _Role.Validity = value; }
        }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId
        {
            get { return _Role.CreatorUserId; }
            set { _Role.CreatorUserId = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _Role.CreateTime; }
            set { _Role.CreateTime = value; }
        }

        /// <summary>
        /// 角色成员
        /// </summary>
        public List<RoleMember> Members
        {
            get
            {
                using (var context = new BaseEntities())
                {
                    return context.RoleMember.Where(m => m.RoleId == ID).ToList();
                }
            }
        }

        /// <summary>
        /// 角色操作权限集合
        /// </summary>
        public List<RoleAction> Actions
        {
            get { return _RoleActions ?? Authority?.GetActions(); }
            set
            {
                var list = value?.Where(a => a.NodeType > 1 && a.Permit != a.Action).ToList();
                SetActions(list ?? new List<RoleAction>());
            }
        }

        /// <summary>
        /// 角色数据权限集合
        /// </summary>
        public List<RoleData> Datas
        {
            get { return _RoleDatas ?? Authority?.GetDatas(); }
            set
            {
                var list = value?.Where(d => d.NodeType > 1 && d.Permit != d.Permission).ToList();
                SetDatas(list ?? new List<RoleData>());
            }
        }

        /// <summary>
        /// 构造函数，构造新的角色实体
        /// </summary>
        public Role()
        {
            _Role = new SYS_Role();
            Result.Success();
        }

        /// <summary>
        /// 构造函数，根据ID读取角色实体
        /// </summary>
        /// <param name="id">角色ID</param>
        public Role(Guid id)
        {
            using (var context = new BaseEntities())
            {
                _Role = context.SYS_Role.SingleOrDefault(r => r.ID == id);
                if (_Role == null)
                {
                    _Role = new SYS_Role();
                    Result.NotFound();
                }
                else
                {
                    Authority = new Authority(id);
                    Result.Success();
                }
            }
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Add()
        {
            using (var context = new BaseEntities())
            {
                context.SYS_Role.Add(_Role);
                context.SYS_Role_Action.AddRange(_AddActions);
                context.SYS_Role_Data.AddRange(_AddDatas);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    Result.DataBaseError();
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Delete()
        {
            var result = DbHelper.Delete(_Role);
            if (!result) Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Update()
        {
            using (var context = new BaseEntities())
            {
                context.SYS_Role.Attach(_Role);
                context.Entry(_Role).State = EntityState.Modified;

                context.SYS_Role_Action.AddRange(_AddActions);
                _UpActions.ToList().ForEach(i =>
                {
                    context.SYS_Role_Action.Attach(i);
                    context.Entry(i).State = EntityState.Modified;
                });
                _DelActions.ToList().ForEach(i =>
                {
                    context.SYS_Role_Action.Attach(i);
                    context.Entry(i).State = EntityState.Deleted;
                });

                context.SYS_Role_Data.AddRange(_AddDatas);
                _UpDatas.ToList().ForEach(i =>
                {
                    context.SYS_Role_Data.Attach(i);
                    context.Entry(i).State = EntityState.Modified;
                });
                _DelDatas.ToList().ForEach(i =>
                {
                    context.SYS_Role_Data.Attach(i);
                    context.Entry(i).State = EntityState.Deleted;
                });

                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    Result.DataBaseError();
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取角色可用权限资源
        /// </summary>
        /// <returns></returns>
        public void GetAllPermission()
        {
            Authority = new Authority(ID);
            _RoleActions = Authority.GetAllActions();
            _RoleDatas = Authority.GetAllDatas();
        }

        /// <summary>
        /// 设置功能权限数据
        /// </summary>
        /// <param name="actions">功能权限数据</param>
        private void SetActions(List<RoleAction> actions)
        {
            _AddActions = from a in actions.Where(i => !i.Action.HasValue && i.Permit.HasValue)
                          select new SYS_Role_Action
                          {
                              ID = a.ID,
                              RoleId = ID,
                              ActionId = a.ActionId,
                              Action = a.Permit.Value,
                              CreatorUserId = CreatorUserId,
                              CreateTime = DateTime.Now
                          };
            using (var context = new BaseEntities())
            {
                var list = context.SYS_Role_Action.ToList();
                _UpActions = from a in actions.Where(i => i.Action.HasValue && i.Permit.HasValue)
                             join p in list on a.ID equals p.ID
                             select new SYS_Role_Action
                             {
                                 ID = a.ID,
                                 RoleId = ID,
                                 ActionId = a.ActionId,
                                 Action = a.Permit.Value,
                                 CreatorUserId = p.CreatorUserId,
                                 CreateTime = p.CreateTime
                             };
                _DelActions = from a in actions.Where(i => i.Action.HasValue && !i.Permit.HasValue)
                              join p in list on a.ID equals p.ID
                              select p;
            }
        }

        /// <summary>
        /// 设置数据权限数据
        /// </summary>
        /// <param name="datas">数据权限数据</param>
        private void SetDatas(List<RoleData> datas)
        {
            _AddDatas = from d in datas.Where(i => !i.Permission.HasValue && i.Permit.HasValue)
                        select new SYS_Role_Data
                        {
                            ID = d.ID,
                            RoleId = ID,
                            ModuleId = (Guid)d.ParentId,
                            Mode = d.Mode,
                            ModeId = d.ModeId,
                            Permission = d.Permit.Value,
                            CreatorUserId = CreatorUserId,
                            CreateTime = DateTime.Now
                        };
            using (var context = new BaseEntities())
            {
                var list = context.SYS_Role_Data.ToList();
                _UpDatas = from d in datas.Where(i => i.Permission.HasValue && i.Permit.HasValue)
                           join p in list on d.ID equals p.ID
                           select new SYS_Role_Data
                           {
                               ID = d.ID,
                               RoleId = ID,
                               ModuleId = (Guid)d.ParentId,
                               Mode = d.Mode,
                               ModeId = d.ModeId,
                               Permission = d.Permit.Value,
                               CreatorUserId = p.CreatorUserId,
                               CreateTime = p.CreateTime
                           };
                _DelDatas = from d in datas.Where(i => i.Permission.HasValue && !i.Permit.HasValue)
                            join p in list on d.ID equals p.ID
                            select p;
            }
        }
    }
}