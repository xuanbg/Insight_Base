using System;
using System.Collections.Generic;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Common.Entity
{
    /// <summary>
    /// 用户会话信息
    /// </summary>
    public class Session:AccessToken
    {
        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 登录用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 绑定的手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public bool Validity { get; set; }

        // 登录部门ID
        public Guid? DeptId;

        // Secret过期时间
        public DateTime ExpireTime;

        // Secret失效时间
        public DateTime FailureTime;

        // 用户在线状态
        public bool OnlineStatus;

        // 用户签名
        private string Signature;

        // 刷新密码
        private string RefreshKey;

        // 连续失败次数
        private int FailureCount;

        // 上次连接时间
        private DateTime LastConnect;

        /// <summary>
        /// 检验是否已经连续错误5次
        /// </summary>
        /// <param name="stamp">用户特征码</param>
        /// <returns>bool 是否已经连续错误5次</returns>
        public bool Ckeck(string stamp)
        {
            var now = DateTime.Now;
            var span = now - LastConnect;
            if (span.TotalMinutes > 15) FailureCount = 0;

            LastConnect = now;
            return FailureCount >= 5 && Stamp != stamp;
        }

        /// <summary>
        /// 验证Token合法性
        /// </summary>
        /// <param name="key"></param>
        /// <returns>bool 是否通过验证</returns>
        public bool Verify(string key)
        {
            var keys = new List<string> {Secret, RefreshKey, Signature};
            if (keys.Contains(key)) return true;

            FailureCount++;
            return false;
        }

        /// <summary>
        /// 设置Secret及过期时间
        /// </summary>
        public void InitSecret()
        {
            var now = DateTime.Now;
            Secret = Util.Hash(Guid.NewGuid() + Signature + now);
            RefreshKey = Util.Hash(Guid.NewGuid() + Secret);
            ExpireTime = now.AddHours(2);
            FailureTime = now.AddHours(Parameters.Expired);
        }

        /// <summary>
        /// 刷新Secret过期时间
        /// </summary>
        public void Refresh()
        {
            ExpireTime = ExpireTime.AddHours(2);
        }

        /// <summary>
        /// 使Session在线
        /// </summary>
        /// <param name="did">用户登录部门ID</param>
        public void Online(Guid? did)
        {
            DeptId = did;
            OnlineStatus = true;
            FailureCount = 0;
        }

        /// <summary>
        /// 注销Session
        /// </summary>
        public void SignOut()
        {
            Secret = Guid.NewGuid().ToString();
            OnlineStatus = false;
        }

        /// <summary>
        /// 生成用户签名
        /// </summary>
        /// <param name="password">用户密码</param>
        public void Sign(string password)
        {
            Signature = Util.Hash(Account.ToUpper() + password);
        }

        /// <summary>
        /// 生成特征码
        /// </summary>
        /// <param name="stamp">用户特征码（可为空）</param>
        public void MakeStamp(string stamp = null)
        {
            Stamp = stamp ?? Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成Token
        /// </summary>
        /// <returns>string 序列化为Json的Token数据</returns>
        public object CreatorKey()
        {
            var obj = new
            {
                AccessToken = Util.Base64(new {ID, Account, UserName, Stamp, Secret}),
                RefreshToken = Util.Base64(new {ID, Account, UserName, Stamp, Secret = RefreshKey}),
                ExpireTime,
                FailureTime
            };
            return obj;
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
