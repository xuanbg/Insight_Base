using System;
using System.Linq;
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
        // 用户签名
        private string _Signature;

        // 刷新密码
        private string _RefreshKey;

        // 连续失败次数
        private int _FailureCount;

        // 上次连接时间
        private DateTime _LastConnect;

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
        /// 随机码
        /// </summary>
        public string Stamp { get; private set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 绑定的手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public bool Validity { get; set; }

        /// <summary>
        /// 构造方法，根据用户账号和索引构建对象
        /// </summary>
        /// <param name="account">用户账号</param>
        public Session(string account)
        {
            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(s => s.LoginName == account);
                if (user == null) return;

                UserType = user.Type;
                Account = user.LoginName;
                Mobile = user.Mobile;
                UserId = user.ID;
                UserName = user.Name;
                Validity = user.Validity;
                Stamp = Guid.NewGuid().ToString("N");
                _Signature = user.Password;
            }
        }

        /// <summary>
        /// 检验是否已经连续错误5次
        /// </summary>
        /// <returns>bool 是否已经连续错误5次</returns>
        public bool Ckeck()
        {
            var now = DateTime.Now;
            var span = now - _LastConnect;
            if (span.TotalMinutes > 15) _FailureCount = 0;

            _LastConnect = now;
            return _FailureCount >= 5;
        }

        /// <summary>
        /// 验证Token合法性
        /// </summary>
        /// <param name="key">密钥</param>
        /// <param name="type">类型：1、验证Secret；2、验证RefreshKey；3、验证Signature</param>
        /// <returns>bool 是否通过验证</returns>
        public bool Verify(string key, int type)
        {
            var str = "";
            switch (type)
            {
                case 1:
                    str = Secret;
                    break;
                case 2:
                    str = _RefreshKey;
                    break;
                case 3:
                    str = Util.Hash(_Signature + Stamp);
                    Stamp = Guid.NewGuid().ToString("N");
                    break;
            }
            if (str == key) return true;

            _FailureCount++;
            return false;
        }

        /// <summary>
        /// 设置Secret及过期时间
        /// </summary>
        public void InitSecret()
        {
            var now = DateTime.Now;
            Secret = Util.Hash(Guid.NewGuid() + _Signature + now);
            _RefreshKey = Util.Hash(Guid.NewGuid() + Secret);
            ExpiryTime = now.AddHours(2);
            FailureTime = now.AddHours(Common.Expired);
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        public void Refresh()
        {
            ExpiryTime = DateTime.Now.AddHours(2);
        }

        /// <summary>
        /// 使Session在线
        /// </summary>
        /// <param name="did">用户登录部门ID</param>
        public void Online(Guid? did)
        {
            DeptId = did;
            OnlineStatus = true;
            _FailureCount = 0;
        }

        /// <summary>
        /// 注销Session
        /// </summary>
        public void SignOut()
        {
            ExpiryTime = DateTime.Now;
            FailureTime = DateTime.Now;
            OnlineStatus = false;
        }

        /// <summary>
        /// 生成用户签名
        /// </summary>
        /// <param name="password">用户密码</param>
        public void Sign(string password)
        {
            _Signature = Util.Hash(Account.ToUpper() + password);
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <returns>string 序列化为Json的Token数据</returns>
        public object CreatorKey()
        {
            return new
            {
                AccessToken = Util.Base64(new {UserId, DeptId, Account, UserName, Secret}),
                RefreshToken = Util.Base64(new {Account, Secret = _RefreshKey}),
                ExpiryTime,
                FailureTime
            };
        }

        /// <summary>
        /// 根据Account判断用户是否相同
        /// </summary>
        /// <param name="account">用户账号</param>
        /// <returns>bool 用户是否相同</returns>
        public bool UserIsSame(string account)
        {
            return Util.StringCompare(Account, account);
        }
    }
}