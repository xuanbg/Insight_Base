using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Services
{
    public partial class Roles
    {
        /// <summary>
        /// 将角色数据插入数据库
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>角色ID</returns>
        private object InsertData(Guid uid, RoleInfo role)
        {
            var asql = "insert SYS_Role_Action(RoleId, ActionId, Action, CreatorUserId) ";
            asql += "select @RoleId, @ActionId, @Action, @CreatorUserId";
            var actions = from a in role.Actions
                select new[]
                {
                    new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) {Value = role.ID},
                    new SqlParameter("@ActionId", SqlDbType.UniqueIdentifier) {Value = a.ID},
                    new SqlParameter("@Action", a.Permit),
                    new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid},
                    new SqlParameter("@Read", SqlDbType.Int) {Value = 0}
                }
                into p
                select SqlHelper.MakeCommand(asql, p);

            var dsql = "insert SYS_RolePerm_Data(RoleId, ModuleId, Mode, Permission, CreatorUserId) ";
            dsql += "select @RoleId, @ModuleId, @Mode, @Permission, @CreatorUserId";
            var datas = from d in role.Datas
                select new[]
                {
                    new SqlParameter("@RoleId", SqlDbType.UniqueIdentifier) {Value = role.ID},
                    new SqlParameter("@ModuleId", SqlDbType.UniqueIdentifier) {Value = d.ParentId},
                    new SqlParameter("@Mode", d.Type - 2),
                    new SqlParameter("@Permission", d.Permit),
                    new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid},
                    new SqlParameter("@Read", SqlDbType.Int) {Value = 0}
                }
                into p
                select SqlHelper.MakeCommand(dsql, p);

            var sql = "insert SYS_Role (Name, Description, CreatorUserId) ";
            sql += "select @Name, @Description, @CreatorUserId;";
            sql += "select ID from SYS_Role where SN = scope_identity()";
            var parm = new[]
            {
                new SqlParameter("@Name", role.Name),
                new SqlParameter("@Description", role.Description),
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid},
                new SqlParameter("@Write", SqlDbType.Int) {Value = 0}
            };
            var cmds = new List<SqlCommand> {SqlHelper.MakeCommand(sql, parm)};
            cmds.AddRange(actions);
            cmds.AddRange(datas);
            return SqlHelper.SqlExecute(cmds, 0);
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
                return context.SaveChanges() > 0;
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
                foreach (var action in role.Actions)
                {
                    var pa = context.SYS_Role_Action.SingleOrDefault(p => p.RoleId == role.ID && p.ActionId == action.ID);
                    if (pa == null && action.Permit.HasValue && !action.state.HasValue)
                    {
                        var ia = new SYS_Role_Action
                        {
                            ID = Guid.NewGuid(),
                            RoleId = role.ID,
                            ActionId = action.ID,
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
                foreach (var data in role.Datas)
                {
                    var pd = context.SYS_Role_Data.SingleOrDefault(p => p.ID == data.ID);
                    if (pd == null && data.Permit.HasValue && !data.state.HasValue)
                    {
                        var id = new SYS_Role_Data
                        {
                            ID = Guid.NewGuid(),
                            Mode = 0,
                            RoleId = role.ID,
                            // ReSharper disable once PossibleInvalidOperationException
                            ModuleId = data.ParentId.Value,
                            Permission = data.Permit.Value,
                            ModeId = data.ID,
                            CreatorUserId = uid,
                            CreateTime = DateTime.Now
                        };
                        context.SYS_RolePerm_Data.Add(id);
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
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色信息结果集</returns>
        private IEnumerable<object> GetRoles()
        {
            using (var context = new BaseEntities())
            {
                var members = context.RoleMember;
                var users = context.RoleUser;
                var roles = from r in context.SYS_Role.OrderBy(r => r.SN)
                            where r.Validity
                            select new
                            {
                                r.ID,
                                r.BuiltIn,
                                r.Name,
                                r.Description,
                                Members = members.Where(m => m.RoleId == r.ID).ToList(),
                                Users = users.Where(u => u.RoleId == r.ID).ToList(),
                                Actions = context.Get_RoleActionPermit(r.ID).OrderBy(a => a.Index).ToList(),
                                Datas = context.Get_RoleDataPermit(r.ID).OrderBy(d => d.Index).ToList()
                            };
                return roles.ToList();
            }
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <param name="type">成员类型</param>
        /// <returns>bool 是否删除成功</returns>
        private bool DeleteRoleMember(Guid id, string type)
        {
            var sql = $"Delete from {(type == "3" ? "SYS_Role_Title" : (type == "2" ? "SYS_Role_UserGroup" : "SYS_Role_User"))} where ID = '{id}'";
            return SqlHelper.SqlNonQuery(SqlHelper.MakeCommand(sql)) > 0;
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
                var list = from o in context.OrgInfo
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
        /// 读取指定角色的操作资源集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>操作资源集合</returns>
        private IEnumerable<object> GetRoleActions(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.Get_RoleAction(id).OrderBy(a => a.Index).ToList();
            }
        }

        /// <summary>
        /// 读取指定角色的数据资源集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>数据资源集合</returns>
        private IEnumerable<object> GetRoleDatas(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.Get_RoleData(id).OrderBy(d => d.Index).ToList();
            }
        }

    }
}
