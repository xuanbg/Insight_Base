using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.DTO;
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

            if (user == null) return result.BadRequest();

            if (Core.IsExisted(user)) return result.AccountExists();

            user.id = Util.NewId();
            user.password = Util.Hash("123456");
            user.creatorId = userId;
            user.createTime = DateTime.Now;
            if (!DbHelper.Insert(user)) return result.DataBaseError();

            var tu = new TenantUser
            {
                id = Util.NewId(),
                tenantId = tenantId,
                userId = user.id,
                creatorId = userId,
                createTime = DateTime.Now
            };

            return DbHelper.Insert(tu) ? result.Created(user) : result.DataBaseError();
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

            return DbHelper.Delete(user) ? result : result.DataBaseError();
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

            if (user == null) return result.BadRequest();

            var data = Core.GetUserById(user.id);
            if (data == null) return result.NotFound();

            data.name = user.name;
            data.account = user.account;
            data.mobile = user.mobile;
            data.email = user.email;
            data.remark = user.remark;
            if (!DbHelper.Update(data)) return result.DataBaseError();

            var session = Core.GetUserCache(user.id);
            if (session == null) return result;

            session.userName = user.name;
            session.account = user.account;
            session.mobile = user.mobile;
            session.email = user.email;
            session.SetChanged();
            Core.SetUserCache(session);

            return result;
        }

        /// <summary>
        /// 获取登录用户的用户对象实体
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetMyself()
        {
            if (!Verify()) return result;

            var data = Core.GetUserById(userId);
            data.password = null;
            data.payPassword = null;

            return data == null ? result.NotFound() : result.Success(data);
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result<object> GetUser(string id)
        {
            if (!Verify("getUsers", id)) return result;

            var data = Core.GetUserById(id);
            if (data == null) return result.NotFound();

            var user = new UserDto {funcs = Core.GetPermitAppTree(tenantId, id), datas = new List<AppTree>()};

            return result.Success(user);
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
                var list = from u in context.users
                    join r in context.tenantUsers on u.id equals r.userId
                    where r.tenantId == tenantId && (string.IsNullOrEmpty(key) || u.name.Contains(key) ||
                                                     u.account.Contains(key) || u.mobile.Contains(key) ||
                                                     u.email.Contains(key))
                    select new
                    {
                        u.id,
                        u.name,
                        u.account,
                        u.mobile,
                        u.email,
                        u.remark,
                        u.isBuiltin,
                        u.isInvalid,
                        u.creatorId,
                        u.createTime
                    };
                var skip = rows * (page - 1);
                var users = list.OrderBy(u => u.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(users, list.Count());
            }
        }

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="aid">应用ID</param>
        /// <param name="code">验证码</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result<object> SignUp(string aid, string code, User user)
        {
            if (!Verify()) return result;

            if (user == null) return result.BadRequest();

            if (!Core.VerifySmsCode(1, user.mobile, code)) return result.SMSCodeError();

            if (Core.IsExisted(user)) return result.AccountExists();

            user.id = Util.NewId();
            user.creatorId = user.id;
            user.createTime = DateTime.Now;
            if (!DbHelper.Insert(user)) return result.DataBaseError();

            var session = Core.GetUserCache(user.id);
            var tokens = session.Creator(Util.NewId("N"), aid);
            Core.SetUserCache(session);

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
            if (!Verify("resetPassword", id)) return result;

            var user = Core.GetUserById(id);
            if (user == null) return result.NotFound();

            if (user.password == password) result.BadRequest("密码不能与原密码相同");

            user.password = password;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            var session = Core.GetUserCache(id);
            if (session == null) return result;

            session.password = password;
            session.SetChanged();
            Core.SetUserCache(session);

            return result;
        }

        /// <summary>
        /// 用户重置登录密码
        /// </summary>
        /// <param name="aid">应用ID</param>
        /// <param name="account">登录账号</param>
        /// <param name="password">新密码（RSA加密）</param>
        /// <param name="code">短信验证码</param>
        /// <param name="mobile">手机号，默认为空。如为空，则使用account</param>
        /// <returns>Result</returns>
        public Result<object> ResetSignature(string aid, string account, string password, string code,
            string mobile = null)
        {
            if (!Core.VerifySmsCode(2, mobile ?? account, code)) return result.SMSCodeError();

            userId = Core.GetUserId(account);
            if (userId == null) return result.NotFound();

            manage = Core.GetUserCache(userId);
            if (manage.password != password)
            {
                var user = Core.GetUserById(userId);
                user.password = password;
                if (!DbHelper.Update(user)) return result.DataBaseError();
            }

            manage.password = password;
            var tokens = manage.Creator(Util.NewId("N"), aid);
            Core.SetUserCache(manage);

            return result.Created(tokens);
        }

        /// <summary>
        /// 获取用户绑定租户集合
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result<object> GetTenants(string account)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result<object> GetLoginDepts(string account)
        {
            using (var context = new Entities())
            {
                var user = context.users.SingleOrDefault(u => u.account == account || u.mobile == account || u.email == account);
                if (user == null) return result.NotFound();

                var list = new List<Organization>();
                var orgs = (from o in context.organizations
                    join r in context.tenantUsers on o.tenantId equals r.tenantId
                    where r.userId == user.id
                    select o).ToList();
                var ids = context.orgMembers.Where(m => m.userId == user.id).Select(i => i.orgId).ToList();
                list.AddRange(orgs.Where(i => i.id == i.tenantId));
                foreach (var id in ids)
                {
                    var org = orgs.Single(i => i.id == id);
                    while (org.parentId != null)
                    {
                        org = orgs.Single(i => i.id == org.parentId);
                        if (list.Any(i => i.id == org.id)) continue;

                        org.remark = org.tenantId;
                        list.Add(org);
                    }
                }

                return result.Success(list);
            }
        }

        /// <summary>
        /// 为指定的登录账号设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="invalid">失效状态</param>
        /// <returns>Result</returns>
        public Result<object> SetUserStatus(string id, bool invalid)
        {
            var action = invalid ? "bannedUser" : "releaseUser";
            if (!Verify(action)) return result;

            var user = Core.GetUserById(id);
            if (user == null) return result.NotFound();

            if (user.isInvalid == invalid) return result;

            user.isInvalid = invalid;
            if (!DbHelper.Update(user)) return result.DataBaseError();

            var session = Core.GetUserCache(user.id);
            if (session == null) return result;

            session.isInvalid = invalid;
            session.SetChanged();
            Core.SetUserCache(session);

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

            manage.Delete(tokenId);
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