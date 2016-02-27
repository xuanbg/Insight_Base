using System;

namespace Insight.WS.Base.Common.Entity
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 自增ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 登录用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户签名，用户名（大写）+ 密码MD5值的结果的MD5值
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 登录部门ID
        /// </summary>
        public Guid? DeptId { get; set; }

        /// <summary>
        /// 登录部门全称
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public bool Validity { get; set; }

        /// <summary>
        /// 用户机器码
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 上次连接时间
        /// </summary>
        public DateTime LastConnect { get; set; }

        /// <summary>
        /// 用户在线状态
        /// </summary>
        public bool OnlineStatus { get; set; }

    }
}
