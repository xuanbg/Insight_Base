using System;
using System.Linq;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Redis;
using Newtonsoft.Json;

namespace Insight.Base.OAuth
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class TokenManage
    {
        // 进程同步基元
        private static readonly Mutex mutex = new Mutex();

        // Token属性是否已改变
        private bool isChanged;

        // 当前令牌对应的关键数据集
        private Token token;

        /// <summary>
        /// 令牌生命周期(秒)
        /// </summary>
        [JsonIgnore]
        public int life => token?.life ?? 7200;

        /// <summary>
        /// 租户ID
        /// </summary>
        [JsonIgnore]
        public string tenantId => token?.tenantId;

        /// <summary>
        /// 应用ID
        /// </summary>
        [JsonIgnore]
        public string appId => token?.appId;

        /// <summary>
        /// 登录部门ID
        /// </summary>
        [JsonIgnore]
        public string deptId => token?.deptId;

        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string mobile { get; set; }

        /// <summary>
        /// 用户E-mail
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string password { get; set; }

        /// <summary>
        /// 支付密码
        /// </summary>
        public string payPassword { get; set; }

        /// <summary>
        /// 用户是否内置
        /// </summary>
        public bool isBuiltIn { get; set; }

        /// <summary>
        /// 用户是否失效
        /// </summary>
        public bool isInvalid { get; set; }

        /// <summary>
        /// 上次验证失败时间
        /// </summary>
        public DateTime lastFailureTime { get; set; }

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int failureCount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TokenManage()
        {
        }

        /// <summary>
        /// 构造函数，根据UserID构建对象
        /// </summary>
        /// <param name="user">用户实体</param>
        public TokenManage(User user)
        {
            userId = user.id;
            userName = user.name;
            account = user.account;
            mobile = user.mobile;
            email = user.email;
            password = user.password;
            payPassword = user.payPassword;
            isBuiltIn = user.isBuiltin;
            isInvalid = user.isInvalid;

            lastFailureTime = DateTime.Now;
            failureCount = 0;
            isChanged = true;
        }

        /// <summary>
        /// 获取缓存中的令牌数据
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        /// <returns>bool 是否存在关键数据集</returns>
        public bool GetToken(string tokenId)
        {
            token = RedisHelper.StringGet<Token>($"Token:{tokenId}");

            return token != null;
        }

        /// <summary>
        /// 生成令牌关键数据
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="aid">应用ID</param>
        /// <param name="tid">租户ID</param>
        /// <returns>令牌数据包</returns>
        public TokenPackage Creator(string code, string aid, string tid = null)
        {
            var funs = Core.GetPermitFuncs(tid, userId, deptId, false, aid)
                .Where(i => i.permit > 0)
                .Select(i => i.key)
                .ToList();
            token = new Token(tid, aid) {permitFuncs = funs};

            var package = InitPackage(code);
            RedisHelper.StringSet($"Token:{code}", token, token.failureTime);
            RedisHelper.HashSet($"Apps:{userId}", aid, code);

            return package;
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        /// <returns>令牌数据包</returns>
        public TokenPackage Refresh(string tokenId)
        {
            token.Refresh();
            var package = InitPackage(tokenId);
            RedisHelper.StringSet($"Token:{tokenId}", token, token.failureTime);

            return package;
        }

        /// <summary>
        /// 使用户离线
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void Delete(string tokenId)
        {
            if (!RedisHelper.HasKey($"Token:{tokenId}")) return;

            RedisHelper.Delete($"Token:{tokenId}");
        }

        /// <summary>
        /// Token是否合法
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns>令牌数据包</returns>
        private TokenPackage InitPackage(string code)
        {
            var accessToken = new AccessToken {id = code, userId = userId, secret = token.secretKey};
            var refreshToken = new AccessToken {id = code, userId = userId, secret = token.refreshKey};
            var tokenPackage = new TokenPackage
            {
                accessToken = Util.Base64(accessToken),
                refreshToken = Util.Base64(refreshToken),
                expiryTime = token.life,
                failureTime = token.life * 12
            };

            token.hash = Util.Hash(tokenPackage.accessToken);

            return tokenPackage;
        }

        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="hash">令牌哈希值</param>
        /// <param name="key">密钥</param>
        /// <param name="tokenType">令牌类型</param>
        /// <returns>Token是否合法</returns>
        public bool Verify(string hash, string key, TokenType tokenType)
        {
            if (token == null) return false;

            return (tokenType == TokenType.RefreshToken || token.hash == hash) && token.VerifyKey(key, tokenType);
        }

        /// <summary>
        /// 验证操作码对应功能是否授权
        /// </summary>
        /// <param name="key">操作码</param>
        /// <returns>bool 是否授权</returns>
        public bool VerifyKeyInCache(string key)
        {
            return token?.permitFuncs != null && token.permitFuncs.Any(i => i.Contains(key));
        }

        /// <summary>
        /// 验证用户是否拥有指定功能的授权
        /// </summary>
        /// <param name="key">操作码</param>
        /// <returns>bool 是否通过验证</returns>
        public bool VerifyKey(string key)
        {
            var permits = Core.GetPermitFuncs(tenantId, userId, deptId).Where(i => i.permit > 0);
            using (var context = new Entities())
            {
                var list = from f in context.functions.ToList()
                    join p in permits on f.id equals p.id
                    select f.alias;

                return list.Any(i => i.Contains(key));
            }
        }

        /// <summary>
        /// 累计失败次数(有效时)
        /// </summary>
        public void AddFailureCount()
        {
            if (UserIsLocked()) return;

            lastFailureTime = DateTime.Now;
            failureCount++;
            isChanged = true;
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="key">支付密码</param>
        /// <returns>支付密码是否通过验证</returns>
        public bool? VerifyPayPassword(string key)
        {
            if (payPassword == null) return null;

            var pw = Util.Hash(userId + key);
            return payPassword == pw;
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <returns>Token是否过期</returns>
        public bool TenantIsExpiry()
        {
            return token == null || tenantId != null && DateTime.Now > token.expireDate;
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <returns>Token是否过期</returns>
        public bool IsExpiry()
        {
            return token == null || token.IsExpiry();
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <param name="tokenId"></param>
        /// <returns>Token是否失效</returns>
        public bool IsFailure(string tokenId)
        {
            if (token == null) return true;

            var code = RedisHelper.HashGet(userId, appId);

            return token.signInOne && code != tokenId || token.IsFailure();
        }

        /// <summary>
        /// 用户是否失效状态
        /// </summary>
        /// <returns>用户是否失效状态</returns>
        public bool UserIsLocked()
        {
            var now = DateTime.Now;
            var resetTime = lastFailureTime.AddMinutes(10);
            if (failureCount > 0 && now > resetTime)
            {
                failureCount = 0;
                isChanged = true;
            }

            return failureCount > 5;
        }

        /// <summary>
        /// 是否已修改
        /// </summary>
        /// <returns></returns>
        public bool IsChanged()
        {
            return isChanged;
        }

        /// <summary>
        /// 设置修改标志位为真值
        /// </summary>
        public void SetChanged()
        {
            isChanged = true;
        }
    }
}