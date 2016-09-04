using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Users : IUsers
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
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var data = context.SYS_User.FirstOrDefault(u => u.LoginName == user.LoginName);
                if (data != null)
                {
                    result.AccountExists();
                    return result;
                }
            }
            
            user.ID = Guid.NewGuid();
            user.Password = Util.Hash("123456");
            user.Type = 1;
            user.CreatorUserId = verify.Basis.UserId;
            var id = InsertData(user);
            if (id == null) result.DataBaseError();
            else result.Created(id.ToString());

            return result;
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveUser(string id)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "BE2DE9AB-C109-418D-8626-236DEF8E8504";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteUser(uid)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="user">用户数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateUserInfo(string id, SYS_User user)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Compare(action, 0, uid);
            result = verify.Result;
            if (!result.Successful) return result;

            var reset = Update(user);
            if (!reset.HasValue)
            {
                result.NotFound();
                return result;
            }

            if (!reset.Value)
            {
                result.DataBaseError();
                return result;
            }

            TokenManage.Update(user);
            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetUser(string id)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Compare(action, 0, uid);
            result = verify.Result;
            if (!result.Successful) return result;

            var user = GetUser(uid);
            if (user == null) result.NotFound();
            else result.Success(user);

            return result;
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetUsers()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var list = GetUserList();
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 获取全部在线用户
        /// </summary>
        /// <param name="type">用户类型</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOnlineUsers(string type)
        {
            const string action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var list = TokenManage.GetOnlineUsers(Convert.ToInt32(type));
            if (list.Any()) result.Success(list);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="user">用户对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult SignUp(string account, SYS_User user)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var data = context.SYS_User.FirstOrDefault(u => u.LoginName == user.LoginName);
                if (data != null)
                {
                    result.AccountExists();
                    return result;
                }
            }

            if (InsertData(user) == null)
            {
                verify.Result.DataBaseError();
                return result;
            }

            var token = new AccessToken {Account = account};
            var session = TokenManage.Get(token);
            token.ID = session.ID;
            token.UserName = session.UserName;
            token.Secret = session.Secret;
            token.FailureTime = session.FailureTime;

            verify.Result.Data = Util.Base64(token);
            return result;
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
            var verify = new Compare(action, account);
            var result = verify.Result;
            if (!result.Successful) return result;

            var session = Util.StringCompare(verify.Basis.Account, account)
                ? verify.Basis
                : TokenManage.Get(account);
            var reset = Update(account, password);
            if (!reset.HasValue)
            {
                result.NotFound();
                return result;
            }

            if (!reset.Value)
            {
                result.DataBaseError();
                return result;
            }

            if (session == null) return result;

            session.Signature = Util.Hash(session.Account.ToUpper() + password);
            return result;
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
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var token = new AccessToken {Account = account, Stamp = verify.Basis.Stamp};
            var session = TokenManage.Get(token);
            if (session == null)
            {
                result.NotFound();
                return result;
            }

            // 验证短信验证码
            var mobile = session.Mobile;
            Parameters.SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = Parameters.SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == 2);
            if (record == null)
            {
                result.SMSCodeError();
                return result;
            }

            Parameters.SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == 2);

            // 更新数据库
            var reset = Update(account, password);
            if (reset == null || !reset.Value)
            {
                result.DataBaseError();
                return result;
            }

            session.Signature = Util.Hash(account.ToUpper() + password);
            session.Stamp = verify.Basis.Stamp;

            token.ID = session.ID;
            token.UserName = session.UserName;
            token.Secret = session.Secret;
            token.FailureTime = session.FailureTime;

            result.Data = Util.Base64(token);
            return result;
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
            var verify = new Compare(action, account);
            var result = verify.Result;
            if (!result.Successful) return result;

            var reset = Update(account, validity);
            if (!reset.HasValue)
            {
                result.NotFound();
                return result;
            }

            if (!reset.Value)
            {
                result.DataBaseError();
                return result;
            }

            TokenManage.SetValidity(account, validity);
            return result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignOut(string account)
        {
            var action = "331BF752-CDB7-44DE-9631-DF2605BB527E";

            var verify = new Compare(action, account);
            var result = verify.Result;
            if (!result.Successful) return result;

            TokenManage.Offline(account);
            return result;
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
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var id = InsertData(verify.Basis.UserId, group);
            if (id == null) result.DataBaseError();
            else result.Created(id.ToString());

            return result;
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveGroup(string id)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "E46B7A1C-A8B0-49B5-8494-BF1B09F43452";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteGroup(uid)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateGroup(string id, SYS_UserGroup @group)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!Update(group)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroup(string id)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var data = GetGroup(uid);
            if (data == null) result.NotFound();
            else result.Success(data);

            return result;
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroups()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = GetGroupList();
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddGroupMember(string id, List<Guid> uids)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "6C41724C-E118-4BCD-82AD-6B13D05C7894";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!AddGroupMember(verify.Basis.UserId, uid, uids)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="ids">户组成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveMember(List<Guid> ids)
        {
            const string action = "686C115A-CE2E-4E84-8F25-B63C15AC173C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteMember(ids)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 获取全部用户组的所有成员信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetGroupMembers()
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = GetMemberList();
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOtherUser(string id)
        {
            var result = new JsonResult();
            Guid uid;
            if (!Guid.TryParse(id, out uid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var data = GetOtherUser(uid);
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        #endregion

    }
}
