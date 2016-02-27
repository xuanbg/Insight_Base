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
        public Session Basis;

        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        public readonly Session Session;

        /// <summary>
        /// JsonResult类型的验证结果
        /// </summary>
        public JsonResult Result = new JsonResult().InvalidAuth();

        /// <summary>
        /// Session.ID是否一致
        /// </summary>
        private readonly bool Identical;

        /// <summary>
        /// 最大授权数
        /// </summary>
        private const int MaxAuth = 999999999;

        /// <summary>
        /// 构造方法，使用Session验证
        /// </summary>
        public SessionVerify()
        {
            // 从请求头获取验证数据
            var dict = General.GetAuthorization();
            Session = General.GetAuthor<Session>(dict["Auth"]);

            // 验证数据不存在
            if (Session == null) return;

            Basis = SessionManage.GetSession(Session);
            Identical = Basis?.ID == Session.ID;
        }

        /// <summary>
        /// 用户登录专用验证方法
        /// </summary>
        /// <returns>bool</returns>
        public void SignIn()
        {
            if (!Compare(null, false)) return;

            Basis.OpenId = Session.OpenId;
            Basis.DeptId = Session.DeptId;
            Basis.DeptName = Session.DeptName;
            Basis.MachineId = Session.MachineId;
        }

        /// <summary>
        /// 用户注册专用验证方法
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool</returns>
        public bool SignUp(string action = null)
        {
            return Basis == null || Compare(action);
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
            if (Session.LoginName == account) action = null;

            return Compare(action);
        }

        /// <summary>
        /// 对Session进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <param name="verify">是否验证模式</param>
        /// <returns>bool</returns>
        public bool Compare(string action = null, bool verify = true)
        {
            Result.InvalidAuth();
            if (Basis == null || !ExtraCheck(verify)) return false;

            if (Basis.ID > MaxAuth) return false;

            if (!Basis.Validity)
            {
                Result.Disabled();
                return false;
            }

            // 验证签名失败计数清零（距上次用户签名验证时间超过1小时）
            if (Basis.FailureCount > 0 && (DateTime.Now - Basis.LastConnect).TotalHours > 1)
            {
                Basis.FailureCount = 0;
            }
            Basis.LastConnect = DateTime.Now;

            // 签名不正确或验证签名失败超过5次（冒用时）则不能通过验证，且连续失败计数累加1次
            if (Basis.Signature != Session.Signature || (Basis.FailureCount >= 5 && Basis.MachineId != Session.MachineId))
            {
                Basis.FailureCount++;
                return false;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            if (Basis.MachineId != Session.MachineId) Result.Multiple();

            // 如Session.ID不一致，返回Session过期的信息
            if (Identical) Result.Success();
            else Result.Expired(Util.CreateKey(Basis));

            if (action == null) return true;

            // 开始鉴权
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
        /// 扩展验证
        /// </summary>
        /// <param name="verify">是否进行扩展验证</param>
        /// <returns>bool 验证是否通过</returns>
        private bool ExtraCheck(bool verify)
        {
            if (!verify) return true;

            if (Util.CheckMachineId && Basis.MachineId != Session.MachineId) return false;

            if (Util.CheckOpenID && Basis.OpenId != Session.OpenId) return false;

            return Basis.UserId == Session.UserId;
        }
    }
}
