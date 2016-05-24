using System;

namespace Insight.Base.Common.Entity
{
    public class ActionInfo
    {
        public Guid ID { get; set; }
        public Guid? ParentId { get; set; }
        public Guid RoleId { get; set; }
        public Guid ActionId { get; set; }
        public int? Action { get; set; }
        public int? Permit { get; set; }
        public int? Index { get; set; }
        public int? NodeType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
