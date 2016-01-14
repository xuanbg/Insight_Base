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
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            var aid = Guid.Parse("60D5BE64-0102-4189-A999-96EDAD3DA1B5");
            if (session.UserId != user.ID && !DataAccess.Authority(session, aid)) return result.Forbidden();

            return InsertData(user) ? result.Created() : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveUser(string id)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var aid = Guid.Parse("BE2DE9AB-C109-418D-8626-236DEF8E8504");
            if (session.UserId != uid && !DataAccess.Authority(session, aid)) return result.Forbidden();

            return RemoveUser(uid) ? result.Created() : result.DataBaseError();
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateUserInfo(SYS_User user)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            var aid = Guid.Parse("3BC17B61-327D-4EAA-A0D7-7F825A6C71DB");
            if (session.UserId != user.ID && !DataAccess.Authority(session, aid)) return result.Forbidden();

            var reset = Update(user);
            if (!reset.HasValue) return result.NotFound();

            if (!reset.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == user.ID);
            if (session == null) return result;

            session.LoginName = user.LoginName;
            session.UserName = user.Name;
            session.UserType = user.Type;
            session.OpenId = user.OpenId;
            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetUser(string id)
        {
            var result = General.Authorize("3BC17B61-327D-4EAA-A0D7-7F825A6C71DB");
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var user = GetUser(uid);
            return user == null ? result.NotFound() : result.Success(Serialize(user));
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUsers()
        {
            var result = General.Authorize("B5992AA3-4AD3-4795-A641-2ED37AC6425C");
            if (!result.Successful) return result;

            var data = GetUserList();
            return data.Rows.Count > 0 ? result.Success(Serialize(data)) : result.NoContent();
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">用户新密码</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateSignature(string id, string pw)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var aid = Guid.Parse("26481E60-0917-49B4-BBAA-2265E71E7B3F");
            if (session.UserId != uid && !DataAccess.Authority(session, aid)) return result.Forbidden();

            var reset = Update(uid, pw);
            if (!reset.HasValue) return result.NotFound();

            if (!reset.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == uid);
            if (session != null) session.Signature = Hash(session.LoginName.ToUpper() + pw);

            return result;
        }

        /// <summary>
        /// 根据用户ID设置用户状态
        /// </summary>
        /// <param name="id"></param>
        /// <param name="validity">可用状态</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetUserStatus(string id, bool validity)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            Guid uid;
            if (!Guid.TryParse(id, out uid)) return result.InvalidGuid();

            var aid = Guid.Parse(validity ? "369548E9-C8DB-439B-A604-4FDC07F3CCDD" : "0FA34D43-2C52-4968-BDDA-C9191D7FCE80");
            if (session.UserId != uid && !DataAccess.Authority(session, aid)) return result.Forbidden();

            var reset = Update(uid, validity);
            if (!reset.HasValue) return result.NotFound();

            if (!reset.Value) return result.DataBaseError();

            session = Sessions.SingleOrDefault(s => s.UserId == uid);
            if (session != null) session.Validity = validity;

            return result;
        }

        /// <summary>
        /// 获取用户登录结果
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignIn()
        {
            var result = new JsonResult();
            var session = GetAuthorization<Session>();
            if (session == null) return result.InvalidAuth();

            var verify = new Verify(session);
            result = verify.Compare();
            if (!result.Successful) return result;

            verify.Basis.DeptId = session.DeptId;
            verify.Basis.DeptName = session.DeptName;
            verify.Basis.MachineId = session.MachineId;
            return result.Success(Serialize(verify.Basis));
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignOut(string account)
        {
            Session session;
            var result = General.Verify(out session);
            if (!result.Successful) return result;

            var aid = Guid.Parse("331BF752-CDB7-44DE-9631-DF2605BB527E");
            if (session.LoginName != account && !DataAccess.Authority(session, aid)) return result.Forbidden();

            session = Sessions.SingleOrDefault(s => s.LoginName == account);
            if (session != null) session.OnlineStatus = false;

            return result;
        }

        #endregion

        #region UserGroup

        /// <summary>
        /// 根据对象实体数据新增一个用户组
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddGroup(SYS_UserGroup @group)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveGroup(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateGroup(SYS_UserGroup @group)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroup(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroups()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddGroupMember(string id, List<Guid> uids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="ids">户组成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveMember(List<Guid> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取全部用户组的所有成员信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroupMembers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOtherUser(string id)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
