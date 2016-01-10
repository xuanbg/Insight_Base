using System;

namespace Insight.WS.Base.Common
{
    public class VerifyRecord
    {

        /// <summary>
        /// 类型：1、注册；2、找回密码；3、设置支付密码
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime FailureTime { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime CreateTime { get; set; }

    }
}
