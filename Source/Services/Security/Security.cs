using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using static Insight.Base.Common.Params;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class Security : ISecurity
    {
        private Token _Session;

        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 联通性测试接口
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> Test()
        {
            return new Result<object>().Success();
        }

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>Result</returns>
        public Result<object> GetCode(string account)
        {
            var userId = Core.GetUserId(account);


            return _Result.Success(userId);
        }

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> GetToken(string account, string signature, string deptid)
        {
            var userId = Core.GetUserId(account);
            if (string.IsNullOrEmpty(userId)) return _Result.BadRequest();

            var token = Core.GetToken(userId);
            if (token.UserIsInvalid()) return _Result.Disabled();

            return _Result.Success(token);
        }

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> RemoveToken()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.successful) return result;

            verify.Basis.Offline(verify.Token.id);
            return result;
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> RefreshToken()
        {
            var compare = new Compare();
            _Result = compare.Result;
            return _Result.successful ? compare.Verify(null, 2) : _Result;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        public Result<object> Verification(string action)
        {
            var compare = new Compare();
            _Result = compare.Result;
            return _Result.successful ? compare.Verify(action) : _Result;
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="code">短信验证码</param>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> SetPayPW(string code, string password)
        {
            if (!Verify()) return _Result;

        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>Result</returns>
        public Result<object> VerifyPayPW(string password)
        {
            if (!Verify()) return _Result;

            return result.Value ? _Result : _Result.InvalidPayKey();
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>Result</returns>
        public Result<object> NewCode(string mobile, int type, int time)
        {
            if (!Verify(null, 60)) return _Result;

            var code = Params.Random.Next(100000, 999999).ToString();
            var key = type + mobile;
            var expire = DateTime.Now.AddMinutes(time);

            var msg = $"已经为手机号【{mobile}】的用户生成了类型为【{type}】的短信验证码：【{code}】。此验证码将于{expire}失效。";
            Task.Run(() => Logger.Write("700501", msg));

            return _Result.Created(code);
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>Result</returns>
        public Result<object> VerifyCode(string mobile, string code, int type, bool remove = true)
        {
            if (!Verify()) return _Result;

            return VerifySmsCode(type + mobile, code, remove) ? _Result : _Result.SMSCodeError();
        }

        /// <summary>
        /// 生成图形验证码
        /// </summary>
        /// <param name="id">验证图形ID</param>
        /// <returns>Result</returns>
        public Result<object> GetPicCode(string id)
        {
            if (!Verify()) return _Result;

            throw new NotImplementedException();
        }

        /// <summary>
        /// 验证图形验证码是否正确
        /// </summary>
        /// <param name="id">验证图形ID</param>
        /// <param name="code">验证码</param>
        /// <returns>Result</returns>
        public Result<object> VerifyPicCode(string id, string code)
        {
            if (!Verify()) return _Result;

            throw new NotImplementedException();
        }

        private Result<object> _Result = new Result<object>();

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <param name="limit">单位时间(秒)内限制调用，0：不限制</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null, int limit = 0)
        {
            var compare = new Compare(limit);
            _Result = compare.Result;
            if (!_Result.successful) return false;

            _Session = compare.Basis;
            _Result = compare.Verify(action);
            return _Result.successful;
        }
    }

    public class Puzzle
    {
        public object Name { get; set; }
        public SortedDictionary<string, string> Fragments { get; set; }
    }
}