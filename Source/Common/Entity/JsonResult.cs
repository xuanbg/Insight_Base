using Insight.Utils.Entity;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public static class JsonResult
    {
        /// <summary>
        /// 未知的验证码类型（411）
        /// </summary>
        public static Result UnknownSmsType(this Result result)
        {
            result.Successful = false;
            result.Code = "411";
            result.Name = "UnknownSmsType";
            result.Message = "未知的验证码类型";
            return result;
        }

        /// <summary>
        /// 请求发送验证码时间间隔过短（412）
        /// </summary>
        public static Result TimeTooShort(this Result result)
        {
            result.Successful = false;
            result.Code = "412";
            result.Name = "TimeIntervalTooShort";
            result.Message = "请求发送验证码时间间隔过短，请稍后再试";
            return result;
        }

        /// <summary>
        /// 短信验证码错误（413）
        /// </summary>
        public static Result SMSCodeError(this Result result)
        {
            result.Successful = false;
            result.Code = "413";
            result.Name = "SMSCodeError";
            result.Message = "短信验证码错误";
            return result;
        }

        /// <summary>
        /// 事件代码已使用（414）
        /// </summary>
        public static Result EventCodeUsed(this Result result)
        {
            result.Successful = false;
            result.Code = "414";
            result.Name = "EventCodeUsed";
            result.Message = "事件代码已使用，请勿重复为该代码配置日志规则";
            return result;
        }

        /// <summary>
        /// 事件规则无需配置（415）
        /// </summary>
        public static Result EventWithoutConfig(this Result result)
        {
            result.Successful = false;
            result.Code = "415";
            result.Name = "EventWithoutConfig";
            result.Message = "事件等级为：0/1/7的，无需配置事件规则";
            return result;
        }

        /// <summary>
        /// 事件代码未配置（416）
        /// </summary>
        public static Result EventCodeNotConfig(this Result result)
        {
            result.Successful = false;
            result.Code = "416";
            result.Name = "EventCodeNotConfig";
            result.Message = "未配置的事件代码，请先为该代码配置日志规则";
            return result;
        }

        /// <summary>
        /// 事件代码错误（417）
        /// </summary>
        public static Result InvalidEventCode(this Result result)
        {
            result.Successful = false;
            result.Code = "417";
            result.Name = "InvalidEventCode";
            result.Message = "错误的事件代码";
            return result;
        }

        /// <summary>
        /// 指定的编码方案不存在（418）
        /// </summary>
        public static Result CodeSchemeNotExists(this Result result)
        {
            result.Successful = false;
            result.Code = "418";
            result.Name = "CodeSchemeNotExists";
            result.Message = "指定的编码方案不存在";
            return result;
        }

        /// <summary>
        /// 错误的支付密码（419）
        /// </summary>
        public static Result InvalidPayKey(this Result result)
        {
            result.Successful = false;
            result.Code = "419";
            result.Name = "InvalidPayKey";
            result.Message = "错误的支付密码";
            return result;
        }
    }
}
