using System;

namespace Insight.Base.Common.Entity
{
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

        /// <summary>
        /// 节点权限情况
        /// </summary>
        public string Description { get; set; }
    }
}
