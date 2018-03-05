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
        public Result<object> GetAllTenants(int rows, int page, string key)
        {
            if (!Verify("getTenants")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var list = from t in context.tenants
                    where !t.isInvalid && !t.isBuiltin || (string.IsNullOrEmpty(key) || t.name.Contains(key) || t.alias.Contains(key) || t.contact.Contains(key))
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

            if (Existed(tenant)) return result.DataAlreadyExists();

            return DbHelper.Insert(tenant) ? result.Created(tenant) : result.DataBaseError();
        }

        /// <summary>
        /// 修改租户信息
        /// </summary>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditTenant(Tenant tenant)
        {
            throw new NotImplementedException();
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
