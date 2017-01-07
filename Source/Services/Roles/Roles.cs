using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Roles : IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result AddRole(Role role)
        {
            const string action = "10B574A2-1A69-4273-87D9-06EDA77B80B6";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (role.Existed) return role.Result;

            role.CreatorUserId = verify.Basis.UserId;
            role.CreateTime = DateTime.Now;
            if (!role.Add()) return role.Result;

            result.Created(role);
            return result;
        }

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result RemoveRole(string id)
        {
            const string action = "FBCEE515-8576-4B10-BA68-CF46743D2199";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var role = new Role(parse.Value);
            if (!role.Result.Successful) return role.Result;

            return role.Delete() ? result : role.Result;
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result EditRole(string id, Role role)
        {
            const string action = "4DC0141D-FE3D-4504-BE70-763028796808";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            if (role.Existed || !role.Update()) return role.Result;

            result.Success(role);
            return result;
        }

        /// <summary>
        /// 获取指定角色
        /// </summary>ID
        /// <param name="id">角色</param>
        /// <returns>Result</returns>
        public Result GetRole(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var role = new Role(parse.Value);
            if (!role.Result.Successful) return role.Result;

            result.Success(role);
            return result;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetRoles(string rows, string page)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.Successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.Successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1)
            {
                result.BadRequest();
                return result;
            }

            using (var context = new BaseEntities())
            {
                var list = from r in context.SYS_Role.Where(r => r.Validity).OrderBy(r => r.SN)
                           select new {r.ID, r.Name, r.Description, r.BuiltIn, r.Validity, r.CreatorUserId, r.CreateTime};
                var skip = ipr.Value*(ipp.Value - 1);
                var roles = new
                {
                    Total = list.Count(),
                    Items = list.Skip(skip).Take(ipr.Value).ToList()
                };
                result.Success(roles);
                return result;
            }
        }

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">成员对象集合</param>
        /// <returns>Result</returns>
        public Result AddRoleMember(string id, List<RoleMember> members)
        {
            const string action = "13D93852-53EC-4A15-AAB2-46C9C48C313A";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful || !members.Any()) return parse.Result;

            using (var context = new BaseEntities())
            {
                var data = from m in members
                           select new SYS_Role_Member
                           {
                               ID = m.ID,
                               Type = m.NodeType,
                               RoleId = parse.Value,
                               MemberId = m.MemberId,
                               CreatorUserId = verify.Basis.UserId,
                               CreateTime = DateTime.Now
                           };
                context.SYS_Role_Member.AddRange(data);
                try
                {
                    context.SaveChanges();
                    var role = new Role(parse.Value);
                    result.Success(role);
                }
                catch
                {
                    result.DataBaseError();
                }
            }

            return result;
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        public Result RemoveRoleMember(string id)
        {
            const string action = "2EF4D82B-4A75-4902-BD9E-B63153D093D2";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var obj = context.SYS_Role_Member.SingleOrDefault(m => m.ID == parse.Value);
                if (obj == null)
                {
                    result.NotFound();
                    return result;
                }

                context.SYS_Role_Member.Remove(obj);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    result.DataBaseError();
                }

                result.Success(new Role(obj.RoleId));
                return result;
            }
        }

        /// <summary>
        /// 根据角色ID获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetMemberUsers(string id, string rows, string page)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.Successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.Successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1)
            {
                result.BadRequest();
                return result;
            }

            using (var context = new BaseEntities())
            {
                var skip = ipr.Value*(ipp.Value - 1);
                var list = context.RoleMemberUser.Where(u => u.RoleId == parse.Value).OrderBy(m => m.SN);
                var members = new
                {
                    Total = list.Count(),
                    Items = list.Skip(skip).Take(ipr.Value).ToList()
                };
                result.Success(members);
                return result;
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result GetMemberOfTitle(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from o in context.SYS_Organization
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 3)
                           on o.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where t == null
                           select new { o.ID, o.ParentId, o.Index, o.NodeType, o.Name };
                if (list.Any()) result.Success(list.OrderBy(o => o.Index).ToList());
                else result.NoContent();

                return result;
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result GetMemberOfGroup(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from g in context.SYS_UserGroup.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 2)
                           on g.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where g.Visible && t == null
                           select new { g.ID, g.Name, g.Description };
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result GetMemberOfUser(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 1) 
                           on u.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where u.Validity && u.Type > 0 && t == null
                           select new { u.ID, u.Name, u.LoginName, u.Description };
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }

        /// <summary>
        /// 获取可用的权限资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        public Result GetAllPermission(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var role = new Role(parse.Value);
            role.GetAllPermission();
            result.Success(role);
            return result;
        }
    }
}