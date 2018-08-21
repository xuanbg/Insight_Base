using System;
using System.Collections.Generic;

namespace Insight.Base.Common.DTO
{
    public class OrgDto
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 上级ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public string tenantId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
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
        public string positionId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 成员用户
        /// </summary>
        public List<MemberUser> members { get; set; }
    }
}