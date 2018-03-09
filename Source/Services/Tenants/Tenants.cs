using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.DTO;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class Tenants : ServiceBase, ITenants
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据关键词查询全部租户集合
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetTenants(int rows, int page, string key)
        {
            if (!Verify("getTenants")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var list = from t in context.tenants
                    where !t.isInvalid && !t.isBuiltin && (string.IsNullOrEmpty(key) || t.name.Contains(key) || t.alias.Contains(key) || t.contact.Contains(key))
                    select new
                    {
                        t.id,
                        t.name,
                        t.alias,
                        t.contact,
                        t.mobile,
                        t.email,
                        t.province,
                        t.city,
                        t.county,
                        t.address,
                        t.remark,
                        t.expireDate,
                        t.createTime
                    };
                var skip = rows * (page - 1);
                var tenants = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(tenants, list.Count());
            }
        }

        /// <summary>
        /// 获取指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        public Result<object> GetTenant(string id)
        {
            if (!Verify("getTenants")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            var tenant = new TenantInfo
            {
                apps = GetApps(id),
                users = GetUsers(id),
            };

            return result.Success(tenant);
        }

        /// <summary>
        /// 新增租户信息
        /// </summary>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddTenant(Tenant tenant)
        {
            if (!Verify("newTenant")) return result;

            if (tenant == null) return result.BadRequest();

            if (Existed(tenant)) return result.DataAlreadyExists();

            tenant.id = Util.NewId();
            tenant.expireDate = DateTime.Now.AddDays(90);
            tenant.isBuiltin = false;
            tenant.isInvalid = false;
            tenant.creatorId = userId;
            tenant.createTime = DateTime.Now;

            return DbHelper.Insert(tenant) ? result.Created(tenant) : result.DataBaseError();
        }

        /// <summary>
        /// 修改租户信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditTenant(string id, Tenant tenant)
        {
            if (!Verify("editTenant")) return result;

            if (tenant == null) return result.BadRequest();

            var data = GetData(tenant.id);
            if (data == null) return result.NotFound();

            data.name = tenant.name;
            data.alias = tenant.alias;
            data.icon = tenant.icon;
            data.contact = tenant.contact;
            data.mobile = tenant.mobile;
            data.email = tenant.email;
            data.province = tenant.province;
            data.city = tenant.city;
            data.county = tenant.county;
            data.address = tenant.address;
            data.remark = tenant.remark;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 延长有效天数
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <param name="expire">续租天数</param>
        /// <returns>Result</returns>
        public Result<object> ExtendTenant(string id, int expire)
        {
            if (!Verify("extend")) return result;

            if (expire < 30) return result.BadRequest("续租时间不能少于30天");

            var data = GetData(id);
            if (data == null) return result.NotFound();

            data.expireDate = data.expireDate.AddDays(expire);

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 删除指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteTenant(string id)
        {
            if (!Verify("deleteTenant")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            data.isInvalid = true;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 为租户绑定应用
        /// </summary>
        /// <param name="tenant">租户-应用关系实体数据</param>
        /// <returns>Result</returns>
        public Result<object> BindApp(TenantApp tenant)
        {
            if (!Verify("bindApp")) return result;

            var data = GetData(tenant.tenantId);
            var app = DbHelper.Find<Application>(tenant.appId);
            if (data == null || app == null) return result.NotFound();

            tenant.id = Util.NewId();
            tenant.creatorId = userId;
            tenant.createTime = DateTime.Now;

            return DbHelper.Insert(tenant) ? result.Success(app) : result.DataBaseError();
        }

        /// <summary>
        /// 为租户关联用户
        /// </summary>
        /// <param name="tenant">租户-用户关系实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddTenantUser(TenantUser tenant)
        {
            if (!Verify()) return result;

            var data = GetData(tenant.tenantId);
            var user = DbHelper.Find<Application>(tenant.userId);
            if (data == null || user == null) return result.NotFound();

            tenant.id = Util.NewId();
            tenant.creatorId = userId;
            tenant.createTime = DateTime.Now;

            return DbHelper.Insert(tenant) ? result.Success(user) : result.DataBaseError();
        }

        /// <summary>
        /// 删除指定ID的租户和用户的绑定关系
        /// </summary>
        /// <param name="id">租户-用户关系ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteTenantUser(string id)
        {
            if (!Verify()) return result;

            var data = DbHelper.Find<TenantUser>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 租户是否已存在
        /// </summary>
        /// <param name="tenant">租户</param>
        /// <returns>是否已存在</returns>
        private static bool Existed(Tenant tenant)
        {
            using (var context = new Entities())
            {
                return context.tenants.Any(i => i.id != tenant.id && i.name == tenant.name && i.alias == tenant.alias);
            }
        }

        /// <summary>
        /// 获取指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>租户</returns>
        private static Tenant GetData(string id)
        {
            using (var context = new Entities())
            {
                return context.tenants.SingleOrDefault(i => i.id == id);
            }
        }

        /// <summary>
        /// 获取绑定应用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static List<Application> GetApps(string id)
        {
            using (var context = new Entities())
            {
                var list = from a in context.applications
                    join r in context.tenantApps on a.id equals r.appId
                    where r.tenantId == id
                    select a;
                return list.ToList();
            }
        }

        /// <summary>
        /// 获取绑定应用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static List<User> GetUsers(string id)
        {
            using (var context = new Entities())
            {
                var list = from u in context.users
                    join r in context.tenantUsers on u.id equals r.userId
                    where r.tenantId == id && !u.isInvalid
                    select u;
                return list.ToList();
            }
        }
    }
}
