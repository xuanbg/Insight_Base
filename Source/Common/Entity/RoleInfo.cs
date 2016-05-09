using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    public class RoleInfo
    {
        public Guid ID { get; set; }
        public bool BuiltIn { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RoleMember> Members { get; set; }
        public List<RoleAction> Actions { get; set; }
        public List<RoleData> Datas { get; set; }
    }

    /// <summary>
    /// 角色成员
    /// </summary>
    public class RoleMember
    {
        public Guid ID { get; set; }
        public Guid RoleId { get; set; }
        public int NodeType { get; set; }
        public int Index { get; set; }
        public Guid? ParentId { get; set; }
        public Guid MemberId { get; set; }
        public string Member { get; set; }
    }

    public class RoleResource
    {
        public Guid ID { get; set; }
        public Guid? ParentId { get; set; }
        public int Index { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? Permit { get; set; }
        public int? State { get; set; }
    }

}
