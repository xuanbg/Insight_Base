using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    [Table("ucb_user")]
    public class User
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 姓名/昵称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 注册邮箱
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 登录密码，保存密码的md5值，初始密码123456
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// 支付密码，保存密码的md5值
        /// </summary>
        [Column("pay_pw")]
        public string payPassword { get; set; }

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
        /// 是否失效：0、有效；1、失效
        /// </summary>
        [Column("is_invalid")]
        public bool isInvalid { get; set; }

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
        /// 授予用户的功能权限
        /// </summary>
        public List<AppTree> funcs { get; set; }

        /// <summary>
        /// 授予用户的数据权限
        /// </summary>
        public List<AppTree> datas { get; set; }
    }

    [Table("ucg_group")]
    public class Group
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
        /// 姓名/昵称
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
        /// 用户组成员
        /// </summary>
        public List<GroupMember> members { get; set; }
    }

    [Table("ucg_group_member")]
    public class GroupMember
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("group_id")]
        public string groupId { get; set; }

        /// <summary>
        /// 是否预置：0、自定；1、预置
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }

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
