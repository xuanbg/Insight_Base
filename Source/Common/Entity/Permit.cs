using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 导航/功能授权实体
    /// </summary>
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
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public byte[] icon { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public bool? permit { get; set; }

        /// <summary>
        /// 是否开始分组：0、否；1、是
        /// </summary>
        public bool isBegin { get; set; }

        /// <summary>
        /// 是否显示文字：0、隐藏；1、显示
        /// </summary>
        public bool isShowText { get; set; }

        /// <summary>
        /// 是否默认启动：0、否；1、是
        /// </summary>
        public bool isDefault { get; set; }
    }

    /// <summary>
    /// 功能授权关键数据
    /// </summary>
    public class PermitFunc
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public int permit { get; set; }
    }

    /// <summary>
    /// 数据授权关键数据
    /// </summary>
    public class PermitData
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
        /// 授权模式()数据权限
        /// </summary>
        public int mode { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public int permit { get; set; }
    }

    /// <summary>
    /// 应用树
    /// </summary>
    public class AppTree
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
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 图标类型
        /// </summary>
        public int nodeType { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public bool? permit { get; set; }
    }

    /// <summary>
    /// 用户角色表
    /// </summary>
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
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

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