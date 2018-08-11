using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.OAuth
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class Session:AccessToken
    {
        // 进程同步基元
        private static readonly Mutex _Mutex = new Mutex();

        // 验证记录
        private readonly List<Code> _Codes = new List<Code>();

        // 上次连接时间
        private DateTime _LastConnectTime;

        // 连续失败次数
        private int _FailureCount;

        // 用户签名
        private string _Signature;

        // 支付密码
        private string _PayPassword;

        // 刷新密码
        private string _RefreshKey;

        /// <summary>
        /// Secret过期时间
        /// </summary>
        public DateTime ExpiryTime { get; private set; }

        /// <summary>
        /// Secret失效时间
        /// </summary>
        public DateTime FailureTime { get; private set; }

        /// <summary>
        /// 用户在线状态
        /// </summary>
        public bool OnlineStatus { get; private set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 绑定的手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户状态，是否被禁止登录
        /// </summary>
        public bool Validity { get; set; }

        /// <summary>
        /// 构造方法，根据UserID构建对象
        /// </summary>
        /// <param name="user">用户实体</param>
        public Session(SYS_User user)
        {
            _Signature = Util.Decrypt(Params.RSAKey, user.Password);
            _PayPassword = user.PayPassword;

            UserType = user.Type;
            account = user.LoginName;
            Mobile = user.Mobile;
            userId = user.ID;
            userName = user.Name;
            Validity = user.Validity;
        }

        /// <summary>
        /// 生成Code
        /// </summary>
        /// <returns>string 一个GUID字符串</returns>
        public string GenerateCode()
        {
            var now = DateTime.Now;
            _Codes.RemoveAll(c => now > c.ExpiryTime);
            if (_Codes.Count > 9) return null;

            var code = new Code {Id = Guid.NewGuid(), ExpiryTime = now.AddMinutes(30)};
            if (UserType != 0) _Codes.Add(code);

            return code.ToString();
        }

        /// <summary>
        /// 检查Session是否正常(未封禁、未锁定)
        /// </summary>
        /// <returns>bool 账户是否正常</returns>
        public bool IsValidity()
        {
            var now = DateTime.Now;
            var span = now - _LastConnectTime;
            if (span.TotalMinutes > 15) _FailureCount = 0;

            _LastConnectTime = now;
            return Validity && (UserType == 0 || _FailureCount < 5);
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <param name="tid">TokenID</param>
        /// <param name="did">登录部门ID，可为空</param>
        /// <returns>string 序列化为Json的Token数据</returns>
        public TokenResult CreatorKey(string tid, Guid? did = null)
        {
            _FailureCount = 0;
            deptId = did;
            InitSecret();
            Refresh();

            var access = new {id = tid, userId, deptId, account, userName, secret};
            var refresh = new {id = tid, account, Secret = _RefreshKey};
            var token = new TokenResult
            {
                accessToken = Util.Base64(access),
                refreshToken = Util.Base64(refresh),
                expiryTime = ExpiryTime,
                failureTime = FailureTime
            };

            return token;
        }

        /// <summary>
        /// 累计失败次数
        /// </summary>
        public void AddFailureCount()
        {
            _FailureCount++;
        }

        /// <summary>
        /// 设置Secret及过期时间
        /// </summary>
        /// <param name="force">是否强制</param>
        public void InitSecret(bool force = false)
        {
            var now = DateTime.Now;
            if (!force && now < FailureTime) return;

            _Mutex.WaitOne();
            if (now < FailureTime)
            {
                _Mutex.ReleaseMutex();
                return;
            }

            secret = Util.Hash(Guid.NewGuid() + _Signature + now);
            _RefreshKey = Util.Hash(Guid.NewGuid() + secret);
            ExpiryTime = now.AddHours(2);
            FailureTime = now.AddHours(Core.Expired);
            _Mutex.ReleaseMutex();
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        public void Refresh()
        {
            var now = DateTime.Now;
            if (now < ExpiryTime) return;

            ExpiryTime = now.AddHours(2);
        }

        /// <summary>
        /// 验证Token合法性
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="type">类型：1、验证Secret；2、验证RefreshKey</param>
        /// <returns>bool 是否通过验证</returns>
        public bool Verify(string key, int type)
        {
            if (type == 1 && secret == key || type == 2 && _RefreshKey == key) return true;

            AddFailureCount();
            return false;
        }

        /// <summary>
        /// 验证支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns>bool 是否通过验证</returns>
        public bool? Verify(string password)
        {
            if (_PayPassword == null) return null;

            return Util.Hash(userId + password) == _PayPassword;
        }

        /// <summary>
        /// 使Session在线
        /// </summary>
        /// <param name="tokenId">TokenID</param>
        /// <param name="did">用户登录部门ID</param>
        public void Online(Guid tokenId, Guid? did)
        {
            deptId = did;
            OnlineStatus = true;
        }

        /// <summary>
        /// 注销Session
        /// </summary>
        /// <param name="tid">TokenID</param>
        public void Offline(Guid tid)
        {
            var now = DateTime.Now;
            ExpiryTime = now;
            FailureTime = now;
            secret = Guid.NewGuid().ToString();
            OnlineStatus = false;
        }

        /// <summary>
        /// 生成用户签名
        /// </summary>
        /// <param name="password">用户密码</param>
        public void Sign(string password)
        {
            _Signature = Util.Decrypt(Params.RSAKey, password);
        }

        /// <summary>
        /// 获取账号签名
        /// </summary>
        /// <param name="key">登录账号</param>
        /// <param name="code">获取到的Code</param>
        /// <returns>string 签名字符串</returns>
        public string GetSign(string key, string code)
        {
            var sign = Util.Hash(key.ToUpper() + _Signature);
            return Util.Hash(sign + code);
        }

        /// <summary>
        /// 设置支付密码
        /// </summary>
        /// <param name="password">支付密码</param>
        /// <returns></returns>
        public bool SetPayPW(string password)
        {
            var pw = Util.Hash(userId + password);
            if (pw == _PayPassword) return true;

            _PayPassword = pw;
            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(s => s.ID == userId);
                if (user == null) return false;

                user.PayPassword = _PayPassword;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 根据Account判断用户是否相同
        /// </summary>
        /// <param name="loginName">用户账号</param>
        /// <returns>bool 用户是否相同</returns>
        public bool UserIsSame(string loginName)
        {
            return Util.StringCompare(account, loginName);
        }

        /// <summary>
        /// Token记录
        /// </summary>
        private class Code
        {
            /// <summary>
            /// TokenID
            /// </summary>
            public Guid Id { private get; set; }

            /// <summary>
            /// 失效时间
            /// </summary>
            public DateTime ExpiryTime { get; set; }

            /// <summary>
            /// 返回此实例的ID的字符串表示形式
            /// </summary>
            /// <returns>string 实例的ID的字符串表示形式</returns>
            public override string ToString()
            {
                return Id.ToString();
            }
        }
    }
}