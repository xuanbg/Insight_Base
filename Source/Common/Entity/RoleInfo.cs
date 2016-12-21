using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    public class RoleInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 是否内置
        /// </summary>
        public bool BuiltIn { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public List<RoleMember> Members { get; set; }

        /// <summary>
        /// 角色操作权限集合
        /// </summary>
        public List<RoleAction> Actions { get; set; }

        /// <summary>
        /// 角色数据权限集合
        /// </summary>
        public List<RoleData> Datas { get; set; }
    }

}
