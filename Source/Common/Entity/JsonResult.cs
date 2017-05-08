using Insight.Utils.Entity;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public static class JsonResult
    {
        /// <summary>
        /// 未知的验证码类型（440）
        /// </summary>
        public static Result UnknownSmsType(this Result result)
        {
            result.successful = false;
            result.code = "440";
            result.name = "UnknownSmsType";
            result.message = "未知的验证码类型";
            return result;
        }

        /// <summary>
        /// 短信验证码错误（441）
        /// </summary>
        public static Result SMSCodeError(this Result result)
        {
            result.successful = false;
            result.code = "441";
            result.name = "SMSCodeError";
            result.message = "短信验证码错误";
            return result;
        }

        /// <summary>
        /// 事件代码已使用（412）
        /// </summary>
        public static Result EventCodeUsed(this Result result)
        {
            result.successful = false;
            result.code = "412";
            result.name = "EventCodeUsed";
            result.message = "事件代码已使用，请勿重复为该代码配置日志规则";
            return result;
        }

        /// <summary>
        /// 事件规则无需配置（413）
        /// </summary>
        public static Result EventWithoutConfig(this Result result)
        {
            result.successful = false;
            result.code = "413";
            result.name = "EventWithoutConfig";
            result.message = "事件等级为：0/1/7的，无需配置事件规则";
            return result;
        }

        /// <summary>
        /// 事件代码未配置（414）
        /// </summary>
        public static Result EventCodeNotConfig(this Result result)
        {
            result.successful = false;
            result.code = "414";
            result.name = "EventCodeNotConfig";
            result.message = "未配置的事件代码，请先为该代码配置日志规则";
            return result;
        }

        /// <summary>
        /// 事件代码错误（415）
        /// </summary>
        public static Result InvalidEventCode(this Result result)
        {
            result.successful = false;
            result.code = "415";
            result.name = "InvalidEventCode";
            result.message = "错误的事件代码";
            return result;
        }

        /// <summary>
        /// 错误的支付密码（416）
        /// </summary>
        public static Result InvalidPayKey(this Result result)
        {
            result.successful = false;
            result.code = "416";
            result.name = "InvalidPayKey";
            result.message = "错误的支付密码";
            return result;
        }

        /// <summary>
        /// 错误的支付密码（417）
        /// </summary>
        public static Result PayKeyNotExists(this Result result)
        {
            result.successful = false;
            result.code = "417";
            result.name = "PayKeyNotExists";
            result.message = "支付密码未设置";
            return result;
        }

        /// <summary>
        /// 指定的编码方案不存在（450）
        /// </summary>
        public static Result CodeSchemeNotExists(this Result result)
        {
            result.successful = false;
            result.code = "450";
            result.name = "CodeSchemeNotExists";
            result.message = "指定的编码方案不存在";
            return result;
        }
    }
}
