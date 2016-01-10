using System;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base.Common
{
    public class Verify
    {

        /// <summary>
        /// 用于验证的基准对象
        /// </summary>
        public readonly Session Basis;

        /// <summary>
        /// 用于验证的目标对象
        /// </summary>
        public readonly Session Session;

        /// <summary>
        /// 是否通过验证
        /// </summary>
        public bool Pass;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="obj"></param>
        public Verify(Session obj)
        {
            Session = obj;
            Basis = obj.ID < Sessions.Count ? Sessions[obj.ID] : GetSession(obj);
            if (Basis.LoginName != obj.LoginName)
            {
                Basis = GetSession(obj);
            }
        }

        /// <summary>
        /// 返回验证结果
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult Compare()
        {
            var result = new JsonResult();
            if (!Basis.Validity)
            {
                Session.LoginResult = LoginResult.Banned;
                return result.Disabled();
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
                return result.InvalidAuth();
            }

            Pass = true;
            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            Basis.LoginResult = Basis.MachineId == Session.MachineId ? LoginResult.Success : LoginResult.Multiple;
            return result.Success();
        }

    }
}
