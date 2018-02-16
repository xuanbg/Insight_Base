using System;
using Insight.Utils.Common;

namespace Insight.Base.OAuth
{
    public class Keys
    {
        // Token允许的超时毫秒数(300秒)
        private const int TIME_OUT = 300;

        // Token过期时间
        private DateTime expiryTime;

        // Token失效时间
        private DateTime failureTime;

        /// <summary>
        /// Token验证密钥
        /// </summary>
        public string secretKey { get; private set; }

        /// <summary>
        /// Token刷新密钥
        /// </summary>
        public string refreshKey { get;}

        /// <summary>
        /// 令牌生命周期(秒)
        /// </summary>
        public int tokenLife { get; }

        /// <summary>
        /// 应用ID
        /// </summary>
        public string appId { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tokenLife">令牌有效小时数</param>
        /// <param name="appId">应用ID</param>
        public Keys(int tokenLife, string appId = null)
        {
            this.appId = appId;
            this.tokenLife = 3600 * tokenLife;
            secretKey = Util.NewId("N");
            refreshKey = Util.NewId("N");
            expiryTime = DateTime.Now.AddSeconds(this.tokenLife / 12 + TIME_OUT);
            failureTime = DateTime.Now.AddSeconds(this.tokenLife + TIME_OUT);
        }

        /// <summary>
        /// 验证密钥
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="type">验证类型(1:验证AccessToken、2:验证RefreshToken)</param>
        /// <returns>是否通过验证</returns>
        public bool VerifyKey(string key, int type)
        {
            if (type < 1 || type > 2) return false;

            var secret = type == 1 ? secretKey : refreshKey;
            return key == secret;
        }

        /// <summary>
        /// 刷新令牌关键数据
        /// </summary>
        public void Refresh()
        {
            expiryTime = DateTime.Now.AddSeconds(tokenLife / 12 + TIME_OUT);
            failureTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
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
            return now.CompareTo(expiry) > 0;
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <returns>Token是否失效</returns>
        public bool IsFailure()
        {
            return DateTime.Now.CompareTo(failureTime) > 0;
        }
    }
}