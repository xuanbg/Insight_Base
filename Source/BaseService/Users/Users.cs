using System;
using System.Collections.Generic;
using System.Linq;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base.Service
{
    public partial class BaseService : Iusers
    {

        #region User

        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddUser(SYS_User user)
        {
            const string action = "60D5BE64-0102-4189-A999-96EDAD3DA1B5";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(user.ID.ToString(), action)) return verify.Result;

            return InsertData(user) ? verify.Result.Created() : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveUser(string id)
        {
            const string action = "BE2DE9AB-C109-418D-8626-236DEF8E8504";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(id, action)) return verify.Result;

            return DeleteUser(verify.Basis.UserId) ? verify.Result.Created() : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateUserInfo(SYS_User user)
        {
            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(user.ID.ToString(), action)) return verify.Result;

            var reset = Update(user);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            var session = Sessions.SingleOrDefault(s => s.UserId == user.ID);
            if (session == null) return verify.Result;

            session.LoginName = user.LoginName;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.OpenId = user.OpenId;
            return verify.Result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetUser(string id)
        {
            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(id, action)) return verify.Result;

            var user = GetUser(verify.Guid);
            return user == null ? verify.Result.NotFound() : verify.Result.Success(Serialize(user));
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUsers()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetUserList();
            return data.Rows.Count > 0 ? verify.Result.Success(Serialize(data)) : verify.Result.NoContent();
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">用户新密码</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateSignature(string id, string pw)
        {
            const string action = "26481E60-0917-49B4-BBAA-2265E71E7B3F";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(id, action)) return verify.Result;

            var reset = Update(verify.Guid, pw);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            var session = Sessions.SingleOrDefault(s => s.UserId == verify.Guid);
            if (session != null) session.Signature = Hash(session.LoginName.ToUpper() + pw);

            return verify.Result;
        }

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="validity">可用状态</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetUserStatus(string id, bool validity)
        {
            var action = validity ? "369548E9-C8DB-439B-A604-4FDC07F3CCDD" : "0FA34D43-2C52-4968-BDDA-C9191D7FCE80";
            var verify = new Verify();
            if (!verify.ParseUserIdAndCompare(id, action)) return verify.Result;

            var reset = Update(verify.Guid, validity);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            var session = Sessions.SingleOrDefault(s => s.UserId == verify.Guid);
            if (session != null) session.Validity = validity;

            return verify.Result;
        }

        /// <summary>
        /// 获取用户登录结果
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignIn()
        {
            var verify = new Verify();
            verify.SignIn();
            return verify.Result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignOut(string account)
        {
            var action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new Verify();
            if (verify.Basis.LoginName == account) action = null;

            if (!verify.Compare(action)) return verify.Result;

            var session = Sessions.SingleOrDefault(s => s.LoginName == account);
            if (session != null) session.OnlineStatus = false;

            return verify.Result;
        }

        #endregion

        #region UserGroup

        /// <summary>
        /// 根据对象实体数据新增一个用户组
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddGroup(SYS_UserGroup group)
        {
            const string action = "6E80210E-6F80-4FF7-8520-B602934D635C";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            var id = InsertData(verify.Basis.UserId, group);
            return id == null ? verify.Result.DataBaseError() : verify.Result.Created();
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveGroup(string id)
        {
            const string action = "E46B7A1C-A8B0-49B5-8494-BF1B09F43452";
            var verify = new Verify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            return DeleteGroup(verify.Guid) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateGroup(SYS_UserGroup group)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            return Update(group) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroup(string id)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetGroup(verify.Guid);
            return data == null ? verify.Result.NoContent() : verify.Result.Success(Serialize(data));
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroups()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetGroupList();
            return data.Rows.Count > 0 ? verify.Result.Success(Serialize(data)) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddGroupMember(string id, List<Guid> uids)
        {
            const string action = "6C41724C-E118-4BCD-82AD-6B13D05C7894";
            var verify = new Verify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            return AddGroupMember(verify.Basis.UserId, verify.Guid, uids) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="ids">户组成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveMember(List<Guid> ids)
        {
            const string action = "686C115A-CE2E-4E84-8F25-B63C15AC173C";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            return DeleteMember(ids) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 获取全部用户组的所有成员信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroupMembers()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Verify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetMemberList();
            return data.Rows.Count > 0 ? verify.Result.Success(Serialize(data)) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOtherUser(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Verify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var data = GetOtherUser(verify.Guid);
            return data.Rows.Count > 0 ? verify.Result.Success(Serialize(data)) : verify.Result.NoContent();
        }

        #endregion

    }
}
