using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Roles : IRoles
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="role">RoleInfo</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddRole(RoleInfo role)
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
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRole(string id)
        {
            const string action = "FBCEE515-8576-4B10-BA68-CF46743D2199";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var del = DeleteRole(rid.Value);
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
        /// <returns>JsonResult</returns>
        public JsonResult EditRole(string id, RoleInfo role)
        {
            const string action = "4DC0141D-FE3D-4504-BE70-763028796808";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue || rid.Value != role.ID)
            {
                result.BadRequest();
                return result;
            }

            var data = Update(verify.Basis.UserId, role);
            if (!data.HasValue)
            {
                verify.Result.NotFound();
                return result;
            }

            if (!data.Value) result.DataBaseError();

            role = GetRole(rid.Value);
            result.Success(role);
            return result;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetAllRole(string rows, string page)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            int r;
            if (!int.TryParse(rows, out r))
            {
                result.BadRequest();
                return result;
            }

            int p;
            if (!int.TryParse(page, out p))
            {
                result.BadRequest();
                return result;
            }

            if (r < 20 || r > 100 || p < 1)
            {
                result.BadRequest();
                return result;
            }

            var list = GetRoles(r, p);
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
        /// <returns>JsonResult</returns>
        public JsonResult AddRoleMember(string id, List<RoleMember> members)
        {
            const string action = "13D93852-53EC-4A15-AAB2-46C9C48C313A";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!members.Any() || !rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            if (!AddRoleMember(rid.Value, members, verify.Basis.UserId)) result.DataBaseError();

            var role = GetRole(rid.Value);
            result.Success(role);
            return result;
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRoleMember(string id)
        {
            const string action = "2EF4D82B-4A75-4902-BD9E-B63153D093D2";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var mid = new GuidParse(id).Result;
            if (mid.HasValue) return DeleteRoleMember(mid.Value);

            result.BadRequest();
            return result;
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfTitle(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var list = GetOtherTitle(rid.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfGroup(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var list = GetOtherGroup(rid.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfUser(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var list = GetOtherUser(rid.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取可用的操作资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetActions(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var list = GetAllActions(rid.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取可用的数据资源列表
        /// </summary>
        /// <param name="id">角色ID（可为空）</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetDatas(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var rid = new GuidParse(id).Result;
            if (!rid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var list = GetAllDatas(rid.Value);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }
    }
}
