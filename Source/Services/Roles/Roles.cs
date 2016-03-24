using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Roles : IRoles
    {
        public JsonResult AddRole(SYS_Role role, DataTable action, DataTable data, DataTable custom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRole(string id)
        {
            const string action = "FBCEE515-8576-4B10-BA68-CF46743D2199";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var result = DeleteRole(verify.Guid);
            if (!result.HasValue) return verify.Result.NotFound();

            return result.Value ? verify.Result : verify.Result.DataBaseError();
        }

        public JsonResult EditRole(string id, SYS_Role obj, List<object> adl, List<object> ddl, List<object> cdl, DataTable adt, DataTable ddt, DataTable cdt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetAllRole()
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var list = GetRoles();
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        public JsonResult AddRoleMember(string id, List<string> tids, List<string> gids, List<string> uids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <param name="type">成员类型</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRoleMember(string id, string type)
        {
            const string action = "2EF4D82B-4A75-4902-BD9E-B63153D093D2";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            return DeleteRoleMember(verify.Guid, type) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfTitle(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var list = GetOtherTitle(verify.Guid);
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfGroup(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var list = GetOtherGroup(verify.Guid);
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetMemberOfUser(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var list = GetOtherUser(verify.Guid);
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        /// <summary>
        /// 获取指定角色的所有功能操作和授权
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetRoleActions(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var list = GetRoleActions(verify.Guid);
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

        /// <summary>
        /// 获取指定角色的所有相对数据权限
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetRoleRelData(string id)
        {
            const string action = "3BC74B61-6FA7-4827-A4EE-E1317BF97388";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var list = GetRoleDatas(verify.Guid);
            return list.Any() ? verify.Result.Success(list) : verify.Result.NoContent();
        }

    }
}
