using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class Role : RoleBase
    {
        private readonly Authority _Authority;
        private IEnumerable<SYS_Role_Action> _AddActions;
        private IEnumerable<SYS_Role_Action> _UpActions;
        private IEnumerable<SYS_Role_Action> _DelActions;
        private IEnumerable<SYS_Role_Data> _AddDatas;
        private IEnumerable<SYS_Role_Data> _UpDatas;
        private IEnumerable<SYS_Role_Data> _DelDatas;
        private List<RoleAction> _RoleActions;
        private List<RoleData> _RoleDatas;

        public Result Result = new Result();

        /// <summary>
        /// 角色操作权限集合
        /// </summary>
        public List<RoleAction> Actions
        {
            get { return _RoleActions ?? _Authority?.GetActions(); }
            set
            {
                var list = value.Where(a => a.NodeType > 1 && a.Permit != a.Action).ToList();
                SetActions(list);
            }
        }

        /// <summary>
        /// 角色数据权限集合
        /// </summary>
        public List<RoleData> Datas
        {
            get { return _RoleDatas ?? _Authority?.GetDatas(); }
            set
            {
                var list = value.Where(d => d.NodeType > 1 && d.Permit != d.Permission).ToList();
                SetDatas(list);
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
                    _Authority = new Authority(id);
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
            _RoleActions = _Authority.GetAllActions();
            _RoleDatas = _Authority.GetAllDatas();
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