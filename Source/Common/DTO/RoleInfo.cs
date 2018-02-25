using System;
using System.Collections.Generic;

namespace Insight.Base.Common.DTO
{
    public class RoleInfo
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 所属应用名称
        /// </summary>
        public string appName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        public bool isBuiltin { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public List<MemberInfo> members { get; set; }

        /// <summary>
        /// 功能授权
        /// </summary>
        public List<AppTree> funcs { get; set; }

        /// <summary>
        /// 数据授权
        /// </summary>
        public List<AppTree> datas { get; set; }
    }

    public class MemberInfo
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 父节点ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 节点类型（成员类型：1、用户；2、用户组；3、岗位）
        /// </summary>
        public int nodeType { get; set; }

        /// <summary>
        /// 成员ID
        /// </summary>
        public string memberId { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string name { get; set; }
    }
}
