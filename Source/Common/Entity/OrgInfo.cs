using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 组织机构
    /// </summary>
    public class OrgInfo
    {
        public Guid ID { get; set; }
        public Guid? ParentId { get; set; }
        public int NodeType { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Alias { get; set; }
        public string Code { get; set; }
        public List<OrgMember> Members { get; set; }
    }

    /// <summary>
    /// 岗位成员
    /// </summary>
    public class OrgMember
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string LoginName { get; set; }
        public bool Validity { get; set; }
    }

}
