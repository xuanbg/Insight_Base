using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;

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
        public static List<Catalog> GetCatalogs(string tenantId, string moduleId)
        {
            using (var context = new Entities())
            {
                var list = context.categories.Where(i => !i.isInvalid && i.tenantId == tenantId && i.moduleId == moduleId);

                return list.OrderBy(i => i.index).ToList();
            }
        }

        /// <summary>
        /// 获取分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>Catalog 分类对象实体</returns>
        public static Catalog GetCatalog(string id)
        {
            return DbHelper.Find<Catalog>(id);
        }

        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="catalog">分类对象实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool Add(Catalog catalog)
        {
            catalog.id = Util.NewId();
            catalog.isBuiltin = false;
            catalog.isInvalid = false;
            catalog.createTime = DateTime.Now;

            return DbHelper.Insert(catalog);
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="data">分类实体</param>
        /// <param name="catalog">分类对象实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool Edit(Catalog data, Catalog catalog)
        {
            data.parentId = catalog.parentId;
            data.index = catalog.index;
            data.code = catalog.code;
            data.name = catalog.name;
            data.alias = catalog.alias;
            data.remark = catalog.remark;

            return DbHelper.Update(data);
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>bool 是否成功</returns>
        public static bool Delete(string id)
        {
            var data = DbHelper.Find<Catalog>(id);
            if (data == null) return false;

            data.isInvalid = true;

            return DbHelper.Update(data);
        }

        /// <summary>
        /// 分类是否存在
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>bool 是否存在</returns>
        public static bool Existed(Catalog catalog)
        {
            using (var context = new Entities())
            {
                return context.categories.Any(i => i.id != catalog.id && i.tenantId == catalog.tenantId &&
                                                   i.moduleId == catalog.moduleId && i.parentId == catalog.parentId &&
                                                   (i.code == catalog.code || i.name == catalog.name));
            }
        }
    }
}
