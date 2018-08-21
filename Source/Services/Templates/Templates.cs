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
    public class Templates : ServiceBase, ITemplates
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void responseOptions()
        {
        }

        /// <summary>
        /// 获取指定业务模块的分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> getCategorys(string id)
        {
            if (!verify("getTemplets")) return result;

            var list = CatalogHelper.getCatalogs(tenantId, id);

            return result.success(list);
        }

        /// <summary>
        /// 新建分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> addCategory(Catalog catalog)
        {
            if (!verify("newTempletCatalog")) return result;

            catalog.tenantId = tenantId;
            catalog.creatorDeptId = deptId;
            catalog.creator = userName;
            catalog.creatorId = userId;
            if (CatalogHelper.existed(catalog)) return result.dataAlreadyExists();

            return CatalogHelper.add(catalog) ? result.success(catalog) : result.dataBaseError();
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> editCategory(string id, Catalog catalog)
        {
            return verify("editTempletCatalog") ? CatalogHelper.edit(id, catalog) : result;
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteCategory(string id)
        {
            return verify("deleteTempletCatalog") ? CatalogHelper.delete(id) : result;
        }

        /// <summary>
        /// 获取所有报表模板
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> getTemplates(string id, int rows, int page)
        {
            if (!verify("getTemplets")) return result;

            if (page < 1 || rows > 100) return result.badRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.templates.Where(i => i.tenantId == tenantId && i.categoryId == id)
                    .Select(i => new {i.id, i.tenantId, i.categoryId, i.name, i.remark, i.creator, i.createTime});
                var templates = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.success(templates, list.Count());
            }
        }

        /// <summary>
        /// 获取所有报表模板
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> getAllTemplates()
        {
            if (!verify()) return result;

            using (var context = new Entities())
            {
                const string moduleId = "ad0bd296-46f5-46b3-85b9-00b6941343e7";
                var cats = from c in context.categories
                    where !c.isInvalid && c.tenantId == tenantId && c.moduleId == moduleId
                    select new {c.id, c.parentId, c.index, c.name};
                var temps = from t in context.templates
                    join c in cats on t.categoryId equals c.id
                    select new {t.id, parentId = c.id, index = 0, t.name};
                var list = cats.Union(temps).OrderBy(i => new {i.index, i.name}).ToList();

                return result.success(list);
            }
        }

        /// <summary>
        /// 获取报表模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>Result</returns>
        public Result<object> getTemplate(string id)
        {
            if (!verify("getTemplets")) return result;

            var data = DbHelper.find<ReportTemplet>(id);

            return data == null ? result.notFound() : result.success(data);
        }

        /// <summary>
        /// 导入报表模板
        /// </summary>
        /// <param name="template">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> importTemplate(ReportTemplet template)
        {
            if (!verify("importTemplet")) return result;

            template.id = Util.newId();
            template.tenantId = tenantId;
            template.isBuiltin = false;
            template.creatorDeptId = deptId;
            template.creator = userName;
            template.creatorId = userId;
            template.createTime = DateTime.Now;
            if (existed(template)) return result.dataAlreadyExists();

            return DbHelper.insert(template) ? result.success(template) : result.dataBaseError();
        }

        /// <summary>
        /// 复制报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="templet">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> copyTemplate(string id, ReportTemplet templet)
        {
            if (!verify("copyTemplet")) return result;

            var data = DbHelper.find<ReportTemplet>(id);
            if (data == null) return result.notFound();

            templet.id = Util.newId();
            templet.tenantId = tenantId;
            templet.content = data.content;
            templet.isBuiltin = false;
            templet.creatorDeptId = deptId;
            templet.creator = userName;
            templet.creatorId = userId;
            templet.createTime = DateTime.Now;
            if (existed(templet)) return result.dataAlreadyExists();

            return DbHelper.insert(templet) ? result.success(templet) : result.dataBaseError();
        }

        /// <summary>
        /// 编辑报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="templet">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> editTemplate(string id, ReportTemplet templet)
        {
            if (!verify("editTemplet")) return result;

            var data = DbHelper.find<ReportTemplet>(id);
            if (data == null) return result.notFound();

            data.categoryId = templet.categoryId;
            data.name = templet.name;
            data.remark = templet.remark;
            if (existed(data)) return result.dataAlreadyExists();

            return DbHelper.update(data) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 设计报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="content">模板内容</param>
        /// <returns>Result</returns>
        public Result<object> designTemplet(string id, string content)
        {
            if (!verify("designTemplet")) return result;

            var data = DbHelper.find<ReportTemplet>(id);
            if (data == null) return result.notFound();

            data.content = content;

            return DbHelper.update(data) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 删除报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteTemplate(string id)
        {
            if (!verify("deleteTemplet")) return result;

            var data = DbHelper.find<ReportTemplet>(id);
            if (data == null) return result.notFound();

            return DbHelper.delete(data) ? result : result.dataBaseError();
        }

        /// <summary>
        /// 模板是否存在
        /// </summary>
        /// <param name="templet"></param>
        /// <returns>bool 是否存在</returns>
        private static bool existed(ReportTemplet templet)
        {
            using (var context = new Entities())
            {
                return context.templates.Any(i => i.id != templet.id && i.tenantId == templet.tenantId &&
                                                  i.categoryId == templet.categoryId && i.name == templet.name);
            }
        }
    }
}