using System;
using System.Collections.Generic;

namespace Insight.Base.Common.Entity
{
    public class UserInfo
    {
        public Guid ID { get; set; }
        public bool BuiltIn { get; set; }
        public string Name { get; set; }
        public string LoginName { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public bool Validity { get; set; }

        public List<RoleAction> Actions { get; set; }

        public List<RoleData> Datas { get; set; }
    }
}
