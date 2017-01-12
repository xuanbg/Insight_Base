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
    public class Users : IUsers
    {
        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result AddUser(User user)
        {
            const string action = "60D5BE64-0102-4189-A999-96EDAD3DA1B5";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (user.Existed) return user.Result;

            user.Password = Util.Hash("123456");
            user.CreatorUserId = verify.Basis.UserId;
            user.CreateTime = DateTime.Now;

            if (!user.Add()) return user.Result;

            result.Created(user);
            return result;
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result RemoveUser(string id)
        {
            const string action = "BE2DE9AB-C109-418D-8626-236DEF8E8504";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var user = new User(parse.Value);
            if (!user.Result.Successful) return user.Result;

            return user.Delete() ? result : user.Result;
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="user">用户数据对象</param>
        /// <returns>Result</returns>
        public Result UpdateUserInfo(string id, User user)
        {
            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            const string action = "3BC17B61-327D-4EAA-A0D7-7F825A6C71DB";
            var verify = new Compare(action, 0, parse.Value);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = new User(parse.Value);
            if (!data.Result.Successful) return data.Result;

            data.Name = user.Name;
            data.Description = user.Description;
            if (!data.Update()) return data.Result;

            result.Success(data);
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

            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action, 0, parse.Value);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = new User(parse.Value);
            if (!data.Result.Successful) return data.Result;

            data.Authority = new Authority(parse.Value, null, InitType.Permission, true);
            result.Success(data);
            return result;
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">关键词</param>
        /// <returns>Result</returns>
        public Result GetUsers(string rows, string page, string key)
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

            using (var context = new BaseEntities())
            {
                var filter = !string.IsNullOrEmpty(key);
                var list = from u in context.SYS_User.Where(u => u.Type > 0 && (!filter || u.Name.Contains(key) || u.LoginName.Contains(key))).OrderBy(u => u.SN)
                    select new {u.ID, u.Name, u.LoginName, u.Mobile, u.Description, u.Type, u.BuiltIn, u.Validity, u.CreatorUserId, u.CreateTime};
                var skip = ipr.Value*(ipp.Value - 1);
                var users = new
                {
                    Total = list.Count(),
                    Items = list.Skip(skip).Take(ipr.Value).ToList()
                };
                result.Success(users);
                return result;
            }
        }

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result SignUp(string account, User user)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            if (user.Existed) return user.Result;

            user.CreateTime = DateTime.Now;
            if (!user.Add()) return user.Result;

            var token = new AccessToken {Account = account};
            var session = OAuth.Common.GetSession(token);
            session.InitSecret();

            verify.Result.Created(session.CreatorKey());
            return result;
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码</param>
        /// <returns>Result</returns>
        public Result UpdateSignature(string account, string password)
        {
            const string action = "26481E60-0917-49B4-BBAA-2265E71E7B3F";
            var verify = new Compare(action, account);
            var result = verify.Result;
            if (!result.Successful) return result;

            var session = Util.StringCompare(verify.Basis.Account, account)
                ? verify.Basis
                : OAuth.Common.GetSession(account);

            var user = new User(account) {Password = Util.Hash(account.ToUpper() + password)};
            if (!user.Result.Successful) return user.Result;

            if (!user.Update()) return user.Result;

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
        /// <returns>Result</returns>
        public Result ResetSignature(string account, string password, string code)
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

            var user = new User(account) {Password = Util.Hash(account.ToUpper() + password)};
            if (!user.Result.Successful) return user.Result;

            if (!user.Update()) return user.Result;

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
        /// <returns>Result</returns>
        public Result SetUserStatus(string account, bool validity)
        {
            var action = validity ? "369548E9-C8DB-439B-A604-4FDC07F3CCDD" : "0FA34D43-2C52-4968-BDDA-C9191D7FCE80";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var user = new User(account) {Validity = validity};
            if (!user.Result.Successful) return user.Result;

            if (!user.Update()) return user.Result;

            var session = OAuth.Common.GetSession(account);
            if (session != null) session.Validity = validity;

            return result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>Result</returns>
        public Result UserSignOut(string account)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var session = verify.Basis;
            session?.SignOut();

            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="deptid">登录部门ID</param>
        /// <returns>Result</returns>
        public Result GetUserRoles(string id, string deptid)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var dept = new GuidParse(deptid);
            if (!parse.Result.Successful) return parse.Result;

            var auth = new Authority(parse.Value, dept.Guid);
            result.Success(auth.RoleList);
            return result;
        }
    }
}
