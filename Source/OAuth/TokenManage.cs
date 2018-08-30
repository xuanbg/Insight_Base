using System;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Redis;

namespace Insight.Base.OAuth
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class TokenManage
    {
        // Token属性是否已改变
        private bool changed;

        // 当前令牌对应的关键数据集
        private Token token;

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
            changed = true;
        }

        /// <summary>
        /// 获取缓存中的令牌数据
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void getToken(string tokenId)
        {
            token = RedisHelper.stringGet<Token>($"Token:{tokenId}");
        }

        /// <summary>
        /// 生成令牌关键数据
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="aid">应用ID</param>
        /// <param name="tid">租户ID</param>
        /// <param name="did">登录部门ID</param>
        /// <returns>TokenPackage 令牌数据包</returns>
        public TokenPackage creator(string code, string aid, string tid = null, string did = null)
        {
            if (string.IsNullOrEmpty(aid)) aid = "Default APP";

            var funs = Core.getPermitFuncs(tid, userId, did, false, aid)
                .Where(i => i.permit > 0)
                .Select(i => i.key)
                .ToList();
            token = new Token(tid, aid) {permitFuncs = funs, deptId = did, deptCode = getDeptCode(tid, did)};

            var package = initPackage(code);
            RedisHelper.stringSet($"Token:{code}", token, token.failureTime);
            RedisHelper.hashSet($"Apps:{userId}", aid, code);

            return package;
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        /// <returns>TokenPackage 令牌数据包</returns>
        public TokenPackage refresh(string tokenId)
        {
            token.refresh();
            var package = initPackage(tokenId);
            RedisHelper.stringSet($"Token:{tokenId}", token, token.failureTime);

            return package;
        }

        /// <summary>
        /// 使用户离线
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public static void delete(string tokenId)
        {
            if (!RedisHelper.hasKey($"Token:{tokenId}")) return;

            RedisHelper.delete($"Token:{tokenId}");
        }

        /// <summary>
        /// Token是否合法
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns>TokenPackage 令牌数据包</returns>
        private TokenPackage initPackage(string code)
        {
            var accessToken = new AccessToken {id = code, userId = userId, secret = token.secretKey};
            var refreshToken = new AccessToken {id = code, userId = userId, secret = token.refreshKey};
            var tokenPackage = new TokenPackage
            {
                accessToken = Util.base64(accessToken),
                refreshToken = Util.base64(refreshToken),
                expiryTime = token.life,
                failureTime = token.life * 12
            };

            token.hash = Util.hash(tokenPackage.accessToken);

            return tokenPackage;
        }

        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="tokenType">令牌类型</param>
        /// <returns>bool Token是否合法</returns>
        public bool verify(string key, TokenType tokenType)
        {
            return token != null && token.verifyKey(key, tokenType);
        }

        /// <summary>
        /// 验证操作码对应功能是否授权
        /// </summary>
        /// <param name="key">操作码</param>
        /// <returns>bool 是否授权</returns>
        public bool verifyKeyInCache(string key)
        {
            return token?.permitFuncs != null && token.permitFuncs.Any(i => i.Contains(key));
        }

        /// <summary>
        /// 验证用户是否拥有指定功能的授权
        /// </summary>
        /// <param name="key">操作码</param>
        /// <returns>bool 是否通过验证</returns>
        public bool verifyKey(string key)
        {
            var permits = Core.getPermitFuncs(token.tenantId, userId, token.deptId).Where(i => i.permit > 0);
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
        public void addFailureCount()
        {
            if (userIsLocked()) return;

            lastFailureTime = DateTime.Now;
            failureCount++;
            changed = true;
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="key">支付密码</param>
        /// <returns>bool 支付密码是否通过验证</returns>
        public bool? verifyPayPassword(string key)
        {
            if (payPassword == null) return null;

            var pw = Util.hash(userId + key);
            return payPassword == pw;
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <returns>bool Token是否过期</returns>
        public bool tenantIsExpiry()
        {
            return token == null || token.tenantId != null && DateTime.Now > token.expireDate;
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <returns>bool Token是否过期</returns>
        public bool isExpiry()
        {
            return token == null || token.isExpiry();
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        /// <param name="hash">令牌哈希值</param>
        /// <param name="type">令牌类型</param>
        /// <returns>bool Token是否失效</returns>
        public bool isFailure(string tokenId, string hash, TokenType type)
        {
            if (token == null || token.hash != hash && type == TokenType.ACCESS_TOKEN) return true;

            var code = RedisHelper.hashGet($"Apps:{userId}", token.appId);

            return token.signInOne && code != tokenId || token.isFailure();
        }

        /// <summary>
        /// 用户是否失效状态
        /// </summary>
        /// <returns>bool 用户是否失效状态</returns>
        public bool userIsLocked()
        {
            var now = DateTime.Now;
            var resetTime = lastFailureTime.AddMinutes(10);
            if (failureCount > 0 && now > resetTime)
            {
                failureCount = 0;
                changed = true;
            }

            return failureCount > 5;
        }

        /// <summary>
        /// 是否已修改
        /// </summary>
        /// <returns>bool 是否已修改</returns>
        public bool isChanged()
        {
            return changed;
        }

        /// <summary>
        /// 设置修改标志位为真值
        /// </summary>
        public void setChanged()
        {
            changed = true;
        }

        /// <summary>
        /// 获取令牌生命周期(秒)
        /// </summary>
        /// <returns>int 令牌生命周期(</returns>
        public int getLife()
        {
            return token?.life ?? 7200;
        }

        /// <summary>
        /// 获取租户ID
        /// </summary>
        /// <returns>string 租户ID</returns>
        public string getTenantId()
        {
            return token?.tenantId;
        }

        /// <summary>
        /// 获取AppID
        /// </summary>
        /// <returns>string AppID</returns>
        public string getAppId()
        {
            return token?.appId ?? "Default APP";
        }

        /// <summary>
        /// 获取用户当前登录部门ID
        /// </summary>
        /// <returns>string 用户当前登录部门ID</returns>
        public string getDeptId()
        {
            return token?.deptId;
        }

        /// <summary>
        /// 获取指定ID的部门编码
        /// </summary>
        /// <param name="tid">租户ID</param>
        /// <param name="did">部门ID</param>
        /// <returns>部门编码</returns>
        private string getDeptCode(string tid, string did)
        {
            if (string.IsNullOrEmpty(did)) return null;

            using (var context = new Entities())
            {
                return context.orgs.SingleOrDefault(i => !i.isInvalid && i.tenantId == tid && i.id == did)?.code;
            }
        }
    }
}