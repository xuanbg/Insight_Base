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
        private AccessToken Token;

        /// <summary>
        /// 当前Web操作上下文
        /// </summary>
        private readonly WebOperationContext Context = WebOperationContext.Current;

        /// <summary>
        /// 最大授权数
        /// </summary>
        private const int MaxAuth = 999999999;

        /// <summary>
        /// 构造方法，用于刷新Token
        /// </summary>
        public Compare()
        {
            InitVerify(60);
            if (!CheckBasis()) return;

            Identify();
        }

        /// <summary>
        /// 构造方法，用于获取AccessToken
        /// </summary>
        /// <param name="token">传入参数</param>
        /// <param name="signature">用户签名</param>
        /// <param name="did">登录部门ID（可为空）</param>
        public Compare(AccessToken token, string signature, string did)
        {
            var time = Util.LimitCall(60);
            if (time > 0)
            {
                Result.TooFrequent(time);
                return;
            }

            var parse = new GuidParse(did);
            if (!parse.Successful)
            {
                Result.InvalidGuid();
                return;
            }

            Token = token;
            if (!CheckBasis()) return;

            Basis.DeptId = parse.Result;
            Identify(signature);
        }

        /// <summary>
        /// 构造方法
        /// 如account和LoginName一致，忽略鉴权
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="account">登录账号</param>
        public Compare(string action, string account)
        {
            InitVerify(0);
            if (!CheckBasis()) return;

            if (Util.StringCompare(Token.Account, account)) action = null;

            CompareToken(action);
        }

        /// <summary>
        /// 构造方法
        /// 如Action不为空，则同时进行鉴权
        /// 如limit大于0，则指定时间内的限制调用
        /// </summary>
        /// <param name="limit">单位时间(秒)内限制调用，0：不限制</param>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="userid">用户ID</param>
        public Compare(int limit, string action = null, Guid? userid = null)
        {
            InitVerify(limit);
            if (!CheckBasis()) return;

            if (Basis.UserId == userid) action = null;

            CompareToken(action);
        }

        /// <summary>
        /// 初始化验证数据
        /// </summary>
        /// <param name="limit"></param>
        private void InitVerify(int limit)
        {
            var time = Util.LimitCall(limit);
            if (time > 0)
            {
                Result.TooFrequent(time);
                return;
            }

            try
            {
                var headers = Context.IncomingRequest.Headers;
                var auth = headers[HttpRequestHeader.Authorization];
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                Token = Util.Deserialize<AccessToken>(json);
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
        /// 检验用户验证信息合法性
        /// </summary>
        /// <returns>bool 是否通过检查</returns>
        private bool CheckBasis()
        {
            Basis = TokenManage.Get(Token);
            if (Basis == null || Basis.ID > MaxAuth) return false;

            if (!Basis.Validity)
            {
                Result.Disabled();
                return false;
            }

            // 验证签名失败计数清零（距上次用户签名验证时间超过15分钟）
            if (Basis.FailureCount > 0 && (DateTime.Now - Basis.LastConnect).TotalMinutes > 15)
            {
                Basis.FailureCount = 0;
            }

            // 检查是否验证签名失败超过5次
            Basis.LastConnect = DateTime.Now;
            if (Basis.FailureCount < 5 || Basis.Stamp == Token.Stamp) return true;

            Result.AccountIsBlocked();
            return false;
        }

        /// <summary>
        /// 对Token进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool 是否通过验证</returns>
        private void CompareToken(string action = null)
        {
            // 检查验证信息是否过期
            if (ExpiredCheck())
            {
                Result.Expired();
                return;
            }

            // 验证Secret
            if (Basis.Secret != Token.Secret)
            {
                Basis.FailureCount++;
                Result.InvalidAuth();
                return;
            }

            // 验证设备特征码
            if (CheckStamp && Basis.Stamp != Token.Stamp)
            {
                Result.SignInOther();
                return;
            }

            Basis.Online();
            if (Basis.Stamp == Token.Stamp) Result.Success();
            else Result.Multiple();

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
        /// 对签名进行合法性校验
        /// </summary>
        /// <param name="signature">是否登录</param>
        /// <returns>bool 是否通过合法性校验</returns>
        private void Identify(string signature)
        {
            // 验证用户签名
            if (Basis.Signature != signature)
            {
                Basis.FailureCount++;
                Result.InvalidAuth();
                return;
            }

            Basis.Online();
            if (Basis.Expired <= DateTime.Now) Basis.InitSecret(Expired);

            if (Basis.UserType != 0) Basis.Stamp = Token.Stamp;

            Result.Success(Util.CreatorKey(Basis));
        }

        /// <summary>
        /// 对签名进行合法性校验
        /// </summary>
        /// <returns>bool 是否通过合法性校验</returns>
        private void Identify()
        {
            // 未超时
            if (Basis.Expired > DateTime.Now)
            {
                Result.WithoutRefresh();
                return;
            }

            // 已失效
            if (Basis.FailureTime < DateTime.Now)
            {
                Result.Failured();
                return;
            }

            // 验证用户签名
            if (Basis.RefreshKey != Token.Secret)
            {
                Basis.FailureCount++;
                Result.InvalidAuth();
                return;
            }

            Basis.Refresh();
            Result.Success();
        }

        /// <summary>
        /// 检查验证信息是否过期
        /// </summary>
        /// <returns>bool 信息是否过期</returns>
        private bool ExpiredCheck()
        {
            if (Basis.FailureTime < DateTime.Now)
            {
                Basis.InitSecret(Expired);
                return true;
            }

            if (Basis.Expired < DateTime.Now) return true;

            // Session.ID失效（服务重启）
            if (Basis.ID != Token.ID) return true;

            // 设备码不同造成过期（用户在另一台设备登录）
            if (CheckStamp && Basis.Stamp != Token.Stamp) return true;

            // 如不自动延长有效期，立即返回
            if (!AutoExten) return false;

            // 在过期前通过验证，过期时间自动延期
            var count = Basis.UserType == 0 ? 1 : 24;
            var time = DateTime.Now.AddHours(count);
            if (time > Basis.Expired && time <= Basis.FailureTime)
            {
                Basis.Expired = Basis.Expired.AddHours(count);
            }

            return false;
        }
    }
}
