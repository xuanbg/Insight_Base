using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    [Table("ucr_config")]
    public class DataConfig
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 类型：0、无归属；1、仅本人；2、仅本部门；3、部门所有；4、机构所有
        /// </summary>
        [Column("data_type")]
        public int dataType { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }
    }

    [Table("ucr_role")]
    public class Role
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("tenant_id")]
        public string tenantId { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("app_id")]
        public string appId { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("is_builtin")]
        public bool isBuiltin { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public List<RoleMember> members { get; set; }

        /// <summary>
        /// 角色功能权限
        /// </summary>
        public List<PermitFunt> permitFunts { get; set; }

        /// <summary>
        /// 角色数据权限
        /// </summary>
        public List<PermitData> permitDatas { get; set; }
    }

    [Table("ucr_role_member")]
    public class RoleMember
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 类型：1、用户；2、用户组；3、岗位
        /// </summary>
        [Column("member_type")]
        public int memberType { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 成员ID
        /// </summary>
        [Column("member_id")]
        public string memberId { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }

    [Table("ucr_role_function")]
    public class RoleFunction
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 功能ID
        /// </summary>
        [Column("function_id")]
        public string functionId { get; set; }

        /// <summary>
        /// 权限：0、拒绝；1、允许
        /// </summary>
        public int permit { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }


    [Table("ucr_role_data")]
    public class RoleData
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public string roleId { get; set; }

        /// <summary>
        /// 模块ID
        /// </summary>
        [Column("module_id")]
        public string moduleId { get; set; }

        /// <summary>
        /// 模式ID
        /// </summary>
        [Column("mode_id")]
        public string modeId { get; set; }

        /// <summary>
        /// 授权模式：0、相对模式；1、用户模式；2、部门模式
        /// </summary>
        public int mode { get; set; }

        /// <summary>
        /// 权限：0、只读；1、读写
        /// </summary>
        public int permit { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }
}
