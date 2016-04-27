using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
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
}
