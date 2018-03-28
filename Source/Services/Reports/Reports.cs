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
    public class Reports : ServiceBase, IReports
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取报表分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetCategorys(string id)
        {
            if (!Verify("getReports")) return result;

            var list = CatalogHelper.GetCatalogs(tenantId, id);

            return result.Success(list);
        }

        /// <summary>
        /// 新建报表分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> AddCategory(Catalog catalog)
        {
            if (!Verify("newReportCatalog")) return result;

            catalog.tenantId = tenantId;
            if (CatalogHelper.Existed(catalog)) return result.DataAlreadyExists();

            return CatalogHelper.Add(catalog) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 编辑报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> EditCategory(string id, Catalog catalog)
        {
            if (!Verify("editReportCatalog")) return result;

            var data = DbHelper.Find<Catalog>(id);
            if (data == null) return result.NotFound();

            return CatalogHelper.Edit(data, catalog) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 删除报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteCategory(string id)
        {
            if (!Verify("deleteReportCatalog")) return result;

            return CatalogHelper.Delete(id) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 获取分类下全部报表
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">页码</param>
        /// <returns>Result</returns>
        public Result<object> GetReports(string id, int rows, int page)
        {
            if (!Verify("getReports")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.definitions.Where(i => i.tenantId == tenantId && i.categoryId == id);
                var definitions = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(definitions, list.Count());
            }
        }

        /// <summary>
        /// 获取报表
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Result</returns>
        public Result<object> GetReport(string id)
        {
            if (!Verify("getReports")) return result;


            return null;
        }

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> AddReport(Definition report)
        {
            if (!Verify("newReport")) return result;

            return null;
        }

        /// <summary>
        /// 编辑报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> EditReport(string id, Definition report)
        {
            if (!Verify("editReport")) return result;

            return null;
        }

        /// <summary>
        /// 删除报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteReport(string id)
        {
            if (!Verify("deleteReport")) return result;

            return null;
        }
    }
}