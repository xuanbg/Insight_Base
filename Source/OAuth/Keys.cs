using System;
using Insight.Utils.Common;

namespace Insight.Base.OAuth
{
    public class Keys
    {
        // Token允许的超时毫秒数(300秒)
        private const int TIME_OUT = 300;

        // Token过期时间
        private DateTime _expiryTime;

        // Token失效时间
        private DateTime _failureTime;

        // Token验证密钥
        public string secretKey;

        // Token刷新密钥
        public string refreshKey;

        // 令牌生命周期(秒)
        public readonly int tokenLife;

        // 应用ID
        public string appId;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Keys()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hours">令牌有效小时数</param>
        /// <param name="appId">应用ID</param>
        public Keys(int hours, string appId = null)
        {
            this.appId = appId;
            tokenLife = 3600 * hours;
            secretKey = Util.NewId("N");
            refreshKey = Util.NewId("N");
            _expiryTime = DateTime.Now.AddSeconds(tokenLife / 12 + TIME_OUT);
            _failureTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
        }

        /// <summary>
        /// 验证密钥
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="type">验证类型(1:验证AccessToken、2:验证RefreshToken)</param>
        /// <returns>是否通过验证</returns>
        public bool verifyKey(string key, int type)
        {
            if (type < 1 || type > 2) return false;

            var secret = type == 1 ? secretKey : refreshKey;
            return key == secret;
        }

        /// <summary>
        /// 刷新令牌关键数据
        /// </summary>
        public void refresh()
        {
            _expiryTime = DateTime.Now.AddSeconds(tokenLife / 12 + TIME_OUT);
            _failureTime = DateTime.Now.AddSeconds(tokenLife + TIME_OUT);
            secretKey = Util.NewId("N");
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <param name="isReal">是否实际过期时间</param>
        /// <returns>Token是否过期</returns>
        public bool isExpiry(bool isReal = false)
        {
            var now = DateTime.Now;
            var expiry = _expiryTime.AddSeconds(isReal ? -TIME_OUT : 0);
            return now.CompareTo(expiry) > 0;
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <returns>Token是否失效</returns>
        public bool isFailure()
        {
            return DateTime.Now.CompareTo(_failureTime) > 0;
        }
    }
}