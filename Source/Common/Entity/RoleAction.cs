using System;

namespace Insight.Base.Common.Entity
{
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

        /// <summary>
        /// 节点权限情况
        /// </summary>
        public string Description { get; set; }
    }
}
