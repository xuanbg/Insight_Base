using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insight.Base.Common.Entity
{
    public class Permit
    {
    }

    [Table("ucv_user_role")]
    public class UserRole
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }

        /// <summary>
        /// 模式ID
        /// </summary>
        [Column("dept_id")]
        public string deptId { get; set; }
    }
}
