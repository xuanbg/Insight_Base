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
        public void responseOptions()
        {
        }

        /// <summary>
        /// 获取报表分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> getCategorys(string id)
        {
            if (!verify("getReports")) return result;

            var list = CatalogHelper.getCatalogs(tenantId, id);

            return result.success(list);
        }

        /// <summary>
        /// 新建报表分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> addCategory(Catalog catalog)
        {
            if (!verify("newReportCatalog")) return result;

            catalog.tenantId = tenantId;
            catalog.creatorDeptId = deptId;
            catalog.creator = userName;
            catalog.creatorId = userId;
            if (CatalogHelper.existed(catalog)) return result.dataAlreadyExists();

            return CatalogHelper.add(catalog) ? result.success(catalog) : result.dataBaseError();
        }

        /// <summary>
        /// 编辑报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> editCategory(string id, Catalog catalog)
        {
            return verify("editReportCatalog") ? CatalogHelper.edit(id, catalog) : result;
        }

        /// <summary>
        /// 删除报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteCategory(string id)
        {
            return verify("deleteReportCatalog") ? CatalogHelper.delete(id) : result;
        }

        /// <summary>
        /// 获取分类下全部报表
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">页码</param>
        /// <returns>Result</returns>
        public Result<object> getReports(string id, int rows, int page)
        {
            if (!verify("getReports")) return result;

            if (page < 1 || rows > 100) return result.badRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.definitions.Where(i => i.tenantId == tenantId && i.categoryId == id);
                var definitions = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.success(definitions, list.Count());
            }
        }

        /// <summary>
        /// 获取报表
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Result</returns>
        public Result<object> getReport(string id)
        {
            if (!verify("getReports")) return result;

            var data = DbHelper.find<ReportDefinition>(id);
            if (data == null) return result.notFound();

            var report = Util.convertTo<Definition>(data);
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

            return result.success(report);
        }

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> addReport(Definition report)
        {
            if (!verify("newReport")) return result;

            var data = Util.convertTo<ReportDefinition>(report);
            data.id = Util.newId();
            data.tenantId = tenantId;
            data.creatorDeptId = deptId;
            data.creator = userName;
            data.creatorId = userId;
            data.createTime = DateTime.Now;
            if (existed(data)) return result.dataAlreadyExists();

            data.periods = report.periods.Select(i => new ReportPeriod
                {
                    id = Util.newId(),
                    reportId = data.id,
                    ruleId = i.ruleId
                }).ToList();
            data.entities = report.entities.Select(i => new ReportEntity
                {
                    id = Util.newId(),
                    reportId = data.id,
                    orgId = i.orgId,
                    name = i.name
                }).ToList();
            data.entities.ForEach(i =>
            {
                i.members = report.members.Where(m => m.entityId == i.id).Select(r => new EntityMember
                    {
                        id = Util.newId(),
                        entityId = i.id,
                        roleId = r.roleId,
                        name = r.name
                    }).ToList();
            });
            if (!DbHelper.insert(data)) return result.dataBaseError();

            report.id = data.id;
            report.creator = data.creator;
            report.createTime = data.createTime;

            return result.created(report);
        }

        /// <summary>
        /// 编辑报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        public Result<object> editReport(string id, ReportDefinition report)
        {
            if (!verify("editReport")) return result;

            var data = DbHelper.find<ReportDefinition>(id);
            if (data == null) return result.notFound();

            data.categoryId = report.categoryId;
            data.name = report.name;
            data.mode = report.mode;
            data.delay = report.delay;
            data.reportType = report.reportType;
            data.dataSource = report.dataSource;
            data.remark = report.remark;
            if (existed(data)) return result.dataAlreadyExists();

            return DbHelper.update(data) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 删除报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteReport(string id)
        {
            if (!verify("deleteReport")) return result;

            var data = DbHelper.find<ReportDefinition>(id);
            if (data == null) return result.notFound();

            return DbHelper.delete(data) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 定义是否存在
        /// </summary>
        /// <param name="definition"></param>
        /// <returns>bool 是否存在</returns>
        public static bool existed(ReportDefinition definition)
        {
            using (var context = new Entities())
            {
                return context.definitions.Any(i => i.id != definition.id && i.tenantId == definition.tenantId 
                && i.categoryId == definition.categoryId && i.name == definition.name);
            }
        }
    }
}