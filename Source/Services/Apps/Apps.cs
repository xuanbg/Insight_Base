using System;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class Apps : ServiceBase, IApps
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据关键词查询全部应用集合
        /// </summary>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetAllApps(string key)
        {
            if (!Verify("getAllApps")) return result;

            using (var context = new Entities())
            {
                var list = context.applications.Where(i => string.IsNullOrEmpty(key) || i.name.Contains(key))
                    .OrderBy(i => i.createTime).ToList();

                return result.Success(list);
            }
        }

        /// <summary>
        /// 根据关键词查询租户绑定应用集合
        /// </summary>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetApps(string key)
        {
            if (!Verify("getApps")) return result;

            using (var context = new Entities())
            {
                var list = from a in context.applications
                    join r in context.tenantApps on a.id equals r.appId
                    where r.tenantId == tenantId && (string.IsNullOrEmpty(key) || a.name.Contains(key))
                    orderby a.createTime
                    select a;

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 获取指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> GetApp(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 新增应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddApp(Application app)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditApp(Application app)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 删除指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteApp(string id)
        {
            throw new NotImplementedException();
        }
    }
}
