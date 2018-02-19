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
        /// 父级ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// 图标类型
        /// </summary>
        public int mode { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 导航名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public byte[] icon { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public int? permit { get; set; }
    }

    public class PermitFunt : Permit
    {
        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 控制器命名空间
        /// </summary>
        public string className { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath { get; set; }
    }

    public class PermitData : Permit
    {
    }

    [Table("ucv_user_role")]
    public class UserRole
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

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