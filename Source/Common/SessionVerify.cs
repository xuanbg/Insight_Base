using System;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Common
{
    public class SessionVerify
    {
        /// <summary>
        /// Guid转换结果
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public readonly Session Basis;

        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        public readonly Session Session;

        /// <summary>
        /// JsonResult类型的验证结果
        /// </summary>
        public JsonResult Result;

        /// <summary>
        /// 最大授权数
        /// </summary>
        private const int MaxAuth = 999999999;

        /// <summary>
        /// 构造方法，使用Session验证
        /// </summary>
        public SessionVerify()
        {
            // 从请求头获取验证数据对象
            var auth = General.GetAuthorization();
            Session = General.GetAuthor<Session>(auth);
            Result = new JsonResult();

            // 验证数据对象不存在
            if (Session == null) return;

            // 获取验证基准数据对象
            Basis = SessionManage.GetSession(Session);
        }

        /// <summary>
        /// 对Session和支付密码进行校验，返回验证结果
        /// </summary>
        /// <param name="key">支付密码（MD5值）</param>
        /// <returns>bool</returns>
        public bool Confirm(string key)
        {
            if (!Compare()) return false;

            if (DataAccess.ConfirmPayKey(Basis.UserId, key)) return true;

            Result.InvalidPayKey();
            return false;
        }

        /// <summary>
        /// 转换一个Guid，并对Session进行校验
        /// </summary>
        /// <param name="id">要转换的Guid字符串</param>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool</returns>
        public bool ParseIdAndCompare(string id, string action = null)
        {
            if (Guid.TryParse(id, out Guid)) return Compare(action);

            Result.InvalidGuid();
            return false;
        }

        /// <summary>
        /// 转换一个用户ID，并对Session进行校验（如id一致，忽略鉴权），返回验证结果，
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="id">用户ID</param>
        /// <returns>bool</returns>
        public bool CompareAsID(string action, string id)
        {
            if (Guid.TryParse(id, out Guid))
            {
                // 如指定的用户ID是操作人ID，则不进行鉴权
                if (Session.UserId == Guid) action = null;

                return Compare(action);
            }

            Result.InvalidGuid();
            return false;
        }

        /// <summary>
        /// 对Session进行校验（如account一致，忽略鉴权），返回验证结果
        /// </summary>
        /// <param name="action">操作码</param>
        /// <param name="account">登录账号</param>
        /// <returns>bool</returns>
        public bool Compare(string action, string account)
        {
            // 如指定的登录账号是操作人的登录账号，则不进行鉴权
            if (Util.StringCompare(Session.LoginName, account)) action = null;

            return Compare(action);
        }

        /// <summary>
        /// 对Session进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="login">是否登录验证</param>
        /// <returns>bool 是否通过验证</returns>
        public bool Compare(string action = null, bool login = false)
        {
            Result.InvalidAuth();
            if (Basis == null || Basis.ID > MaxAuth) return false;

            if (!Basis.Validity)
            {
                Result.Disabled();
                return false;
            }

            // 验证签名失败计数清零（距上次用户签名验证时间超过15分钟）
            if (Basis.FailureCount > 0 && (DateTime.Now - Basis.LastConnect).TotalMinutes > 15)
            {
                Basis.FailureCount = 0;
            }
            Basis.LastConnect = DateTime.Now;

            // 签名不正确或验证签名失败超过5次（冒用时）则不能通过验证，且连续失败计数累加1次
            if (Basis.Signature != Session.Signature || (Basis.FailureCount >= 5 && Basis.MachineId != Session.MachineId))
            {
                Basis.FailureCount++;
                if (Basis.FailureCount >= 5) Result.AccountIsBlocked();

                return false;
            }

            // 检查验证信息是否过期
            if (!login && ExpiredCheck())
            {
                var check = Util.CheckMachineId || Util.CheckOpenID;
                var key = check ? null : Util.CreateKey(Basis);
                Result.Expired(key);
                return false;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            Result.Success();

            // 如配置为不验证设备ID，设备ID不一致时返回多点登录信息
            if (Basis.MachineId != Session.MachineId) Result.Multiple();

            // 如action为空，立即返回；否则进行鉴权
            if (action == null) return true;

            Guid aid;
            if (!Guid.TryParse(action, out aid))
            {
                Result.InvalidGuid();
                return false;
            }

            // 根据传入的操作码进行鉴权
            if (DataAccess.Authority(Session, aid)) return true;

            Result.Forbidden();
            return false;
        }

        /// <summary>
        /// 检查验证信息是否过期
        /// </summary>
        /// <returns>bool 信息是否过期</returns>
        private bool ExpiredCheck()
        {
            // Session.ID失效（服务重启）或超过设定时间过期（长期未操作）
            if (Basis.ID != Session.ID || Basis.Expired < DateTime.Now) return true;

            // 设备码不同造成过期（用户在另一台设备登录）
            if (Util.CheckMachineId && Basis.MachineId != Session.MachineId) return true;

            // OpenID不同造成过期（用户使用另一个微信账号登录）
            if (Util.CheckOpenID && Basis.OpenId != Session.OpenId) return true;

            // 在过期前3天通过验证，过期时间自动延期
            var time = Basis.Expired.AddDays(-3);
            if (time < DateTime.Now) Basis.Expired = DateTime.Now.AddHours(Util.Expired);

            return false;
        }
    }
}
