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
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取指定业务模块的分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetCategorys(string id)
        {
            if (!Verify("getTemplets")) return result;

            var list = CatalogHelper.GetCatalogs(tenantId, id);

            return result.Success(list);
        }

        /// <summary>
        /// 新建分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> AddCategory(Catalog catalog)
        {
            if (!Verify("newTempletCatalog")) return result;

            catalog.tenantId = tenantId;
            catalog.creatorDeptId = deptId;
            catalog.creator = userName;
            catalog.creatorId = userId;
            if (CatalogHelper.Existed(catalog)) return result.DataAlreadyExists();

            return CatalogHelper.Add(catalog) ? result.Success(catalog) : result.DataBaseError();
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        public Result<object> EditCategory(string id, Catalog catalog)
        {
            return Verify("editTempletCatalog") ? CatalogHelper.Edit(id, catalog) : result;
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteCategory(string id)
        {
            return Verify("deleteTempletCatalog") ? CatalogHelper.Delete(id) : result;
        }

        /// <summary>
        /// 获取所有报表模板
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> GetTemplates(string id, int rows, int page)
        {
            if (!Verify("getTemplets")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            var skip = rows * (page - 1);
            using (var context = new Entities())
            {
                var list = context.templates.Where(i => i.tenantId == tenantId && i.categoryId == id)
                    .Select(i => new {i.id, i.tenantId, i.categoryId, i.name, i.remark, i.creator, i.createTime});
                var templates = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(templates, list.Count());
            }
        }

        /// <summary>
        /// 获取所有报表模板
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetAllTemplates()
        {
            if (!Verify()) return result;

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

                return result.Success(list);
            }
        }

        /// <summary>
        /// 获取报表模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>Result</returns>
        public Result<object> GetTemplate(string id)
        {
            if (!Verify("getTemplets")) return result;

            var data = DbHelper.Find<ReportTemplet>(id);

            return data == null ? result.NotFound() : result.Success(data);
        }

        /// <summary>
        /// 导入报表模板
        /// </summary>
        /// <param name="template">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> ImportTemplate(ReportTemplet template)
        {
            if (!Verify("importTemplet")) return result;

            template.id = Util.NewId();
            template.tenantId = tenantId;
            template.isBuiltin = false;
            template.creatorDeptId = deptId;
            template.creator = userName;
            template.creatorId = userId;
            template.createTime = DateTime.Now;
            if (Existed(template)) return result.DataAlreadyExists();

            return DbHelper.Insert(template) ? result.Success(template) : result.DataBaseError();
        }

        /// <summary>
        /// 复制报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="templet">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> CopyTemplate(string id, ReportTemplet templet)
        {
            if (!Verify("copyTemplet")) return result;

            var data = DbHelper.Find<ReportTemplet>(id);
            if (data == null) return result.NotFound();

            templet.id = Util.NewId();
            templet.tenantId = tenantId;
            templet.content = data.content;
            templet.isBuiltin = false;
            templet.creatorDeptId = deptId;
            templet.creator = userName;
            templet.creatorId = userId;
            templet.createTime = DateTime.Now;
            if (Existed(templet)) return result.DataAlreadyExists();

            return DbHelper.Insert(templet) ? result.Success(templet) : result.DataBaseError();
        }

        /// <summary>
        /// 编辑报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="templet">报表模板</param>
        /// <returns>Result</returns>
        public Result<object> EditTemplate(string id, ReportTemplet templet)
        {
            if (!Verify("editTemplet")) return result;

            var data = DbHelper.Find<ReportTemplet>(id);
            if (data == null) return result.NotFound();

            data.categoryId = templet.categoryId;
            data.name = templet.name;
            data.remark = templet.remark;
            if (Existed(data)) return result.DataAlreadyExists();

            return DbHelper.Update(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 设计报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="content">模板内容</param>
        /// <returns>Result</returns>
        public Result<object> DesignTemplet(string id, string content)
        {
            if (!Verify("designTemplet")) return result;

            var data = DbHelper.Find<ReportTemplet>(id);
            if (data == null) return result.NotFound();

            data.content = content;

            return DbHelper.Update(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 删除报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteTemplate(string id)
        {
            if (!Verify("deleteTemplet")) return result;

            var data = DbHelper.Find<ReportTemplet>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 模板是否存在
        /// </summary>
        /// <param name="templet"></param>
        /// <returns>bool 是否存在</returns>
        private static bool Existed(ReportTemplet templet)
        {
            using (var context = new Entities())
            {
                return context.templates.Any(i => i.id != templet.id && i.tenantId == templet.tenantId &&
                                                  i.categoryId == templet.categoryId && i.name == templet.name);
            }
        }
    }
}