using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.WS.Base.Common
{
    public class Authority
    {
        /// <summary>
        /// 业务模块ID
        /// </summary>
        private Guid ModuleId;

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
        /// <param name="mid">业务模块ID</param>
        /// <param name="uid">登录用户ID</param>
        /// <param name="did">登录部门ID（默认为空）</param>
        public Authority(Guid mid, Guid uid, Guid? did = null)
        {
            ModuleId = mid;
            UserId = uid;
            DeptId = did;
        }

        /// <summary>
        /// 根据登录用户ID和登录部门ID，获取用户的角色集合
        /// </summary>
        /// <returns>角色ID集合</returns>
        public IQueryable<Guid> GetUserRoles()
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
}
