using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Common
{
    public class CatalogHelper
    {
        /// <summary>
        /// 获取分类集合
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="moduleId">业务模块ID</param>
        /// <returns>分类集合</returns>
        public static List<Catalog> getCatalogs(string tenantId, string moduleId)
        {
            using (var context = new Entities())
            {
                var list = context.categories.Where(i =>
                    !i.isInvalid && i.tenantId == tenantId && i.moduleId == moduleId);

                return list.OrderBy(i => i.index).ToList();
            }
        }

        /// <summary>
        /// 获取分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>Catalog 分类对象实体</returns>
        public static Catalog getCatalog(string id)
        {
            return DbHelper.find<Catalog>(id);
        }

        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="catalog">分类对象实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool add(Catalog catalog)
        {
            using (var context = new Entities())
            {
                var list = context.categories.Where(i => i.parentId == catalog.parentId && i.index >= catalog.index);
                foreach (var item in list)
                {
                    item.index++;
                }

                context.SaveChanges();
            }

            catalog.id = Util.newId();
            catalog.isBuiltin = false;
            catalog.isInvalid = false;
            catalog.createTime = DateTime.Now;

            return DbHelper.insert(catalog);
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="catalog">分类对象实体</param>
        /// <returns>bool 是否成功</returns>
        public static Result<object> edit(string id, Catalog catalog)
        {
            var result = new Result<object>();
            var data = DbHelper.find<Catalog>(id);
            if (data == null) return result.notFound();

            updateIndex(data, catalog);

            data.parentId = catalog.parentId;
            data.index = catalog.index;
            data.code = catalog.code;
            data.name = catalog.name;
            data.alias = catalog.alias;
            data.remark = catalog.remark;
            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>bool 是否成功</returns>
        public static Result<object> delete(string id)
        {
            var result = new Result<object>();
            var data = DbHelper.find<Catalog>(id);
            if (data == null) return result.notFound();

            using (var context = new Entities())
            {
                if (context.categories.Any(i => i.parentId == id))
                {
                    return result.badRequest("请先删除下级分类");
                }

                var list = context.categories.Where(i => i.parentId == data.parentId && i.index > data.index);
                foreach (var item in list)
                {
                    item.index--;
                }

                context.SaveChanges();
            }

            return DbHelper.delete(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 分类是否存在
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>bool 是否存在</returns>
        public static bool existed(Catalog catalog)
        {
            using (var context = new Entities())
            {
                return context.categories.Any(i => i.id != catalog.id && i.tenantId == catalog.tenantId &&
                                                   i.moduleId == catalog.moduleId && i.parentId == catalog.parentId &&
                                                   (!string.IsNullOrEmpty(catalog.code) && i.code == catalog.code ||
                                                    i.name == catalog.name));
            }
        }

        /// <summary>
        /// 自动调整相关分类下其他分类的索引值
        /// </summary>
        /// <param name="old"></param>
        /// <param name="cat"></param>
        private static void updateIndex(Catalog old, Catalog cat)
        {
            using (var context = new Entities())
            {
                if (cat.parentId == old.parentId)
                {
                    if (cat.index < old.index)
                    {
                        var list = context.categories.Where(i => i.index >= cat.index && i.index < old.index);
                        foreach (var item in list)
                        {
                            item.index++;
                        }
                    }
                    else if (cat.index > old.index)
                    {
                        var list = context.categories.Where(i => i.index > old.index && i.index <= cat.index);
                        foreach (var item in list)
                        {
                            item.index--;
                        }
                    }
                }
                else
                {
                    var oldList = context.categories.Where(i => i.parentId == old.parentId && i.index >= old.index);
                    foreach (var item in oldList)
                    {
                        item.index--;
                    }

                    var newList = context.categories.Where(i => i.parentId == cat.parentId && i.index >= cat.index);
                    foreach (var item in newList)
                    {
                        item.index++;
                    }
                }

                context.SaveChanges();
            }
        }
    }
}