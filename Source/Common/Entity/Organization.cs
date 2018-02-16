using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 组织机构
    /// </summary>
    public class Organization
    {
        public Guid ID { get; set; }
        public Guid? ParentId { get; set; }
        public int NodeType { get; set; }
        public int Index { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Alias { get; set; }
        public string Code { get; set; }
        public List<MemberUser> Members { get; set; }
    }
}