using System;
using System.Collections.Generic;

namespace Insight.Base.Common.DTO
{
    public class GroupInfo
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        public string tenantId { get; set; }

        /// <summary>
        /// 姓名/昵称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        public bool isBuiltin { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 用户组成员
        /// </summary>
        public List<MemberUser> members { get; set; }
    }
}
