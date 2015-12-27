using System;
using static Insight.WS.Verify.Util;

namespace Insight.WS.Verify
{
    public class Verify
    {
        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        private readonly Session Session;

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        private readonly Session Basis;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="obj"></param>
        public Verify(Session obj)
        {
            Session = obj;
            Basis = obj.ID < Sessions.Count ? Sessions[obj.ID] : GetSession(obj);
            if (Basis.UserId != obj.UserId)
            {
                Basis = GetSession(obj);
            }
        }

        /// <summary>
        /// 通过Session返回验证结果
        /// </summary>
        /// <returns>Session 包含验证结果的Session</returns>
        public Session ReturnSession()
        {
            // 用户被封禁
            if (!Basis.Validity)
            {
                Session.LoginResult = LoginResult.Banned;
                return Session;
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
                Session.LoginResult = LoginResult.Failure;
                return Session;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            Session.LoginResult = Basis.MachineId == Session.MachineId ? LoginResult.Success : LoginResult.Multiple;
            return Session;
        }

        /// <summary>
        /// 返回验证结果
        /// </summary>
        /// <returns>bool 是否通过验证</returns>
        public bool ReturnBoolean()
        {
            // 用户被封禁，不能通过验证
            if (!Basis.Validity) return false;

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
            return true;
        }

    }
}
