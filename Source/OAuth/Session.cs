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
    public class Session
    {
        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        // 当前令牌对应的关键数据集
        private Keys _currentKeys;

        // Token属性是否已改变
        public bool isChanged;

        // 登录部门ID
        public string deptId;

        // 用户ID
        public string userId;

        // 用户名
        public string userName;

        // 登录账号
        private string _account;

        // 用户手机号
        private string _mobile;

        // 用户E-mail
        private string _email;

        // 登录密码
        private string _password;

        // 支付密码
        private readonly string _payPassword;

        // 用户是否内置
        private bool _isBuiltIn;

        // 用户是否失效
        private readonly bool _isInvalid;

        // 上次验证失败时间
        private DateTime _lastFailureTime;

        // 连续失败次数
        private int _failureCount;

        // 使用中的令牌关键数据集
        private readonly Dictionary<string, Keys> _keyMap;

        /// <summary>
        /// 构造方法，根据UserID构建对象
        /// </summary>
        /// <param name="user">用户实体</param>
        public Session(User user)
        {
            userId = user.id;
            userName = user.name;
            _account = user.account;
            _mobile = user.mobile;
            _email = user.email;
            _password = user.password;
            _payPassword = user.payPassword;
            _isBuiltIn = user.isBuiltin;
            _isInvalid = user.isInvalid;

            _lastFailureTime = DateTime.Now;
            _failureCount = 0;
            _keyMap = new Dictionary<string, Keys>();
            isChanged = true;
        }

        /// <summary>
        /// 选择当前令牌对应的关键数据集
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void SelectKeys(string tokenId)
        {
            _currentKeys = _keyMap[tokenId];
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
            foreach (var item in _keyMap)
            {
                // 如应用ID不为空,且应用ID有对应的Key则从Map中删除该应用对应的Key.否则使用无应用ID的公共Key.
                if (!string.IsNullOrEmpty(appId))
                {
                    if (appId == item.Value.appId)
                    {
                        _keyMap.Remove(item.Key);
                        _currentKeys = null;
                        break;
                    }
                }
                else
                {
                    if (item.Value.appId == null)
                    {
                        item.Value.refresh();

                        _currentKeys = item.Value;
                        code = item.Key;
                        break;
                    }
                }
            }

            // 生成新的Key加入Map
            if (_currentKeys == null)
            {
                _currentKeys = new Keys(hours, appId);
                _keyMap.Add(code, _currentKeys);
            }

            if (_failureCount > 0) _failureCount = 0;

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
            _currentKeys.refresh();
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
                secret = _currentKeys.secretKey
            };

            var refreshToken = new RefreshToken
            {
                id = code,
                secret = _currentKeys.refreshKey
            };

            var tokenPackage = new TokenPackage
            {
                accessToken = Util.Base64(accessToken),
                refreshToken = Util.Base64(refreshToken),
                expiryTime = _currentKeys.tokenLife / 12,
                failureTime = _currentKeys.tokenLife
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
            if (_currentKeys != null && _currentKeys.verifyKey(key, type)) return true;

            AddFailureCount();
            return false;
        }

        /// <summary>
        /// 累计失败次数(有效时)
        /// </summary>
        public void AddFailureCount()
        {
            if (UserIsInvalid()) return;

            _failureCount++;
            _lastFailureTime = DateTime.Now;
            isChanged = true;
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="key">支付密码</param>
        /// <returns>支付密码是否通过验证</returns>
        public bool? VerifyPayPassword(string key)
        {
            if (_payPassword == null) return null;

            var pw = Util.Hash(userId + key);
            return _payPassword == pw;
        }

        /// <summary>
        /// Token是否过期
        /// </summary>
        /// <returns>Token是否过期</returns>
        public bool IsExpiry()
        {
            return _currentKeys == null || _currentKeys.isExpiry();
        }

        /// <summary>
        /// Token是否失效
        /// </summary>
        /// <returns>Token是否失效</returns>
        public bool IsFailure()
        {
            return _currentKeys == null || _currentKeys.isFailure();
        }

        /// <summary>
        /// 用户是否失效状态
        /// </summary>
        /// <returns>用户是否失效状态</returns>
        public bool UserIsInvalid()
        {
            var now = DateTime.Now;
            var resetTime = _lastFailureTime.AddMinutes(10);
            if (_failureCount > 0 && now > resetTime)
            {
                _failureCount = 0;
                isChanged = true;
            }

            return _failureCount > 5 || _isInvalid;
        }

        /// <summary>
        /// 使用户离线
        /// </summary>
        /// <param name="tokenId">令牌ID</param>
        public void DeleteKeys(string tokenId)
        {
            if (!_keyMap.ContainsKey(tokenId)) return;

            _keyMap.Remove(tokenId);
            isChanged = true;
        }
    }
}