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
    public partial class Roles : IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result AddRole(RoleInfo role)
        {
            const string action = "10B574A2-1A69-4273-87D9-06EDA77B80B6";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!InsertData(verify.Basis.UserId, role))
            {
                result.DataBaseError();
                return result;
            }
            var r = GetRole(role.ID);
            result.Created(r);
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

            var del = DeleteRole(parse.Value);
            if (!del.HasValue)
            {
                result.NotFound();
                return result;
            }

            if (!del.Value) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="role">RoleInfo</param>
        /// <returns>Result</returns>
        public Result EditRole(string id, RoleInfo role)
        {
            const string action = "4DC0141D-FE3D-4504-BE70-763028796808";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var data = Update(verify.Basis.UserId, role);
            if (!data.HasValue)
            {
                verify.Result.NotFound();
                return result;
            }

            if (!data.Value) result.DataBaseError();

            role = GetRole(parse.Value);
            result.Success(role);
            return result;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetAllRole(string rows, string page)
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

            var list = GetRoles(ipr.Value, ipp.Value);
            var count = GetRoleCount();
            if (list.Any()) result.Success(list, count);
            else result.NoContent();

            return result;
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

            if (!AddRoleMember(parse.Value, members, verify.Basis.UserId)) result.DataBaseError();

            var role = GetRole(parse.Value);
            result.Success(role);
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
            return !parse.Result.Successful ? parse.Result : DeleteRoleMember(parse.Value);
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

            var list = GetMemberUsers(parse.Value, ipr.Value, ipp.Value);
            var count = GetUsersCount(parse.Value);
            if (list.Any()) result.Success(list, count);
            else result.NoContent();

            return result;
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

            var list = GetOtherTitle(parse.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
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

            var list = GetOtherGroup(parse.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
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

            var list = GetOtherUser(parse.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取可用的操作资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        public Result GetActions(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var list = GetAllActions(parse.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取可用的数据资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>Result</returns>
        public Result GetDatas(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var list = GetAllDatas(parse.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }
    }
}
