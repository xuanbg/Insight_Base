using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Entity;
using static Insight.Base.Common.Parameters;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Security : ISecurity
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
            var context = WebOperationContext.Current;
            if (context == null) return;

            var response = context.OutgoingResponse;
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type, Authorization");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        /// <summary>
        /// 联通性测试接口
        /// </summary>
        /// <returns>Result</returns>
        public Result Test()
        {
            var result = new Result();
            result.Success();
            return result;
        }

        /// <summary>
        /// 获取指定账户的Code
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>Result</returns>
        public Result GetCode(string account)
        {
            var token = new AccessToken { Account = account };
            var verify = new Compare(token);
            var result = verify.Result;

            return result;
        }

        /// <summary>
        /// 获取指定账户的AccessToken
        /// </summary>
        /// <param name="id">SessionID</param>
        /// <param name="account">用户账号</param>
        /// <param name="signature">用户签名</param>
        /// <param name="deptid">登录部门ID（可为空）</param>
        /// <returns>Result</returns>
        public Result GetToken(int id, string account, string signature, string deptid)
        {
            var token = new AccessToken {ID = id, Account = account};
            var verify = new Compare(token, signature, deptid);
            var result = verify.Result;

            return result;
        }

        /// <summary>
        /// 移除指定账户的AccessToken
        /// </summary>
        /// <returns>Result</returns>
        public Result RemoveToken()
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            verify.Basis.SignOut();
            return result;
        }

        /// <summary>
        /// 刷新AccessToken，延长过期时间
        /// </summary>
        /// <returns>Result</returns>
        public Result RefreshToken()
        {
            var verify = new Compare(60);
            var result = verify.Result;

            return result;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="action">需要鉴权的操作ID</param>
        /// <returns>Result</returns>
        public Result Verification(string action)
        {
            var verify = new Compare(action);
            var result = verify.Result;

            return result;
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="type">验证类型</param>
        /// <param name="time">过期时间（分钟）</param>
        /// <returns>Result</returns>
        public Result NewCode(string mobile, int type, int time)
        {
            if (!Verify()) return _Result;

            var record = SmsCodes.OrderByDescending(r => r.CreateTime).FirstOrDefault(r => r.Mobile == mobile && r.Type == type);
            if (record != null && (DateTime.Now - record.CreateTime).TotalSeconds < 60)
            {
                var result = new JsonResult();
                result.TimeTooShort();
                return result;
            }

            var code = Parameters.Random.Next(100000, 999999).ToString();
            record = new VerifyRecord
            {
                Type = type,
                Mobile = mobile,
                Code = code,
                FailureTime = DateTime.Now.AddMinutes(time),
                CreateTime = DateTime.Now
            };
            SmsCodes.Add(record);

            var msg = $"已经为手机号【{mobile}】的用户生成了类型为【{type}】的短信验证码：【{code}】。此验证码将于{record.FailureTime}失效。";
            var ts = new ThreadStart(() => new Logger("700501", msg).Write());
            new Thread(ts).Start();

            _Result.Success(code);
            return _Result;
        }

        /// <summary>
        /// 验证验证码是否正确
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="code">验证码</param>
        /// <param name="type">验证码类型</param>
        /// <param name="remove">是否验证成功后删除记录</param>
        /// <returns>Result</returns>
        public Result VerifyCode(string mobile, string code, int type, bool remove = true)
        {
            if (!Verify()) return _Result;

            SmsCodes.RemoveAll(c => c.FailureTime < DateTime.Now);
            var record = SmsCodes.FirstOrDefault(c => c.Mobile == mobile && c.Code == code && c.Type == type);
            if (record == null)
            {
                var result = new JsonResult();
                result.SMSCodeError();
                return result;
            }

            if (!remove) return _Result;

            SmsCodes.RemoveAll(c => c.Mobile == mobile && c.Type == type);
            return _Result;
        }

        /// <summary>
        /// 生成图形验证码
        /// </summary>
        /// <param name="id">验证图形ID</param>
        /// <returns>Result</returns>
        public Result GetPicCode(string id)
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
        public Result VerifyPicCode(string id, string code)
        {
            if (!Verify()) return _Result;

            throw new NotImplementedException();
        }

        private Result _Result = new Result();

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var verify = new Compare(action);
            _Result = verify.Result;

            return _Result.Successful;
        }
    }

    public class Puzzle
    {
        public object Name { get; set; }
        public SortedDictionary<string, string> Fragments { get; set; }
    }
}