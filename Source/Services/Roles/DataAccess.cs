using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public partial class Roles
    {
        /// <summary>
        /// 将角色数据插入数据库
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>bool 是否插入成功</returns>
        private bool InsertData(Guid uid, RoleInfo role)
        {
            var now = DateTime.Now;
            var r = new SYS_Role
            {
                ID = role.ID,
                Name = role.Name,
                Description = role.Description,
                BuiltIn = false,
                Validity = true,
                CreatorUserId = uid,
                CreateTime = now
            };
            var alist = from a in role.Actions
                        where a.NodeType > 1 && a.Permit != a.Action && (a.Permit.HasValue || a.Action.HasValue)
                        select new SYS_Role_Action
                        {
                            ID = a.ID,
                            RoleId = r.ID,
                            ActionId = a.ActionId,
                            Action = a.Permit.Value,
                            CreatorUserId = uid,
                            CreateTime = now
                        };
            var dlist = from d in role.Datas
                        where d.NodeType > 1 && d.Permit != d.Permission && (d.Permit.HasValue || d.Permission.HasValue)
                        select new SYS_Role_Data
                        {
                            ID = d.ID,
                            RoleId = r.ID,
                            ModuleId = (Guid)d.ParentId,
                            Mode = d.Mode,
                            ModeId = d.ModeId,
                            Permission = d.Permit.Value,
                            CreatorUserId = uid,
                            CreateTime = now
                        };
            using (var context = new BaseEntities())
            {
                context.SYS_Role.Add(r);
                context.SYS_Role_Action.AddRange(alist);
                context.SYS_Role_Data.AddRange(dlist);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除指定角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>bool 是否成功</returns>
        private bool? DeleteRole(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var role = context.SYS_Role.SingleOrDefault(r => r.ID == id && !r.BuiltIn);
                if (role == null) return null;

                context.SYS_Role.Remove(role);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 编辑指定角色
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>是否成功</returns>
        private bool? Update(Guid uid, RoleInfo role)
        {
            using (var context = new BaseEntities())
            {
                var sr = context.SYS_Role.SingleOrDefault(r => r.ID == role.ID);
                if (sr == null) return null;

                sr.Name = role.Name;
                sr.Description = role.Description;

                // 更新操作权限
                var actions = role.Actions.Where(a => a.NodeType > 1 && a.Permit != a.Action && (a.Permit.HasValue || a.Action.HasValue));
                foreach (var action in actions)
                {
                    var pa = context.SYS_Role_Action.SingleOrDefault(p => p.ID == action.ID);
                    if (pa == null && action.Permit.HasValue && !action.Action.HasValue)
                    {
                        var ia = new SYS_Role_Action
                        {
                            ID = Guid.NewGuid(),
                            RoleId = role.ID,
                            ActionId = action.ActionId,
                            Action = action.Permit.Value,
                            CreatorUserId = uid,
                            CreateTime = DateTime.Now
                        };
                        context.SYS_Role_Action.Add(ia);
                        continue;
                    }

                    if (pa == null) return null;

                    if (action.Permit.HasValue)
                    {
                        pa.Action = action.Permit.Value;
                    }
                    else
                    {
                        context.SYS_Role_Action.Remove(pa);
                    }
                }

                // 更新数据权限
                var datas = role.Datas.Where(d => d.NodeType > 1 && d.Permit != d.Permission && (d.Permit.HasValue || d.Permission.HasValue));
                foreach (var data in datas)
                {
                    var pd = context.SYS_Role_Data.SingleOrDefault(p => p.ID == data.ID);
                    if (pd == null && data.Permit.HasValue && !data.Permission.HasValue)
                    {
                        var id = new SYS_Role_Data
                        {
                            ID = Guid.NewGuid(),
                            RoleId = role.ID,
                            ModuleId = data.ParentId.Value,
                            Mode = data.Mode,
                            ModeId = data.ModeId,
                            Permission = data.Permit.Value,
                            CreatorUserId = uid,
                            CreateTime = DateTime.Now
                        };
                        context.SYS_Role_Data.Add(id);
                        continue;
                    }

                    if (pd == null) return null;

                    if (data.Permit.HasValue)
                    {
                        pd.Permission = data.Permit.Value;
                    }
                    else
                    {
                        context.SYS_Role_Data.Remove(pd);
                    }
                }

                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 获取角色总数
        /// </summary>
        /// <returns>int 角色总数</returns>
        private int GetRoleCount()
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Role.Count(r => r.Validity);
            }
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>角色信息结果集合</returns>
        private List<RoleInfo> GetRoles(int rows, int page)
        {
            using (var context = new BaseEntities())
            {
                var skip = rows*(page - 1);
                var list = from r in context.SYS_Role.Where(r => r.Validity).OrderBy(r => r.SN).Skip(skip).Take(rows)
                           select new RoleInfo{ID = r.ID, Name = r.Name, Description = r.Description, BuiltIn = r.BuiltIn};
                return list.ToList();
            }
        }

        /// <summary>
        /// 根据ID获取角色信息
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>RoleInfo 角色对象</returns>
        private RoleInfo GetRole(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var role = context.SYS_Role.Single(r => r.ID == id);
                var obj = new RoleInfo
                {
                    ID = role.ID,
                    BuiltIn = role.BuiltIn,
                    Name = role.Name,
                    Description = role.Description,
                    Members = context.RoleMember.Where(m => m.RoleId == id).ToList(),
                    Actions = context.RoleAction.Where(a => a.RoleId == id).ToList(),
                    Datas = context.RoleData.Where(d => d.RoleId == id).ToList()
                };
                return obj;
            }
        }

        /// <summary>
        /// 获取角色成员用户总数
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>int 成员用户总数</returns>
        private int GetUsersCount(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.RoleMemberUser.Count(u => u.RoleId == id);
            }
        }

        /// <summary>
        /// 获取角色操作权限集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<RoleAction> GetActions(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.RoleAction.Where(a => a.RoleId == id).ToList();
            }
        }

        /// <summary>
        /// 获取角色数据权集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<RoleData> GetPermDatas(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.RoleData.Where(d => d.RoleId == id).ToList();
            }
        }

        /// <summary>
        /// 获取角色成员信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private List<RoleMember> GetMembers(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.RoleMember.Where(m => m.RoleId == id).ToList();
            }
        }

        /// <summary>
        /// 获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>角色成员用户集合</returns>
        private List<RoleMemberUser> GetMemberUsers(Guid id, int rows, int page)
        {
            using (var context = new BaseEntities())
            {
                var skip = rows*(page - 1);
                return context.RoleMemberUser.Where(u => u.RoleId == id).OrderBy(m => m.LoginName).Skip(skip).Take(rows).ToList();
            }
        }

        /// <summary>
        /// 保存角色成员到数据库
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">角色成员对象集合</param>
        /// <param name="uid">登录用户ID</param>
        /// <returns>bool 是否保存成功</returns>
        private bool AddRoleMember(Guid id, List<RoleMember> members, Guid uid)
        {
            using (var context = new BaseEntities())
            {
                var data = from m in members
                           select new SYS_Role_Member
                           {
                               ID = m.ID,
                               Type = m.NodeType,
                               RoleId = id,
                               MemberId = m.MemberId,
                               CreatorUserId = uid,
                               CreateTime = DateTime.Now
                           };
                context.SYS_Role_Member.AddRange(data);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        private Result DeleteRoleMember(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var result = new Result();
                var obj = context.SYS_Role_Member.SingleOrDefault(m => m.ID == id);
                if (obj == null)
                {
                    result.NotFound();
                    return result;
                }

                context.SYS_Role_Member.Remove(obj);
                try
                {
                    context.SaveChanges();
                    var role = GetRole(obj.RoleId);
                    result.Success(role);
                    return result;
                }
                catch
                {
                    result.DataBaseError();
                    return result;
                }

            }
        }

        /// <summary>
        /// 获取非角色成员的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>组织机构集合</returns>
        private IEnumerable<object> GetOtherTitle(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var list = from o in context.SYS_Organization
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == id && r.Type == 3) on o.ID equals r.MemberId into temp
                           from t in temp.DefaultIfEmpty()
                           where t == null
                           select new { o.ID, o.ParentId, o.Index, o.NodeType, o.Name };
                return list.OrderBy(o => o.Index).ToList();
            }
        }

        /// <summary>
        /// 获取非角色成员的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>用户组集合</returns>
        private IEnumerable<object> GetOtherGroup(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var list = from g in context.SYS_UserGroup.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == id && r.Type == 2) on g.ID equals r.MemberId into temp
                           from t in temp.DefaultIfEmpty()
                           where g.Visible && t == null
                           select new { g.ID, g.Name, g.Description };
                return list.ToList();
            }
        }

        /// <summary>
        /// 获取非角色成员的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>用户集合</returns>
        private IEnumerable<object> GetOtherUser(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == id && r.Type == 1) on u.ID equals r.MemberId into temp
                           from t in temp.DefaultIfEmpty()
                           where u.Validity && u.Type > 0 && t == null
                           select new { u.ID, u.Name, u.LoginName, u.Description };
                return list.ToList();
            }
        }

        /// <summary>
        /// 获取全部可用操作资源
        /// </summary>
        /// <param name="rid">角色ID</param>
        /// <returns>可用操作资源集合</returns>
        private IEnumerable<ActionInfo> GetAllActions(Guid rid)
        {
            using (var context = new BaseEntities())
            {
                var ids = from r in context.SYS_Role_Action.Where(r => r.RoleId == rid)
                          join a in context.SYS_ModuleAction.Where(a => a.Validity) on r.ActionId equals a.ID
                          select a.ModuleId;
                var gl = from g in context.SYS_ModuleGroup
                         join m in context.SYS_Module.Where(m => m.Validity) on g.ID equals m.ModuleGroupId
                         select new ActionInfo { ID = g.ID, Index = g.Index, NodeType = 0, Name = g.Name };
                var ml = from m in context.SYS_Module.Where(m => m.Validity)
                         let perm = ids.Any(id => id == m.ID) ? (int?)1 : null
                         select new ActionInfo { ID = m.ID, ParentId = m.ModuleGroupId, Permit = perm, Index = m.Index, NodeType = 1, Name = m.ApplicationName };
                var al = from a in context.SYS_ModuleAction.Where(a => a.Validity)
                         join m in context.SYS_Module.Where(m => m.Validity) on a.ModuleId equals m.ID
                         let t = context.SYS_Role_Action.FirstOrDefault(r => r.RoleId == rid && r.ActionId == a.ID)
                         let perm = t == null ? null : (int?) t.Action
                         let id = perm.HasValue ? t.ID : Guid.NewGuid()
                         let desc = perm.HasValue ? (perm.Value == 0 ? "拒绝" : "允许") : null
                         select new ActionInfo {ID = id, ParentId = a.ModuleId, ActionId = a.ID, Action = perm, Permit = perm, Index = a.Index, NodeType = 2, Name = a.Alias, Description = desc};
                var list = new List<ActionInfo>();
                list.AddRange(gl.Distinct());
                list.AddRange(ml.Distinct());
                list.AddRange(al.Distinct());
                return list;
            }
        }

        /// <summary>
        /// 获取全部可用数据资源
        /// </summary>
        /// <param name="rid">角色ID</param>
        /// <returns>可用数据资源集合</returns>
        private IEnumerable<DataInfo> GetAllDatas(Guid rid)
        {
            using (var context = new BaseEntities())
            {
                var ids = from r in context.SYS_Role_Data.Where(r => r.RoleId == rid)
                          select r.ModuleId;
                var gl = from g in context.SYS_ModuleGroup
                         join m in context.SYS_Module.Where(m => m.Validity && m.Name != null) on g.ID equals m.ModuleGroupId
                         select new DataInfo {ID = g.ID, Index = g.Index, NodeType = 0, Name = g.Name};
                var ml = from m in context.SYS_Module.Where(m => m.Validity && m.Name != null)
                         let perm = ids.Any(id => id == m.ID) ? (int?)1 : null
                         select new DataInfo {ID = m.ID, ParentId = m.ModuleGroupId, Permit = perm, Index = m.Index, NodeType = 1, Name = m.Name + "数据"};
                var list = new List<DataInfo>();
                list.AddRange(gl.Distinct());
                list.AddRange(ml.Distinct());
                foreach (var m in ml)
                {
                    var dl0 = from d in context.SYS_Data
                              let t = context.SYS_Role_Data.FirstOrDefault(r => r.Mode == 0 && r.RoleId == rid && r.ModuleId == m.ID && r.ModeId == d.ID)
                              let perm = t == null ? null : (int?) t.Permission
                              let id = perm.HasValue ? t.ID : Guid.NewGuid()
                              let desc = perm.HasValue ? (perm.Value == 0 ? "只读" : "读写") : null
                              select new DataInfo {ID = id, ParentId = m.ID, Mode = 0, ModeId = d.ID, Permission = perm, Permit = perm, Index = d.Type, NodeType = d.Type + 2, Name = d.Alias, Description = desc};
                    var dl1 = from r in context.SYS_Role_Data.Where(r => r.RoleId == rid && r.ModuleId == m.ID && r.Mode == 1)
                              join u in context.SYS_User on r.ModeId equals u.ID
                              let desc = r.Permission == 0 ? "只读" : "读写"
                              select new DataInfo {ID = r.ID, ParentId = m.ID, Mode = 1, ModeId = r.ModeId, Permission = r.Permission, Permit = r.Permission, Index = 6, NodeType = 3, Name = u.Name, Description = desc};
                    var dl2 = from r in context.SYS_Role_Data.Where(r => r.RoleId == rid && r.ModuleId == m.ID && r.Mode == 2)
                              join o in context.SYS_Organization on r.ModeId equals o.ID
                              let desc = r.Permission == 0 ? "只读" : "读写"
                              select new DataInfo {ID = r.ID, ParentId = m.ID, Mode = 2, ModeId = r.ModeId, Permission = r.Permission, Permit = r.Permission, Index = 7, NodeType = 4, Name = o.FullName, Description = desc};
                    list.AddRange(dl0);
                    list.AddRange(dl1);
                    list.AddRange(dl2);
                }
                return list;
            }
        }
    }
}
