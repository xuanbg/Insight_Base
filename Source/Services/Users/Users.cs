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
        public void responseOptions()
        {
        }

        /// <summary>
        /// 根据对象实体数据新增一个用户
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result<object> addUser(User user)
        {
            if (!verify("newUser")) return result;

            if (user == null) return result.badRequest();

            if (Core.isExisted(user)) return result.accountExists();

            user.id = Util.newId();
            user.password = Util.hash("123456");
            user.creatorId = userId;
            user.createTime = DateTime.Now;
            if (!DbHelper.insert(user)) return result.dataBaseError();

            var tu = new TenantUser
            {
                id = Util.newId(),
                tenantId = tenantId,
                userId = user.id,
                creatorId = userId,
                createTime = DateTime.Now
            };

            return DbHelper.insert(tu) ? result.created(user) : result.dataBaseError();
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result<object> removeUser(string id)
        {
            if (!verify("deleteUser")) return result;

            var user = Core.getUserById(id);
            if (user == null) return result.notFound();

            return DbHelper.delete(user) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user">用户数据对象</param>
        /// <returns>Result</returns>
        public Result<object> updateUserInfo(string id, User user)
        {
            if (!verify("editUser")) return result;

            if (user == null) return result.badRequest();

            var data = Core.getUserById(user.id);
            if (data == null) return result.notFound();

            data.name = user.name;
            data.account = user.account;
            data.mobile = user.mobile;
            data.email = user.email;
            data.remark = user.remark;
            if (!DbHelper.update(data)) return result.dataBaseError();

            var session = Core.getUserCache(user.id);
            if (session == null) return result;

            session.userName = user.name;
            session.account = user.account;
            session.mobile = user.mobile;
            session.email = user.email;
            session.setChanged();
            Core.setUserCache(session);

            return result;
        }

        /// <summary>
        /// 获取登录用户的用户对象实体
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> getMyself()
        {
            if (!verify()) return result;

            var data = Core.getUserById(userId);
            if (data == null) return result.notFound();

            data.password = null;
            data.payPassword = null;

            return result.success(data);
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>Result</returns>
        public Result<object> getUser(string id)
        {
            if (!verify("getUsers", id)) return result;

            var data = Core.getUserById(id);
            if (data == null) return result.notFound();

            var user = new UserDto {funcs = Core.getPermitAppTree(tenantId, id), datas = new List<AppTree>()};

            return result.success(user);
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">关键词</param>
        /// <returns>Result</returns>
        public Result<object> getUsers(int rows, int page, string key)
        {
            if (!verify("getUsers")) return result;

            if (page < 1 || rows > 100) return result.badRequest();

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

                return result.success(users, list.Count());
            }
        }

        /// <summary>
        /// 根据对象实体数据注册一个用户
        /// </summary>
        /// <param name="aid">应用ID</param>
        /// <param name="code">验证码</param>
        /// <param name="user">用户对象</param>
        /// <returns>Result</returns>
        public Result<object> signUp(string aid, string code, User user)
        {
            if (user == null) return result.badRequest();

            var fingerprint = getFingerprint();
            var limitKey = Util.hash("signUp" + fingerprint + Util.serialize(code));
            var surplus = Params.callManage.getSurplus(limitKey, 60);
            if (surplus > 0) return result.tooFrequent(surplus);

            if (!Core.verifySmsCode(1, user.mobile, code)) return result.smsCodeError();

            if (Core.isExisted(user)) return result.accountExists();

            if (string.IsNullOrEmpty(user.id)) user.id = Util.newId();

            if (string.IsNullOrEmpty(user.password)) user.password = Util.hash("123456");

            user.creatorId = user.id;
            user.createTime = DateTime.Now;
            if (!DbHelper.insert(user)) return result.dataBaseError();

            var id = Core.getUserId(user.account);
            manage = Core.getUserCache(id);
            var tokens = manage.creator(Util.newId("N"), aid);
            Core.setUserCache(manage);

            return result.created(tokens);
        }

        /// <summary>
        /// 更新指定用户Session的签名
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="password">新密码（RSA加密）</param>
        /// <returns>Result</returns>
        public Result<object> updateSignature(string id, string password)
        {
            if (!verify("resetPassword", id)) return result;

            var user = Core.getUserById(id);
            if (user == null) return result.notFound();

            if (user.password == password) result.badRequest("密码不能与原密码相同");

            user.password = password;
            if (!DbHelper.update(user)) return result.dataBaseError();

            manage = Core.getUserCache(id);
            if (manage == null) return result;

            manage.password = password;
            manage.setChanged();
            Core.setUserCache(manage);

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
        public Result<object> resetSignature(string aid, string account, string password, string code, string mobile)
        {
            if (string.IsNullOrEmpty(aid) || string.IsNullOrEmpty(password)) return result.invalidValue();

            var fingerprint = getFingerprint();
            var limitKey = Util.hash("resetSignature" + fingerprint + Util.serialize(code));
            var surplus = Params.callManage.getSurplus(limitKey, 10);
            if (surplus > 0) return result.tooFrequent(surplus);

            if (!Core.verifySmsCode(2, mobile ?? account, code)) return result.smsCodeError();

            userId = Core.getUserId(account);
            if (userId == null) return result.notFound();

            manage = Core.getUserCache(userId);
            if (manage.password != password)
            {
                var user = Core.getUserById(userId);
                user.password = password;
                if (!DbHelper.update(user)) return result.dataBaseError();
            }

            manage.password = password;
            manage.setChanged();
            var tokens = manage.creator(Util.newId("N"), aid);
            Core.setUserCache(manage);

            return result.created(tokens);
        }

        /// <summary>
        /// 获取用户绑定租户集合
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result<object> getTenants(string account)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result<object> getLoginDepts(string account)
        {
            using (var context = new Entities())
            {
                var user = context.users.SingleOrDefault(u =>
                    u.account == account || u.mobile == account || u.email == account);
                if (user == null) return result.notFound();

                var list = new List<Org>();
                var orgs = (from o in context.orgs
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

                return result.success(list);
            }
        }

        /// <summary>
        /// 为指定的登录账号设置用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="invalid">失效状态</param>
        /// <returns>Result</returns>
        public Result<object> setUserStatus(string id, bool invalid)
        {
            var action = invalid ? "bannedUser" : "releaseUser";
            if (!verify(action)) return result;

            var user = Core.getUserById(id);
            if (user == null) return result.notFound();

            if (user.isInvalid == invalid) return result;

            user.isInvalid = invalid;
            if (!DbHelper.update(user)) return result.dataBaseError();

            var session = Core.getUserCache(user.id);
            if (session == null) return result;

            session.isInvalid = invalid;
            session.setChanged();
            Core.setUserCache(session);

            return result;
        }

        /// <summary>
        /// 设置指定用户的登录状态为离线
        /// </summary>
        /// <param name="id">用户账号</param>
        /// <returns>Result</returns>
        public Result<object> userSignOut(string id)
        {
            if (!verify()) return result;

            TokenManage.delete(tokenId);
            return result;
        }

        /// <summary>
        /// 根据ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="deptid">登录部门ID</param>
        /// <returns>Result</returns>
        public Result<object> getUserRoles(string id, string deptid)
        {
            if (!verify()) return result;

            using (var context = new Entities())
            {
                var list = context.userRoles.ToList();
                return result.success(list);
            }
        }
    }
}