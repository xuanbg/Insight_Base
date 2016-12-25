using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    public class RoleInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否内置
        /// </summary>
        public bool BuiltIn { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Validity { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 角色操作权限集合
        /// </summary>
        public List<RoleAction> Actions { get; set; }

        /// <summary>
        /// 角色数据权限集合
        /// </summary>
        public List<RoleData> Datas { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public List<RoleMember> Members { get; set; }
    }
}
