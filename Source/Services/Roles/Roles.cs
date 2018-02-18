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
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result<object> AddRole(Role role)
        {
            if (!Verify("10B574A2-1A69-4273-87D9-06EDA77B80B6")) return _Result;

            if (role.Existed) return role.Result;

            role.CreatorUserId = _UserId;
            role.CreateTime = DateTime.Now;
            return role.Add() ? _Result.Created(role) : role.Result;
        }

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveRole(string id)
        {
            if (!Verify("FBCEE515-8576-4B10-BA68-CF46743D2199")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var role = new Role(parse.Value);
            return role.Result.successful && role.Delete() ? _Result : role.Result;
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result<object> EditRole(string id, Role role)
        {
            if (!Verify("4DC0141D-FE3D-4504-BE70-763028796808")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            if (role.Existed || !role.Update()) return role.Result;

            role.Authority = new Authority(parse.Value);
            return _Result.Success(role);
        }

        /// <summary>
        /// 获取指定角色
        /// </summary>ID
        /// <param name="id">角色</param>
        /// <returns>Result</returns>
        public Result<object> GetRole(string id)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var role = new Role(parse.Value);
            return role.Result.successful ? _Result.Success(role) : role.Result;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> GetRoles(string rows, string page)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1) return _Result.BadRequest();

            using (var context = new Entities())
            {
                var list = from r in context.SYS_Role.Where(r => r.Validity).OrderBy(r => r.SN)
                           select new {r.ID, r.Name, r.Description, r.BuiltIn, r.Validity, r.CreatorUserId, r.CreateTime};
                var skip = ipr.Value*(ipp.Value - 1);
                var roles = list.Skip(skip).Take(ipr.Value).ToList();

                return _Result.Success(roles, list.Count().ToString());
            }
        }

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">成员对象集合</param>
        /// <returns>Result</returns>
        public Result<object> AddRoleMember(string id, List<RoleMember> members)
        {
            if (!Verify("13D93852-53EC-4A15-AAB2-46C9C48C313A")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful || !members.Any()) return parse.Result;

            using (var context = new Entities())
            {
                var data = from m in members
                           select new SYS_Role_Member
                           {
                               ID = m.ID,
                               Type = m.NodeType,
                               RoleId = parse.Value,
                               MemberId = m.MemberId,
                               CreatorUserId = _UserId,
                               CreateTime = DateTime.Now
                           };
                context.SYS_Role_Member.AddRange(data);
                try
                {
                    context.SaveChanges();
                    var role = new Role(parse.Value);
                    return _Result.Success(role);
                }
                catch
                {
                    return _Result.DataBaseError();
                }
            }
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveRoleMember(string id)
        {
            if (!Verify("2EF4D82B-4A75-4902-BD9E-B63153D093D2")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new Entities())
            {
                var obj = context.SYS_Role_Member.SingleOrDefault(m => m.ID == parse.Value);
                if (obj == null) return _Result.NotFound();

                context.SYS_Role_Member.Remove(obj);
                try
                {
                    context.SaveChanges();
                    return _Result.Success(new Role(obj.RoleId));
                }
                catch (Exception)
                {
                    return _Result.DataBaseError();
                }
            }
        }

        /// <summary>
        /// 根据角色ID获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberUsers(string id, string rows, string page)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1) return _Result.BadRequest();

            using (var context = new Entities())
            {
                var skip = ipr.Value*(ipp.Value - 1);
                var list = context.RoleMemberUser.Where(u => u.RoleId == parse.Value).OrderBy(m => m.SN);
                var members = list.Skip(skip).Take(ipr.Value).ToList();

                return _Result.Success(members, list.Count().ToString());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfTitle(string id)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new Entities())
            {
                var list = from o in context.SYS_Organization
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 3)
                           on o.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where t == null
                           select new { o.ID, o.ParentId, o.Index, o.NodeType, o.Name };
                return list.Any() ? _Result.Success(list.OrderBy(o => o.Index).ToList()) : _Result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfGroup(string id)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;
            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new Entities())
            {
                var list = from g in context.SYS_UserGroup.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 2)
                           on g.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where g.Visible && t == null
                           select new { g.ID, g.Name, g.Description };
                return list.Any() ? _Result.Success(list.ToList()) : _Result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfUser(string id)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new Entities())
            {
                var list = from u in context.SYS_User.OrderBy(g => g.SN)
                           join r in context.SYS_Role_Member.Where(r => r.RoleId == parse.Value && r.Type == 1) 
                           on u.ID equals r.MemberId into temp from t in temp.DefaultIfEmpty()
                           where u.Validity && u.Type > 0 && t == null
                           select new { u.ID, u.Name, u.LoginName, u.Description };
                return list.Any() ? _Result.Success(list.ToList()) : _Result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 获取可用的权限资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> GetAllPermission(string id)
        {
            if (!Verify("3BC74B61-6FA7-4827-A4EE-E1317BF97388")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var role = new Role(parse.Value);
            role.GetAllPermission();

            return _Result.Success(role);
        }

        private Result<object> _Result = new Result<object>();
        private Guid _UserId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var compare = new Verify();
            _Result = compare.Result;
            if (!_Result.successful) return false;

            _UserId = compare.basis.userId;
            _Result = compare.Compare(action);

            return _Result.successful;
        }
    }
}