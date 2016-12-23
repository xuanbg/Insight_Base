using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            else result.Created(id);

            return result;
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveUser(string id)
        {
            const string action = "BE2DE9AB-C109-418D-8626-236DEF8E8504";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            if (!DeleteUser(uid.Value)) result.DataBaseError();

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
            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Compare(action, 0, uid);
            result = Util.ConvertTo<JsonResult>(verify.Result);
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

            var session = OAuth.Common.GetSession(user.LoginName);
            if (session == null) return result;

            session.UserName = user.Name;
            session.UserType = user.Type;
            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result GetUser(string id)
        {
            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Compare(action, 0, parse.Value);
            var result = verify.Result;
            if (!result.Successful) return result;

            var user = GetUser(parse.Value);
            if (user == null) result.NotFound();
            else result.Success(user);

            return result;
        }

        /// <summary>
        /// 获取用户授权信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result GetUserPermission(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var auth = new Authority(parse.Value, null, InitType.Permission, true);
            var user = new {Actions = auth.GetActions(), Datas = auth.GetDatas()};
            result.Success(user);
            return result;
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetUsers(string rows, string page)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
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

            var list = new TabList<UserInfo>
            {
                Total = GetUserCount(),
                Items = GetUsers(ipr.Value, ipp.Value)
            };
            result.Success(list);

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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            var session = OAuth.Common.GetSession(token);
            session.InitSecret();

            verify.Result.Success(session.CreatorKey());
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var session = Util.StringCompare(verify.Basis.Account, account)
                ? verify.Basis
                : OAuth.Common.GetSession(account);
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

            session.Sign(password);
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var token = new AccessToken {Account = account};
            var session = OAuth.Common.GetSession(token);
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

            session.Sign(password);
            session.InitSecret();

            result.Success(session.CreatorKey());
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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

            var session = OAuth.Common.GetSession(account);
            if (session != null) session.Validity = validity;

            return result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UserSignOut(string account)
        {
            const string action = "331BF752-CDB7-44DE-9631-DF2605BB527E";
            var verify = new Compare(action, account);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var session = OAuth.Common.GetSession(account);
            session?.SignOut();

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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var id = InsertData(verify.Basis.UserId, group);
            if (id == null) result.DataBaseError();
            else result.Created(id);

            return result;
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveGroup(string id)
        {
            const string action = "E46B7A1C-A8B0-49B5-8494-BF1B09F43452";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            if (!DeleteGroup(uid.Value)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="group">用户组对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateGroup(string id, SYS_UserGroup group)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var data = GetGroup(uid.Value);
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            const string action = "6C41724C-E118-4BCD-82AD-6B13D05C7894";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            if (!AddGroupMember(verify.Basis.UserId, uid.Value, uids)) result.DataBaseError();

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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            var result = Util.ConvertTo<JsonResult>(verify.Result);
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
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = Util.ConvertTo<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            var uid = new GuidParse(id).Guid;
            if (!uid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            var data = GetOtherUser(uid.Value);
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        #endregion

    }
}
