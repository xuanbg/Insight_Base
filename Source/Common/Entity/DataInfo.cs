using System;

namespace Insight.Base.Common.Entity
{
    public class DataInfo
    {
        public Guid ID { get; set; }
        public Guid? ParentId { get; set; }
        public Guid RoleId { get; set; }
        public int Mode { get; set; }
        public Guid ModeId { get; set; }
        public int? Permission { get; set; }
        public int? Permit { get; set; }
        public int? Index { get; set; }
        public int? NodeType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
