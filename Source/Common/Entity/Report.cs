using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 分期规则
    /// </summary>
    [Table("ibr_rule")]
    public class Rule
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 周期类型
        /// </summary>
        [Column("cycle_type")]
        public int cycleType { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 周期数
        /// </summary>
        public int? cycle { get; set; }

        /// <summary>
        /// 起始日期
        /// </summary>
        [Column("start_time")]
        public DateTime? startTime { get; set; }

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
        /// 创建部门ID
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

    /// <summary>
    /// 报表模板
    /// </summary>
    [Table("ibr_templates")]
    public class Template
    {
        /// <summary>
        /// 唯一ID
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
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }

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
        /// 创建部门ID
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

    /// <summary>
    /// 报表定义
    /// </summary>
    [Table("ibr_definition")]
    public class Definition
    {
        /// <summary>
        /// 唯一ID
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
        /// 模板ID
        /// </summary>
        [Column("template_id")]
        public string templateId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 分期模式
        /// </summary>
        public int mode { get; set; }

        /// <summary>
        /// 延时(小时)
        /// </summary>
        public int delay { get; set; }

        /// <summary>
        /// 报表类型
        /// </summary>
        [Column("report_type")]
        public int reportType { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        [Column("data_source")]
        public string dataSource { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 创建部门ID
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

        /// <summary>
        /// 报表分期
        /// </summary>
        public List<Period> periods { get; set; }

        /// <summary>
        /// 会计主体
        /// </summary>
        public List<Entity> entities { get; set; }
    }

    /// <summary>
    /// 报表分期
    /// </summary>
    [Table("ibr_period")]
    public class Period
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 报表ID
        /// </summary>
        [Column("report_id")]
        public string reportId { get; set; }

        /// <summary>
        /// 分期规则ID
        /// </summary>
        [Column("rule_id")]
        public string ruleId { get; set; }
    }

    /// <summary>
    /// 会计主体
    /// </summary>
    [Table("ibr_entity")]
    public class Entity
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 报表ID
        /// </summary>
        [Column("report_id")]
        public string reportId { get; set; }

        /// <summary>
        /// 组织机构ID
        /// </summary>
        [Column("org_id")]
        public string orgId { get; set; }

        /// <summary>
        /// 组织机构名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 报表成员（角色）
        /// </summary>
        public List<EntityMember> members { get; set; }
    }

    /// <summary>
    /// 会计主体成员
    /// </summary>
    [Table("ibr_member")]
    public class EntityMember
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 会计主体ID
        /// </summary>
        [Column("entity_id")]
        public string entityId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string name { get; set; }
    }

    /// <summary>
    /// 报表实例
    /// </summary>
    [Table("ibr_instances")]
    public class Instance
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 报表ID
        /// </summary>
        [Column("report_id")]
        public string reportId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public byte[] content { get; set; }

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

    /// <summary>
    /// 实例用户关系
    /// </summary>
    [Table("ibr_instanc_user")]
    public class InstancUser
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 会计主体ID
        /// </summary>
        [Column("instance_id")]
        public string entityId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }
    }

    /// <summary>
    /// 计划任务
    /// </summary>
    [Table("ibr_schedular")]
    public class Schedular
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 报表ID
        /// </summary>
        [Column("report_id")]
        public string reportId { get; set; }

        /// <summary>
        /// 分期规则ID
        /// </summary>
        [Column("rule_id")]
        public string ruleId { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        [Column("build_time")]
        public DateTime buildTime { get; set; }
    }
}
