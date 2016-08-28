using System;
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

            var id = InsertData(verify.Basis.UserId, role);
            if (id == null) result.DataBaseError();
            else result.Created(id.ToString());

            return result;
        }

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRole(string id)
        {
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "FBCEE515-8576-4B10-BA68-CF46743D2199";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var del = DeleteRole(rid);
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

            var data = Update(verify.Basis.UserId, role);
            if (!data.HasValue)
            {
                verify.Result.NotFound();
                return result;
            }

            if (!data.Value) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetAllRole()
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var list = GetRoles();
            if (list.Any()) result.Success(list);
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
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "13D93852-53EC-4A15-AAB2-46C9C48C313A";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var add = AddRoleMember(rid, members, verify.Basis.UserId);
            if (add == null) result.DataBaseError();
            else result.Success(result);

            return result;
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <param name="type">成员类型</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRoleMember(string id, string type)
        {
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "2EF4D82B-4A75-4902-BD9E-B63153D093D2";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteRoleMember(rid, type)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfTitle(string id)
        {
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var list = GetOtherTitle(rid);
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
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var list = GetOtherGroup(rid);
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
            var result = new JsonResult();
            Guid rid;
            if (!Guid.TryParse(id, out rid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var list = GetOtherUser(rid);
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
            var result = new JsonResult();
            var parse = new GuidParse(id);
            if (!parse.Successful)
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var list = GetAllActions(parse.Result);
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
            var result = new JsonResult();
            var parse = new GuidParse(id);
            if (!parse.Successful)
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var list = GetAllDatas(parse.Result);
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }
    }
}
