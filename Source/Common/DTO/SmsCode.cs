namespace Insight.Base.Common.DTO
{
    public class SmsCode
    {
        /// <summary>
        /// 类型
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 是否验证后删除
        /// </summary>
        public bool remove { get; set; }
    }
}
