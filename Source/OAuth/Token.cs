using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.DTO;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Insight.Utils.Redis;

namespace Insight.Base.OAuth
{
    public class Token
    {
        // Token允许的超时毫秒数(300秒)
        private const int TIME_OUT = 300;

        /// <summary>
        /// 访问令牌MD5摘要
        /// </summary>
        public string hash { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public string tenantId { get; set; }

        /// <summary>
        /// 登录部门ID
        /// </summary>
        public string deptId { get; set; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string appId { get; set; }

        /// <summary>
        /// 令牌生命周期(秒)
        /// </summary>
        public int tokenLife { get; set; }

        /// <summary>
        /// 租户到期时间
        /// </summary>
        public DateTime expireDate { get; set; }

        /// <summary>
        /// Token过期时间
        /// </summary>
        public DateTime expiryTime { get; set; }

        /// <summary>
        /// Token失效时间
        /// </summary>
        public DateTime failureTime { get; set; }

        /// <summary>
        /// Token验证密钥
        /// </summary>
        public string secretKey { get; set; }

        /// <summary>
        /// Token刷新密钥
        /// </summary>
        public string refreshKey { get; set; }

        /// <summary>
        /// 授权操作码集合
        /// </summary>
        public List<string> permitFuncs { get; set; }

        /// <summary>
        /// 授权数据信息集合
        /// </summary>
        public List<PermitData> permitDatas { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Token()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="appId">应用ID</param>
        public Token(string tenantId, string appId)
        {
            this.tenantId = tenantId;
            this.appId = appId;

            expireDate = GetExpireDate(tenantId);
            tokenLife = GetTokenLife(appId);
            secretKey = Util.NewId("N");
            refreshKey = Util.NewId("N");
            expiryTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
            failureTime = DateTime.Now.AddSeconds(tokenLife * 12 + TIME_OUT);
        }

        /// <summary>
        /// 验证密钥
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="tokenType">令牌类型</param>
        /// <returns>是否通过验证</returns>
        public bool VerifyKey(string key, TokenType tokenType)
        {
            var passed = key == (tokenType == TokenType.AccessToken ? secretKey : refreshKey);
            if (passed && tokenType == TokenType.AccessToken && tokenLife < 3600 && DateTime.Now.AddSeconds(tokenLife / 2 + TIME_OUT) > expiryTime)
            {
                expiryTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
                failureTime = DateTime.Now.AddSeconds(tokenLife * 12 + TIME_OUT);
            }

            return passed;
        }

        /// <summary>
        /// 刷新令牌关键数据
        /// </summary>
        public void Refresh()
        {
            expiryTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
            failureTime = DateTime.Now.AddSeconds(tokenLife * 12 + TIME_OUT);
            secretKey = Util.NewId("N");
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <param name="isReal">是否实际过期时间</param>
        /// <returns>Token是否过期</returns>
        public bool IsExpiry(bool isReal = false)
        {
            var now = DateTime.Now;
            var expiry = expiryTime.AddSeconds(isReal ? -TIME_OUT : 0);
            return now > expiry;
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <returns>Token是否失效</returns>
        public bool IsFailure()
        {
            return DateTime.Now > failureTime;
        }

        /// <summary>
        /// 获取租户的过期时间
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <returns>租户的过期时间</returns>
        private static DateTime GetExpireDate(string tenantId)
        {
            using (var context = new Entities())
            {
                return context.tenants.SingleOrDefault(i => i.id == tenantId)?.expireDate ?? DateTime.MinValue;
            }
        }

        /// <summary>
        /// 查询指定ID的应用的令牌生命周期(秒)
        /// </summary>
        /// <param name="appId">应用ID</param>
        /// <returns>应用的令牌生命周期(秒)</returns>
        private static int GetTokenLife(string appId)
        {
            if (string.IsNullOrEmpty(appId)) return 7200;

            // 从缓存读取应用的令牌生命周期
            const string fiele = "TokenLife";
            var val = RedisHelper.HashGet($"App:{appId}", fiele);
            if (!string.IsNullOrEmpty(val)) return Convert.ToInt32(val);

            // 从数据库读取应用的令牌生命周期
            using (var context = new Entities())
            {
                var life = context.applications.SingleOrDefault(i => i.id == appId)?.tokenLife ?? 7200;
                RedisHelper.HashSet($"App:{appId}", fiele, life.ToString());

                return life;
            }
        }
    }
}