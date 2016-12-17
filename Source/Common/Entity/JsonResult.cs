using Insight.Utils.Entity;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public class JsonResult:Result
    {
        /// <summary>
        /// 未知的验证码类型（411）
        /// </summary>
        public void UnknownSmsType()
        {
            Successful = false;
            Code = "411";
            Name = "UnknownSmsType";
            Message = "未知的验证码类型";
        }

        /// <summary>
        /// 请求发送验证码时间间隔过短（412）
        /// </summary>
        public void TimeTooShort()
        {
            Successful = false;
            Code = "412";
            Name = "TimeIntervalTooShort";
            Message = "请求发送验证码时间间隔过短，请稍后再试";
        }

        /// <summary>
        /// 短信验证码错误（413）
        /// </summary>
        public void SMSCodeError()
        {
            Successful = false;
            Code = "413";
            Name = "SMSCodeError";
            Message = "短信验证码错误";
        }

        /// <summary>
        /// 事件代码已使用（414）
        /// </summary>
        public void EventCodeUsed()
        {
            Successful = false;
            Code = "414";
            Name = "EventCodeUsed";
            Message = "事件代码已使用，请勿重复为该代码配置日志规则";
        }

        /// <summary>
        /// 事件规则无需配置（415）
        /// </summary>
        public void EventWithoutConfig()
        {
            Successful = false;
            Code = "415";
            Name = "EventWithoutConfig";
            Message = "事件等级为：0/1/7的，无需配置事件规则";
        }

        /// <summary>
        /// 事件代码未配置（416）
        /// </summary>
        public void EventCodeNotConfig()
        {
            Successful = false;
            Code = "416";
            Name = "EventCodeNotConfig";
            Message = "未配置的事件代码，请先为该代码配置日志规则";
        }

        /// <summary>
        /// 事件代码错误（417）
        /// </summary>
        public void InvalidEventCode()
        {
            Successful = false;
            Code = "417";
            Name = "InvalidEventCode";
            Message = "错误的事件代码";
        }

        /// <summary>
        /// 指定的编码方案不存在（418）
        /// </summary>
        public void CodeSchemeNotExists()
        {
            Successful = false;
            Code = "418";
            Name = "CodeSchemeNotExists";
            Message = "指定的编码方案不存在";
        }

        /// <summary>
        /// 错误的支付密码（419）
        /// </summary>
        public void InvalidPayKey()
        {
            Successful = false;
            Code = "419";
            Name = "InvalidPayKey";
            Message = "错误的支付密码";
        }
    }
}
