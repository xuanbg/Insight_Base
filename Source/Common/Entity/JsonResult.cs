using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public class JsonResult:Result
    {
        /// <summary>
        /// 用户多地登录（202）
        /// </summary>
        public void Multiple()
        {
            Successful = true;
            Code = "202";
            Name = "MultipleLogin";
            Message = "用户已在其他设备登录";
        }

        /// <summary>
        /// 无需刷新（205）
        /// </summary>
        public void WithoutRefresh()
        {
            Successful = true;
            Code = "205";
            Name = "WithoutRefresh";
            Message = "尚未过期，无需刷新";
        }

        /// <summary>
        /// 用户未取得授权（403）
        /// </summary>
        public void Forbidden()
        {
            Successful = false;
            Code = "403";
            Name = "Forbidden";
            Message = "当前用户未取得授权";
        }

        /// <summary>
        /// AccessToken已过期（405）
        /// </summary>
        public void Expired()
        {
            Successful = false;
            Code = "405";
            Name = "AccessTokenExpired";
            Message = "AccessToken已过期";
        }

        /// <summary>
        /// AccessToken已失效（406）
        /// </summary>
        public void Failured()
        {
            Successful = false;
            Code = "406";
            Name = "AccessTokenFailured";
            Message = "AccessToken已失效";
        }

        /// <summary>
        /// 已在其他设备登录（407）
        /// </summary>
        public void SignInOther()
        {
            Successful = false;
            Code = "407";
            Name = "SignInOtherDevice";
            Message = "用户已在其他设备登录";
        }

        /// <summary>
        /// 账号已锁定（408）
        /// </summary>
        public void AccountIsBlocked()
        {
            Successful = false;
            Code = "408";
            Name = "AccountIsBlocked";
            Message = "账号已锁定";
        }

        /// <summary>
        /// 用户已存在（409）
        /// </summary>
        public void AccountExists()
        {
            Successful = false;
            Code = "409";
            Name = "AccountAlreadyExists";
            Message = "用户已存在";
        }

        /// <summary>
        /// 用户被禁止登录（410）
        /// </summary>
        public void Disabled()
        {
            Successful = false;
            Code = "410";
            Name = "AccountIsDisabled";
            Message = "当前用户被禁止登录";
        }

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
