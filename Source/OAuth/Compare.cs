using System;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using Insight.Base.Common;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    public class Compare
    {
        private readonly WebOperationContext _Context = WebOperationContext.Current;
        private Session _Basis;

        // 用户传入Token
        public AccessToken Token;

        /// <summary>
        /// 验证结果
        /// </summary>
        public Result<object> Result { get; } = new Result<object>().Success();

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public Session Basis => _Basis ?? (_Basis = Core.GetSession(Token.userId));

        /// <summary>
        /// 构造方法，可选流控
        /// </summary>
        /// <param name="limit">单位时间(秒)内限制调用，0：不限制</param>
        public Compare(int limit = 0)
        {
            var headers = _Context.IncomingRequest.Headers;
            var auth = headers[HttpRequestHeader.Authorization];
            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                Token = Util.Deserialize<AccessToken>(json);

                if (Basis == null)
                {
                    Result.Failured();
                    return;
                }

                // 检查Session是否正常
                if (!Basis.UserIsInvalid())
                {
                    Result.Disabled();
                    return;
                }

                if (limit == 0) return;

                var uri = _Context.IncomingRequest.UriTemplateMatch;
                var key = Util.Hash(Token.id + uri.Data);
                var time = Params.CallManage.LimitCall(key, limit);
                if (time == 0) return;

                Result.TooFrequent(time.ToString());
            }
            catch (Exception ex)
            {
                var msg = $"提取验证信息失败。Token is:{auth ?? "null"}\r\nException:{ex}";
                new Thread(() => new Logger("500101", msg).Write()).Start();

                Result.InvalidAuth();
            }
        }

        /// <summary>
        /// 对Secret进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="type">类型：1、验证Secret；2、验证RefreshKey。默认为0</param>
        /// <returns>Result</returns>
        public Result<object> Verify(string action = null, int type = 1)
        {
            if (Basis.IsFailure()) return Result.Failured();

            if (type == 0 && Basis.IsExpiry()) return Result.Expired();

            // 验证Secret
            if (!Basis.VerifyToken(Token.secret, type)) return Result.InvalidAuth();

            // 如action为空，立即返回；否则进行鉴权
            if (string.IsNullOrEmpty(action)) return Result.Success();

            // 根据传入的操作码进行鉴权
            var auth = new Authority(Basis.userId, Basis.deptId);
            return auth.Identify(action) ? Result.Success() : Result.Forbidden();
        }
    }
}