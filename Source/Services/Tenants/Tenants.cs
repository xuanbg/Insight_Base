using System;
using System.Linq;
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
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetAllTenants(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据关键词查询租户绑定租户集合
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetTenants()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取指定ID的租户
        /// </summary>
        /// <param name="id">租户ID</param>
        /// <returns>Result</returns>
        public Result<object> GetTenant(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 新增租户信息
        /// </summary>
        /// <param name="tenant">租户实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddTenant(Tenant tenant)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
