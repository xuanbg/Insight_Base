using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 组织机构
    /// </summary>
    [Table("uco_organization")]
    public class Org
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 上级ID
        /// </summary>
        [Column("parent_id")]
        public string parentId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        [Column("node_type")]
        public int nodeType { get; set; }

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
        /// 全称
        /// </summary>
        public string fullname { get; set; }

        /// <summary>
        /// 职能ID，字典
        /// </summary>
        [Column("position_id")]
        public string positionId { get; set; }

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
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }

        /// <summary>
        /// 成员用户
        /// </summary>
        public List<OrgMember> members { get; set; }
    }


    [Table("uco_org_member")]
    public class OrgMember
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("org_id")]
        public string orgId { get; set; }

        /// <summary>
        /// 成员ID
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