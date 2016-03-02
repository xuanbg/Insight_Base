using System;
using System.Collections.Generic;
using System.Linq;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using Insight.WS.Base.Users;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base
{
    public partial class BaseService : IUsers
    {

        #region User

        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="user">用户对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddUser(string account, SYS_User user)
        {
            const string action = "60D5BE64-0102-4189-A999-96EDAD3DA1B5";
            var verify = new SessionVerify();

            // 用户注册，验证用户签名
            if (verify.Basis == null)
            {
                var session = verify.Session;
                var sign = Hash(session.LoginName + user.LoginName + user.Password);
                if (sign != session.Signature) return verify.Result.InvalidAuth();

                if (!InsertData(user)) return verify.Result.DataBaseError();

                // 返回用于验证的Key
                session.Signature = Hash(account.ToUpper() + user.Password);
                session = SessionManage.GetSession(session);
                return verify.Result.Created(CreateKey(session));
            }

            // 管理员添加用户，验证管理员身份及鉴权
            if (!verify.Compare(action)) return verify.Result;

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
            var verify = new SessionVerify();
            if (!verify.CompareAsID(action, id)) return verify.Result;

            return DeleteUser(verify.Basis.UserId) ? verify.Result : verify.Result.DataBaseError();
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateUserInfo(string id, SYS_User user)
        {
            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new SessionVerify();
            if (!verify.CompareAsID(action, id)) return verify.Result;

            var reset = Update(user);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            SessionManage.UpdateSession(user);
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
            var verify = new SessionVerify();
            if (!verify.CompareAsID(action, id)) return verify.Result;

            var user = GetUser(verify.Guid);
            return user == null ? verify.Result.NotFound() : verify.Result.Success(user);
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUsers()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetUserList();
            return data.Rows.Count > 0 ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateSignature(string account, string password)
        {
            const string action = "26481E60-0917-49B4-BBAA-2265E71E7B3F";
            var verify = new SessionVerify();
            var session = verify.Basis;
            if (!StringCompare(session.LoginName, account)) session = SessionManage.GetSession(account);

            if (!verify.Compare(action, account)) return verify.Result;

            // 调用信分宝接口修改信分宝用户密码
            if (session != null && session.UserType <= 0)
            {
                var xresult = XFBInterface.ChangXFBPassword(account, password, session.Signature);
                if (xresult?.resultCode != "0") return verify.Result.XfbInterfaceFail(xresult?.resultMessage);
            }

            var reset = Update(account, password);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            if (session == null) return verify.Result;

            session.Signature = Hash(session.LoginName.ToUpper() + password);
            return verify.Result.Success(CreateKey(session));
        }

        /// <summary>
        /// 用户重置登录密码
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <param name="code">短信验证码</param>
        /// <returns>JsonResult</returns>
        public JsonResult ResetSignature(string account, string password, string code)
        {
            var verify = new SessionVerify();
            var session = verify.Basis;
            if (session == null) return verify.Result.NotFound();

            var sign = Hash(session.LoginName.ToUpper() + code + password);
            if (verify.Session.Signature != sign) return verify.Result.InvalidAuth();

            // 验证短信验证码
            var mobile = session.LoginName;
            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == 2);
            if (record == null) return verify.Result.SMSCodeError();

            SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == 2);

            // 调用信分宝接口修改信分宝用户密码
            var xresult = XFBInterface.ChangXFBPassword(account, password, session.Signature);
            if (xresult?.resultCode != "0") return verify.Result.XfbInterfaceFail(xresult?.resultMessage);

            // 更新用户登录密码
            var reset = Update(account, password);
            if (reset == null || !reset.Value) return verify.Result.DataBaseError();

            session.Signature = Hash(account.ToUpper() + password);
            return verify.Result.Success(CreateKey(session));
        }

        /// <summary>
        /// 为指定的登录账号设置用户状态
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="validity">可用状态</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetUserStatus(string account, bool validity)
        {
            var action = validity ? "369548E9-C8DB-439B-A604-4FDC07F3CCDD" : "0FA34D43-2C52-4968-BDDA-C9191D7FCE80";
            var verify = new SessionVerify();
            if (!verify.Compare(action, account)) return verify.Result;

            var reset = Update(verify.Guid, validity);
            if (!reset.HasValue) return verify.Result.NotFound();

            if (!reset.Value) return verify.Result.DataBaseError();

            SessionManage.SetValidity(account, validity);
            return verify.Result;
        }

        /// <summary>
        /// 获取用户登录结果
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignIn(string account)
        {
            var verify = new SessionVerify();
            if (!verify.Compare(null, false)) return verify.Result;

            // 更新缓存信息
            verify.Basis.OpenId = verify.Session.OpenId;
            verify.Basis.MachineId = verify.Session.MachineId;
            verify.Basis.DeptId = verify.Session.DeptId;
            verify.Basis.DeptName = verify.Session.DeptName;
            verify.Basis.Expired = DateTime.Now.AddHours(Expired);

            // 返回用于验证的Key
            var key = CreateKey(verify.Basis);
            return verify.Result.Success(key);
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignOut(string account)
        {
            var action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new SessionVerify();
            if (verify.Basis.LoginName == account) action = null;

            if (!verify.Compare(action)) return verify.Result;

            SessionManage.Offline(account);
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
            var verify = new SessionVerify();
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
            var verify = new SessionVerify();
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
            var verify = new SessionVerify();
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
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetGroup(verify.Guid);
            return data == null ? verify.Result.NoContent() : verify.Result.Success(data);
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroups()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetGroupList();
            return data.Rows.Count > 0 ? verify.Result.Success(data) : verify.Result.NoContent();
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
            var verify = new SessionVerify();
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
            var verify = new SessionVerify();
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
            var verify = new SessionVerify();
            if (!verify.Compare(action)) return verify.Result;

            var data = GetMemberList();
            return data.Rows.Count > 0 ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOtherUser(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new SessionVerify();
            if (!verify.ParseIdAndCompare(id, action)) return verify.Result;

            var data = GetOtherUser(verify.Guid);
            return data.Rows.Count > 0 ? verify.Result.Success(data) : verify.Result.NoContent();
        }

        #endregion

    }
}
