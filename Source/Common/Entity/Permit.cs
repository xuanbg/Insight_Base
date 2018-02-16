using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    public class Permit
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 导航ID
        /// </summary>
        [Column("navigator_id")]
        public string navigatorId { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 接口路由
        /// </summary>
        public string routes { get; set; }
    }

    [Table("ucv_user_role")]
    public class UserRole
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }

        /// <summary>
        /// 模式ID
        /// </summary>
        [Column("dept_id")]
        public string deptId { get; set; }
    }
}
