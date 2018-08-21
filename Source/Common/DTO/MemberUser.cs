namespace Insight.Base.Common.DTO
{
    public class MemberUser
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 上级ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 用户登录名
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 用户描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool isInvalid { get; set; }
    }
}