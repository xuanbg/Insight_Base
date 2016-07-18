using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Common
{
    public class Compare
    {
        /// <summary>
        /// 验证结果
        /// </summary>
        public JsonResult Result = new JsonResult().InvalidAuth();

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Token Basis;

        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        public Token Token;

        /// <summary>
        /// 当前Web操作上下文
        /// </summary>
        private readonly WebOperationContext Context = WebOperationContext.Current;

        /// <summary>
        /// 远程客户端信息
        /// </summary>
        private readonly MessageProperties Properties;

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
                Properties = OperationContext.Current.IncomingMessageProperties;
                var time = LimitCall(limit);
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
            if (Util.StringCompare(Token.LoginName, account)) action = null;

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
                Token = Util.Deserialize<Token>(json);
                Basis = TokenManage.Get(Token);
            }
            catch (Exception ex)
            {
                Context.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                var ts = new ThreadStart(() => new Logger("500601", $"提取验证信息失败。\r\nException:{ex}").Write());
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

            // 签名不正确或验证签名失败超过5次（冒用时）则不能通过验证，且连续失败计数累加1次
            if (Basis.Signature != Token.Signature || (Basis.FailureCount >= 5 && Basis.MachineId != Token.MachineId))
            {
                Basis.FailureCount++;
                if (Basis.FailureCount >= 5) Result.AccountIsBlocked();

                return;
            }

            // 检查验证信息是否过期
            if (!login && ExpiredCheck())
            {
                var check = Util.CheckMachineId || Util.CheckOpenID;
                var key = check ? null : Util.CreateKey(Basis);
                Result.Expired(key);
                return;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            Result.Success();

            // 如配置为不验证设备ID，设备ID不一致时返回多点登录信息
            if (Basis.MachineId != Token.MachineId) Result.Multiple();

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
        /// 检查验证信息是否过期
        /// </summary>
        /// <returns>bool 信息是否过期</returns>
        private bool ExpiredCheck()
        {
            // Session.ID失效（服务重启）或超过设定时间过期（长期未操作）
            if (Basis.ID != Token.ID || Basis.Expired < DateTime.Now)
            {
                Basis.Signature = Util.Hash(Guid.NewGuid().ToString());
                return true;
            }

            // 设备码不同造成过期（用户在另一台设备登录）
            if (Util.CheckMachineId && Basis.MachineId != Token.MachineId) return true;

            // OpenID不同造成过期（用户使用另一个微信账号登录）
            if (Util.CheckOpenID && Basis.OpenId != Token.OpenId) return true;

            // 在过期前1天通过验证，过期时间自动延期
            var time = Basis.Expired.AddDays(-1);
            if (time < DateTime.Now) Basis.Expired = DateTime.Now.AddHours(Util.Expired);

            return false;
        }

        /// <summary>
        /// 根据传入的时长返回当前调用的剩余限制时间（秒）
        /// </summary>
        /// <param name="seconds">限制访问时长（秒）</param>
        /// <returns>int 剩余限制时间（秒）</returns>
        private int LimitCall(int seconds)
        {
            var property = Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
            if (property == null) return 0;

            var ip = property.Address;
            if (!Util.Requests.ContainsKey(ip))
            {
                Util.Requests.Add(ip, DateTime.Now);
                return 0;
            }

            // 计算剩余秒数surplus
            var span = Util.Requests[ip].AddSeconds(seconds) - DateTime.Now;
            var surplus = (int)Math.Floor(span.TotalSeconds);

            // 已过限制时间，更新调用时间，返回0
            if (surplus <= 0)
            {
                Util.Requests[ip] = DateTime.Now;
                return 0;
            }

            // 未过限制时间，且调用间隔3秒，不更新调用时间返回剩余秒数
            if (seconds - surplus > 3) return surplus;

            // 更新调用时间，返回设定时间
            Util.Requests[ip] = DateTime.Now;
            return seconds;
        }
    }
}
