using System;
using System.Collections.Generic;
using System.Threading;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class Token
    {
        // 进程同步基元
        private static readonly Mutex mutex = new Mutex();

        // 当前令牌对应的关键数据集
        private Keys currentKeys;

        // Token属性是否已改变
        private bool isChanged;

        /// <summary>
        /// 登录部门ID
        /// </summary>
        public string deptId { get; set; }

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
        /// 使用中的令牌关键数据集
        /// </summary>
        public Dictionary<string, Keys> keyMap { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Token()
        {
        }

        /// <summary>
        /// 构造函数，根据UserID构建对象
        /// </summary>
        /// <param name="user">用户实体</param>
        public Token(User user)
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
            keyMap = new Dictionary<string, Keys>();
            isChanged = true;
        }

        /// <summary>
        /// 选择当前令牌对应的关键数据集
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void SelectKeys(string tokenId)
        {
            currentKeys = keyMap[tokenId];
        }

        /// <summary>
        /// 生成令牌关键数据
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="hours">令牌有效小时数</param>
        /// <param name="appId">应用ID</param>
        /// <returns>令牌数据包</returns>
        public TokenPackage CreatorKey(string code, int hours = 24, string appId = null)
        {
            foreach (var item in keyMap)
            {
                // 如应用ID不为空,且应用ID有对应的Key则从Map中删除该应用对应的Key.否则使用无应用ID的公共Key.
                if (!string.IsNullOrEmpty(appId))
                {
                    if (appId == item.Value.appId)
                    {
                        keyMap.Remove(item.Key);
                        currentKeys = null;
                        break;
                    }
                }
                else
                {
                    if (item.Value.appId == null)
                    {
                        item.Value.Refresh();

                        currentKeys = item.Value;
                        code = item.Key;
                        break;
                    }
                }
            }

            // 生成新的Key加入Map
            if (currentKeys == null)
            {
                currentKeys = new Keys(hours, appId);
                keyMap.Add(code, currentKeys);
            }

            if (failureCount > 0) failureCount = 0;

            isChanged = true;

            return InitPackage(code);
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        /// <returns>令牌数据包</returns>
        public TokenPackage RefreshToken(string tokenId)
        {
            currentKeys.Refresh();
            isChanged = true;

            return InitPackage(tokenId);
        }

        /// <summary>
        /// Token是否合法
        /// </summary>
        /// <param name="code">Code</param>
        /// <returns>令牌数据包</returns>
        private TokenPackage InitPackage(string code)
        {
            var accessToken = new AccessToken
            {
                id = code,
                userId = userId,
                userName = userName,
                secret = currentKeys.secretKey
            };

            var refreshToken = new RefreshToken
            {
                id = code,
                userId = userId,
                secret = currentKeys.refreshKey
            };

            var tokenPackage = new TokenPackage
            {
                accessToken = Util.Base64(accessToken),
                refreshToken = Util.Base64(refreshToken),
                expiryTime = currentKeys.tokenLife / 12,
                failureTime = currentKeys.tokenLife
            };

            return tokenPackage;
        }

        /// <summary>
        /// 验证Token是否合法
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="type">验证类型(1:验证AccessToken、2:验证RefreshToken)</param>
        /// <returns>Token是否合法</returns>
        public bool VerifyToken(string key, int type)
        {
            if (currentKeys != null && currentKeys.VerifyKey(key, type)) return true;

            AddFailureCount();
            return false;
        }

        /// <summary>
        /// 累计失败次数(有效时)
        /// </summary>
        public void AddFailureCount()
        {
            if (UserIsInvalid()) return;

            failureCount++;
            lastFailureTime = DateTime.Now;
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
        public bool IsExpiry()
        {
            return currentKeys == null || currentKeys.IsExpiry();
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <returns>Token是否失效</returns>
        public bool IsFailure()
        {
            return currentKeys == null || currentKeys.IsFailure();
        }

        /// <summary>
        /// 用户是否失效状态
        /// </summary>
        /// <returns>用户是否失效状态</returns>
        public bool UserIsInvalid()
        {
            var now = DateTime.Now;
            var resetTime = lastFailureTime.AddMinutes(10);
            if (failureCount > 0 && now > resetTime)
            {
                failureCount = 0;
                isChanged = true;
            }

            return failureCount > 5 || isInvalid;
        }

        /// <summary>
        /// 使用户离线
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void DeleteKeys(string tokenId)
        {
            if (!keyMap.ContainsKey(tokenId)) return;

            keyMap.Remove(tokenId);
            isChanged = true;
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
        public void setChanged()
        {
            isChanged = true;
        }
    }
}