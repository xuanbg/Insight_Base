using System;
using System.Collections.Generic;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base
{
    public partial class Roles
    {
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
        /// 获取所有角色
        /// </summary>
        /// <returns>角色信息结果集</returns>
        private IEnumerable<object> GetRoles()
        {
            using (var context = new BaseEntities())
            {
                var members = context.RoleMember;
                var users = context.RoleUser;
                var modules = context.RoleModulePermit;
                var actions = context.RoleActionPermit;
                var datas = context.RoleDataPermit;
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
                                Modules = modules.Where(m => m.RoleId == r.ID).OrderBy(m => m.Index).ToList(),
                                Actions = actions.Where(a => a.RoleId == r.ID).OrderBy(a => a.Index).ToList(),
                                Datas = datas.Where(d => d.RoleId == r.ID).OrderBy(d => d.Index).ToList()
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
            return SqlNonQuery(MakeCommand(sql)) > 0;
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
                           join r in context.SYS_Role_Title.Where(r => r.RoleId == id) on o.ID equals r.OrgId into temp
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
                           join r in context.SYS_Role_UserGroup.Where(r => r.RoleId == id) on g.ID equals r.GroupId into temp
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
                           join r in context.SYS_Role_User.Where(r => r.RoleId == id) on u.ID equals r.UserId into temp
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
