using System;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using static Insight.Base.Common.Parameters;

namespace Insight.Base.Common
{
    public class Compare
    {
        /// <summary>
        /// 验证结果
        /// </summary>
        public JsonResult Result = new JsonResult();

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Session Basis;

        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        public AccessToken Token;

        /// <summary>
        /// 当前Web操作上下文
        /// </summary>
        private readonly WebOperationContext Context = WebOperationContext.Current;

        /// <summary>
        /// 最大授权数
        /// </summary>
        private const int MaxAuth = 999999999;

        /// <summary>
        /// 构造方法
        /// 如Action不为空，则同时进行鉴权
        /// 如limit大于0，则指定时间内的限制调用
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="limit">单位时间(秒)内限制调用，0：不限制</param>
        /// <param name="login">是否登录验证</param>
        public Compare(string action = null, int limit = 0, bool login = false)
        {
            if (limit > 0)
            {
                var time = Util.LimitCall(limit);
                if (time > 0)
                {
                    Result.TooFrequent(time.ToString());
                    return;
                }
            }

            InitVerify();
            CompareToken(action, login);
        }

        /// <summary>
        /// 构造方法
        /// 如uid和UserId一致，忽略鉴权
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="uid">用户ID</param>
        public Compare(string action, Guid uid)
        {
            InitVerify();
            if (Token.UserId == uid) action = null;

            CompareToken(action);
        }

        /// <summary>
        /// 构造方法
        /// 如account和LoginName一致，忽略鉴权
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="account">登录账号</param>
        public Compare(string action, string account)
        {
            InitVerify();
            if (Util.StringCompare(Token.Account, account)) action = null;

            CompareToken(action);
        }

        /// <summary>
        /// 初始化验证数据
        /// </summary>
        private void InitVerify()
        {
            try
            {
                var headers = Context.IncomingRequest.Headers;
                var auth = headers[HttpRequestHeader.Authorization];
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                Token = Util.Deserialize<AccessToken>(json);
                Basis = TokenManage.Get(Token);
            }
            catch (Exception ex)
            {
                Context.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                var msg = $"提取验证信息失败。\r\nException:{ex}";
                var ts = new ThreadStart(() => new Logger("500101", msg).Write());
                new Thread(ts).Start();
                throw new Exception("提取验证信息失败");
            }
        }

        /// <summary>
        /// 对Session进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="login">是否登录验证</param>
        /// <returns>bool 是否通过验证</returns>
        private void CompareToken(string action = null, bool login = false)
        {
            if (Basis == null || Basis.ID > MaxAuth) return;

            if (!Basis.Validity)
            {
                Result.Disabled();
                return;
            }

            // 验证签名失败计数清零（距上次用户签名验证时间超过15分钟）
            if (Basis.FailureCount > 0 && (DateTime.Now - Basis.LastConnect).TotalMinutes > 15)
            {
                Basis.FailureCount = 0;
            }

            Basis.LastConnect = DateTime.Now;
            if (!Identify(login)) return;

            // 如配置为不验证设备ID，设备ID不一致时返回多点登录信息
            if (!CheckMachineId && Basis.MachineId != Token.MachineId) Result.Multiple();

            // 如action为空，立即返回；否则进行鉴权
            if (action == null) return;

            Guid aid;
            if (!Guid.TryParse(action, out aid))
            {
                Result.InvalidGuid();
                return;
            }

            // 根据传入的操作码进行鉴权
            var auth = new Authority(Basis.UserId, Basis.DeptId);
            if (auth.Identify(aid)) return;

            Result.Forbidden();
        }

        /// <summary>
        /// 对AccessToken进行合法性校验
        /// </summary>
        /// <param name="login">是否登录</param>
        /// <returns>bool 是否通过合法性校验</returns>
        private bool Identify(bool login)
        {
            // 检查验证信息是否过期
            if (!login && ExpiredCheck())
            {
                Basis.Secret = TokenManage.GetSecret(Basis.Signature);
                Result.Expired();
                return false;
            }

            // 检查是否验证签名失败超过5次
            if (Basis.FailureCount >= 5 && Basis.MachineId != Token.MachineId)
            {
                Result.AccountIsBlocked();
                return false;
            }

            // 验证用户签名（登录时）或Secret
            var identify = login ? Basis.Signature == Token.Signature : Basis.Secret == Token.Secret;
            if (!identify)
            {
                Basis.FailureCount++;
                Result.InvalidAuth();
                return false;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            Result.Success();
            if (!login) return true;

            // 登录时更新缓存信息
            Basis.OpenId = Token.OpenId;
            Basis.DeptId = Token.DeptId;
            Basis.DeptName = Token.DeptName;
            Basis.MachineId = Token.MachineId;
            return true;
        }

        /// <summary>
        /// 检查验证信息是否过期
        /// </summary>
        /// <returns>bool 信息是否过期</returns>
        private bool ExpiredCheck()
        {
            if (Basis.FailureTime < DateTime.Now) return true;

            // Session.ID失效（服务重启）
            if (Basis.ID != Token.ID) return true;

            // 设备码不同造成过期（用户在另一台设备登录）
            if (CheckMachineId && Basis.MachineId != Token.MachineId) return true;

            // OpenID不同造成过期（用户使用另一个微信账号登录）
            if (CheckOpenID && Basis.OpenId != Token.OpenId) return true;

            // 在过期前1天通过验证，过期时间自动延期
            var time = Basis.FailureTime.AddDays(-1);
            if (time < DateTime.Now) Basis.FailureTime = DateTime.Now.AddHours(Expired);

            return false;
        }
    }
}
