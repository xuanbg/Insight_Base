using System;
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
    public class Users : ServiceBase, IUsers
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result<object> AddUser(User user)
        {
            if (!Verify("newUser")) return result;

            if (Core.IsExisted(user)) return result.AccountExists();

            user.id = Util.NewId();
            user.password = Util.Hash("123456");
            user.creatorId = userId;
            user.createTime = DateTime.Now;
            
            return DbHelper.Insert(user) ? result.Created(user) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveUser(string id)
        {
            if (!Verify("deleteUser")) return result;

            var user = Core.GetUserById(id);
            if (user == null) return result.NotFound();

            user.isInvalid = true;

            return DbHelper.Update(user) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user">用户数据对象</param>
        /// <returns>Result</returns>
        public Result<object> UpdateUserInfo(string id, User user)
        {
            if (!Verify("editUser")) return result;

            var data = Core.GetUserById(user.id);
            if (data == null) return result.NotFound();

            data.name = user.name;
            data.remark = user.remark;
            if (!DbHelper.Update(data)) return result.DataBaseError();

            var session = Core.GetToken(user.id);
            if (session == null) return result;

            session.userName = user.name;
            session.SetChanged();
            Core.SetTokenCache(session);

            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result<object> GetUser(string id)
        {
            if (!Verify("getUsers")) return result;

            var data = Core.GetUserById(id);
            if (data == null) return result.NotFound();

            data.password = null;
            data.payPassword = null;
            data.funts = null;
            return result.Success(data);
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetUsers(int rows, int page, string key)
        {
            if (!Verify("getUsers")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var list = from u in context.users.Where(u => string.IsNullOrEmpty(key) || u.name.Contains(key) || u.account.Contains(key) || u.mobile.Contains(key) || u.email.Contains(key))
                    select new{u.id,u.name,u.account,u.mobile,u.email,u.remark,u.isBuiltin,u.isInvalid,u.creatorId,u.createTime};
                var skip = rows*(page - 1);
                var users = list.OrderBy(u => u.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(users, list.Count());
            }
        }

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="code">验证码</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result<object> SignUp(string appId, string code, User user)
        {
            if (!Verify()) return result;

            if (!Core.VerifySmsCode(1, user.mobile, code)) return result.SMSCodeError();

            if (Core.IsExisted(user)) return result.AccountExists();

            user.id = Util.NewId();
            user.creatorId = user.id;
            user.createTime = DateTime.Now;
            if (!DbHelper.Insert(user)) return result.DataBaseError();

            var session = Core.GetToken(user.id);
            var tokens = session.CreatorKey(Util.NewId("N"), appId);
            Core.SetTokenCache(session);

            return result.Created(tokens);
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="password">新密码（RSA加密）</param>
        /// <returns>Result</returns>
        public Result<object> UpdateSignature(string id, string password)
        {
            if (!Verify("reset", id)) return result;

            var user = Core.GetUserById(id);
            if (user == null) return result.NotFound();

            if (user.password == password) result.BadRequest("密码不能与原密码相同");

            user.password = password;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            var session = Core.GetToken(id);
            if (session == null) return result;

            session.password = password;
            session.SetChanged();
            Core.SetTokenCache(session);

            return result;
        }

        /// <summary>
        /// 用户重置登录密码
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码（RSA加密）</param>
        /// <param name="code">短信验证码</param>
        /// <param name="mobile">手机号，默认为空。如为空，则使用account</param>
        /// <returns>Result</returns>
        public Result<object> ResetSignature(string appId, string account, string password, string code, string mobile = null)
        {
            if (!Core.VerifySmsCode(2, mobile ?? account, code)) return result.SMSCodeError();

            userId = Core.GetUserId(account);
            if (userId == null) return result.NotFound();

            token = Core.GetToken(userId);
            if (token.password != password)
            {
                var user = Core.GetUserById(userId);
                user.password = password;
                if (!DbHelper.Update(user)) return result.DataBaseError();
            }

            token.password = password;
            var tokens = token.CreatorKey(Util.NewId("N"), appId);
            Core.SetTokenCache(token);

            return result.Created(tokens);
        }

        /// <summary>
        /// 为指定的登录账号设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="invalid">失效状态</param>
        /// <returns>Result</returns>
        public Result<object> SetUserStatus(string id, bool invalid)
        {
            var action = invalid ? "banned" : "release";
            if (!Verify(action)) return result;

            var user = Core.GetUserById(id);
            if (user == null) return result.NotFound();

            if (user.isInvalid == invalid) return result;

            user.isInvalid = invalid;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            var session = Core.GetToken(user.id);
            if (session == null) return result;

            session.isInvalid = invalid;
            session.SetChanged();
            Core.SetTokenCache(session);

            return result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="id">用户账号</param>
        /// <returns>Result</returns>
        public Result<object> UserSignOut(string id)
        {
            if (!Verify()) return result;

            token.DeleteKeys(tokenId);
            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="deptid">登录部门ID</param>
        /// <returns>Result</returns>
        public Result<object> GetUserRoles(string id, string deptid)
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                var list = context.userRoles.ToList();
                return result.Success(list);
            }
        }

    }
}