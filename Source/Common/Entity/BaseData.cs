using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    [Table("ibd_category")]
    public class Catalog
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 上级分类ID
        /// </summary>
        [Column("parent_id")]
        public string parentId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 业务模块ID
        /// </summary>
        [Column("module_id")]
        public string moduleId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 别名/简称
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("is_builtin")]
        public bool isBuiltin { get; set; }

        /// <summary>
        /// 是否失效：0、有效；1、失效
        /// </summary>
        [Column("is_invalid")]
        public bool isInvalid { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_dept_id")]
        public string creatorDeptId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creator { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }

    [Table("ibd_region")]
    public class Region
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 上级区划ID
        /// </summary>
        [Column("parent_id")]
        public string parentId { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        public int grade { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 别名/简称
        /// </summary>
        public string alias { get; set; }
    }

    [Table("ibd_image")]
    public class ImageData
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        [Column("category_id")]
        public string categoryId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        [Column("image_type")]
        public int imageType { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 扩展名
        /// </summary>
        public string expand { get; set; }

        /// <summary>
        /// 涉密等级，字典
        /// </summary>
        public string secrec { get; set; }

        /// <summary>
        /// 页数
        /// </summary>
        public int pages { get; set; }

        /// <summary>
        /// 文件字节数
        /// </summary>
        public long size { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        [Column("file_path")]
        public string filePath { get; set; }

        /// <summary>
        /// 电子影像内容
        /// </summary>
        public byte[] image { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否失效：0、有效；1、失效
        /// </summary>
        [Column("is_invalid")]
        public bool isInvalid { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_dept_id")]
        public string creatorDeptId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string creator { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }

    [Table("ibd_param")]
    public class Parameter
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        [Column("module_id")]
        public string moduleId { get; set; }

        /// <summary>
        /// 选项ID
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 选项参数值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 生效机构ID
        /// </summary>
        [Column("dept_id")]
        public string deptId { get; set; }

        /// <summary>
        /// 所属用户
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }
}
