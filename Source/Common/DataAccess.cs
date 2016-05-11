using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;

namespace Insight.Base.Common
{
    public class DataAccess
    {
        /// <summary>
        /// 获取服务列表
        /// </summary>
        /// <returns>服务列表</returns>
        public static List<SYS_Interface> GetServiceList()
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Interface.ToList();
            }
        }

        /// <summary>
        /// 根据用户登录名获取用户对象实体
        /// </summary>
        /// <param name="str">用户登录名</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(string str)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(s => s.LoginName == str);
            }
        }

        /// <summary>
        /// 根据输入的参数生成调整Index值的SQL命令
        /// </summary>
        /// <param name="dataTable">数据表名称</param>
        /// <param name="oldIndex">原Index值</param>   
        /// <param name="newIndex">现Index值</param>   
        /// <param name="parentId">父节点或分类ID</param>   
        /// <param name="isCategoryId">true:根据CategoryId（分类下）；false:根据ParentId（父节点下）；</param>
        /// <param name="moduleId">主数据类型（如果数据表名为BASE_Category则必须输入该参数</param>
        /// <returns>string SQL命令</returns>
        public static string ChangeIndex(string dataTable, int oldIndex, int newIndex, Guid? parentId, bool isCategoryId = true, Guid? moduleId = null)
        {
            var smb = oldIndex < newIndex ? "-" : "+";
            var join = dataTable.Substring(0, 3) == "MDG" ? "join MasterData M on M.ID = D.MID " : "";
            var t1 = isCategoryId ? "CategoryId" : "ParentId";
            var t2 = parentId == null ? "is null" : $"= '{parentId}'";
            var t3 = dataTable == "BASE_Category" ? $"and ModuleId = '{moduleId}' " : "";
            var r1 = oldIndex < newIndex ? ">" : "<";
            var r2 = oldIndex < newIndex ? "<=" : ">=";
            var sql = $"update D set [Index] = D.[Index] {smb} 1 from {dataTable} D {join}";
            sql += $"where {t1} {t2} {t3}and [Index] {r1} {oldIndex} and [Index] {r2} {newIndex}";
            return sql;
        }

        /// <summary>
        /// 校验支付密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="key">支付密码</param>
        /// <returns>bool 是否正确</returns>
        public static bool ConfirmPayKey(Guid id, string key)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.Any(u => u.ID == id && u.PayPassword == key);
            }
        }
    }
}
