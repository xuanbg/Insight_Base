using System;
using System.Collections.Generic;
using System.Data.Entity;
using Insight.Base.Common.Entity;

namespace Insight.Base.Common
{
    public class DbHelper
    {
        /// <summary>
        /// 插入数据库记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entry">数据实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool Insert<T>(T entry) where T : class
        {
            using (var context = new BaseEntities())
            {
                var obj = context.Set<T>();
                obj.Add(entry);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 插入多条数据库记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entrys">数据实体集合</param>
        /// <returns>bool 是否成功</returns>
        public static bool Insert<T>(List<T> entrys) where T : class
        {
            using (var context = new BaseEntities())
            {
                var obj = context.Set<T>();
                obj.AddRange(entrys);
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除数据库记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entry">数据实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool Delete<T>(T entry) where T : class
        {
            using (var context = new BaseEntities())
            {
                var obj = context.Set<T>();
                obj.Attach(entry);
                context.Entry(entry).State = EntityState.Deleted;
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 删除多条数据库记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entrys">数据实体集合</param>
        /// <returns>bool 是否成功</returns>
        public static bool Delete<T>(List<T> entrys) where T : class
        {
            using (var context = new BaseEntities())
            {
                entrys.ForEach(i =>
                {
                    var obj = context.Set<T>();
                    obj.Attach(i);
                    context.Entry(i).State = EntityState.Deleted;
                });
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 更新数据库记录
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entry">数据实体</param>
        /// <returns>bool 是否成功</returns>
        public static bool Update<T>(T entry) where T : class
        {
            using (var context = new BaseEntities())
            {
                var obj = context.Set<T>();
                obj.Attach(entry);
                context.Entry(entry).State = EntityState.Modified;
                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}