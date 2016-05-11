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
        /// 角色成员
        /// </summary>
        public List<MemberUsers> MemberUsers { get; set; }

        /// <summary>
        /// 角色操作权限集合
        /// </summary>
        public List<RoleAction> Actions { get; set; }

        /// <summary>
        /// 角色数据权限集合
        /// </summary>
        public List<RoleData> Datas { get; set; }
    }

    /// <summary>
    /// 角色成员
    /// </summary>
    public class RoleMember
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 父节点ID
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 成员ID
        /// </summary>
        public Guid MemberId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 节点类型（成员类型：1、用户；2、用户组；3、岗位）
        /// </summary>
        public int NodeType { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 角色成员用户
    /// </summary>
    public class MemberUsers
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用户登录名
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户备注
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Validity { get; set; }

    }

    /// <summary>
    /// 角色操作权限
    /// </summary>
    public class RoleAction
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 父节点ID（数据节点：业务模块ID）
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 模块功能ID
        /// </summary>
        public Guid ActionId { get; set; }

        /// <summary>
        /// 原权限：0、只读；1、读写
        /// </summary>
        public int? Action { get; set; }

        /// <summary>
        /// 新权限：0、只读；1、读写
        /// </summary>
        public int? Permit { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public int NodeType { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 角色数据权限
    /// </summary>
    public class RoleData
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 父节点ID（数据节点：业务模块ID）
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// 数据授权模式：0、相对模式；1、用户模式；2、部门模式
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// 模式ID或部门/用户ID（绝对模式）
        /// </summary>
        public Guid ModeId { get; set; }

        /// <summary>
        /// 原权限：0、只读；1、读写
        /// </summary>
        public int? Permission { get; set; }

        /// <summary>
        /// 新权限：0、只读；1、读写
        /// </summary>
        public int? Permit { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public int NodeType { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
    }

}
