using System;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using Insight.Base.Common;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Server;

namespace Insight.Base.OAuth
{
    public class Compare
    {
        private readonly WebOperationContext _Context = WebOperationContext.Current;
        private AccessToken _Token;
        private Session _Basis;

        /// <summary>
        /// 验证结果
        /// </summary>
        public Result Result { get; } = new Result();

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Session Basis => _Basis ?? (_Basis = Common.GetSession(_Token.account));

        /// <summary>
        /// 构造方法，如Action不为空，则同时进行鉴权
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="limit">单位时间(秒)内限制调用，0：不限制</param>
        /// <param name="userid">用户ID</param>
        public Compare(string action = null, int limit = 0, Guid? userid = null)
        {
            if (!InitVerify(limit)) return;

            if (Basis.userId == userid) action = null;

            Verify(action);
        }

        /// <summary>
        /// 构造方法
        /// 如account和LoginName一致，忽略鉴权
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="account">登录账号</param>
        public Compare(string action, string account)
        {
            if (!InitVerify(0)) return;

            if (Basis.UserIsSame(account)) action = null;

            Verify(action);
        }

        /// <summary>
        /// 构造方法，用于获取Code
        /// </summary>
        /// <param name="token">传入参数</param>
        public Compare(AccessToken token)
        {
            _Token = token;
            var time = CallManage.LimitCall(GetKey(), 3);
            if (time > 0)
            {
                Result.TooFrequent(time);
                return;
            }

            if (!CheckBasis()) return;

            Result.Success(Basis.id.ToString("N"));
        }

        /// <summary>
        /// 构造方法，用于获取AccessToken
        /// </summary>
        /// <param name="token">传入参数</param>
        /// <param name="signature">用户签名</param>
        public Compare(AccessToken token, string signature)
        {
            _Token = token;
            var time = CallManage.LimitCall(GetKey(), 3);
            if (time > 0)
            {
                Result.TooFrequent(time);
                return;
            }

            if (!CheckBasis()) return;

            // 更新SessionID，验证用户签名
            if (!Basis.Verify(signature, 3))
            {
                Result.InvalidAuth();
                return;
            }

            // 如Token失效，则重新生成随机码、过期时间和失效时间
            if (DateTime.Now > Basis.FailureTime) Basis.InitSecret();

            Basis.Online(token.deptId);
            Result.Success(Basis.CreatorKey());
        }

        /// <summary>
        /// 构造方法，用于刷新Token
        /// </summary>
        /// <param name="limit">限制访问秒数</param>
        public Compare(int limit)
        {
            if (!InitVerify(limit)) return;

            // 未超时
            var now = DateTime.Now;
            if (now < Basis.ExpiryTime)
            {
                Result.WithoutRefresh();
                return;
            }

            // 已失效
            if (now > Basis.FailureTime)
            {
                Result.Failured();
                return;
            }

            // 验证用户签名
            if (!Basis.Verify(_Token.secret, 2))
            {
                Result.Failured();
                return;
            }

            Basis.Refresh();
            Result.Success(Basis.ExpiryTime);
        }

        /// <summary>
        /// 初始化验证数据
        /// </summary>
        /// <param name="limit">限制访问秒数</param>
        private bool InitVerify(int limit)
        {
            try
            {
                var headers = _Context.IncomingRequest.Headers;
                var auth = headers[HttpRequestHeader.Authorization];
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                _Token = Util.Deserialize<AccessToken>(json);

                var time = CallManage.LimitCall(GetKey(), limit);
                if (time <= 0) return CheckBasis();

                Result.TooFrequent(time);
                return false;
            }
            catch (Exception ex)
            {
                _Context.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                var msg = $"提取验证信息失败。\r\nException:{ex}";
                var ts = new ThreadStart(() => new Logger("500101", msg).Write());
                new Thread(ts).Start();
                return false;
            }
        }

        /// <summary>
        /// 检查Session是否正常(正常授权、未封禁、未锁定)
        /// </summary>
        /// <returns>bool Session是否正常</returns>
        private bool CheckBasis()
        {
            if (Basis == null)
            {
                Result.NotFound();
                return false;
            }

            if (!Basis.Validity)
            {
                Result.Disabled();
                return false;
            }

            // 检查Session是否正常
            if (Basis.Ckeck(_Token.id)) return true;

            Result.AccountIsBlocked();
            return false;
        }

        /// <summary>
        /// 对Secret进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool 是否通过验证</returns>
        private void Verify(string action = null)
        {
            var now = DateTime.Now;
            if (now > Basis.FailureTime.AddMinutes(10))
            {
                Result.Failured();
                return;
            }

            if (now > Basis.ExpiryTime.AddMinutes(10))
            {
                Result.Expired();
                return;
            }

            // 验证Secret
            if (!Basis.Verify(_Token.secret, 1))
            {
                Result.InvalidAuth();
                return;
            }

            // 如action为空，立即返回；否则进行鉴权
            Result.Success();
            if (string.IsNullOrEmpty(action)) return;

            Guid aid;
            if (!Guid.TryParse(action, out aid))
            {
                Result.InvalidGuid();
                return;
            }

            // 根据传入的操作码进行鉴权
            var auth = new Authority(Basis.userId, Basis.deptId);
            if (auth.Identify(aid)) return;

            Result.Forbidden();
        }

        /// <summary>
        /// 获取用于鉴别访问来源的Key
        /// </summary>
        /// <returns>string Key</returns>
        private string GetKey()
        {
            var uri = _Context.IncomingRequest.UriTemplateMatch;
            return Util.Hash(_Token.id.ToString() + uri.Data);
        }
    }
}