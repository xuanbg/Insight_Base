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
            catalog.creatorDeptId = deptId;
            catalog.creator = userName;
            catalog.creatorId = userId;
            if (CatalogHelper.Existed(catalog)) return result.DataAlreadyExists();

            return CatalogHelper.Add(catalog) ? result.Success(catalog) : result.DataBaseError();
        }

        /// <summary>
        /// 编辑报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> EditCategory(string id, Catalog catalog)
        {
            return Verify("editReportCatalog") ? CatalogHelper.Edit(id, catalog) : result;
        }

        /// <summary>
        /// 删除报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteCategory(string id)
        {
            return Verify("deleteReportCatalog") ? CatalogHelper.Delete(id) : result;
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

            var data = DbHelper.Find<ReportDefinition>(id);
            if (data == null) return result.NotFound();

            var report = Util.ConvertTo<Definition>(data);
            using (var context = new Entities())
            {
                var periods = from p in context.periods
                    join r in context.rules on p.ruleId equals r.id
                    where p.reportId == id
                    select new Period {id = p.id, name = r.name};
                var entities = from e in context.reportEntities
                    join o in context.orgs on e.orgId equals o.id
                    where e.reportId == id
                    select new Entity {id = e.id, name = o.fullname};

                report.periods = periods.ToList();
                report.entities = entities.ToList();
            }

            return result.Success(report);
        }

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> AddReport(Definition report)
        {
            if (!Verify("newReport")) return result;

            var data = Util.ConvertTo<ReportDefinition>(report);
            data.id = Util.NewId();
            data.tenantId = tenantId;
            data.creatorDeptId = deptId;
            data.creator = userName;
            data.creatorId = userId;
            data.createTime = DateTime.Now;
            if (Existed(data)) return result.DataAlreadyExists();

            data.periods = report.periods.Select(i => new ReportPeriod
                {
                    id = Util.NewId(),
                    reportId = data.id,
                    ruleId = i.ruleId
                }).ToList();
            data.entities = report.entities.Select(i => new ReportEntity
                {
                    id = Util.NewId(),
                    reportId = data.id,
                    orgId = i.orgId,
                    name = i.name
                }).ToList();
            data.entities.ForEach(i =>
            {
                i.members = report.members.Where(m => m.entityId == i.id).Select(r => new EntityMember
                    {
                        id = Util.NewId(),
                        entityId = i.id,
                        roleId = r.roleId,
                        name = r.name
                    }).ToList();
            });
            if (!DbHelper.Insert(data)) return result.DataBaseError();

            report.id = data.id;
            report.creator = data.creator;
            report.createTime = data.createTime;

            return result.Created(report);
        }

        /// <summary>
        /// 编辑报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> EditReport(string id, ReportDefinition report)
        {
            if (!Verify("editReport")) return result;

            var data = DbHelper.Find<ReportDefinition>(id);
            if (data == null) return result.NotFound();

            data.categoryId = report.categoryId;
            data.name = report.name;
            data.mode = report.mode;
            data.delay = report.delay;
            data.reportType = report.reportType;
            data.dataSource = report.dataSource;
            data.remark = report.remark;
            if (Existed(data)) return result.DataAlreadyExists();

            return DbHelper.Update(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 删除报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteReport(string id)
        {
            if (!Verify("deleteReport")) return result;

            var data = DbHelper.Find<ReportDefinition>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 定义是否存在
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>bool 是否存在</returns>
        public static bool Existed(ReportDefinition definition)
        {
            using (var context = new Entities())
            {
                return context.definitions.Any(i => i.id != definition.id && i.tenantId == definition.tenantId 
                && i.categoryId == definition.categoryId && i.name == definition.name);
            }
        }
    }
}