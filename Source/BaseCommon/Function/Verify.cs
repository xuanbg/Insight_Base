using System;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.General;
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
        /// JsonResult类型的验证结果
        /// </summary>
        public JsonResult Result = new JsonResult();

        /// <summary>
        /// Guid转换结果
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// 用于验证的基础规则
        /// </summary>
        private readonly string Rule;

        /// <summary>
        /// 用于验证的字符串
        /// </summary>
        private readonly string VerifyString;

        /// <summary>
        /// Session.ID是否一致
        /// </summary>
        private readonly bool Identical = true;

        /// <summary>
        /// 最大授权数
        /// </summary>
        private readonly int MaxAuth = Convert.ToInt32(GetAppSetting("MaxAuth"));

        /// <summary>
        /// 构造方法，使用Session验证
        /// </summary>
        public Verify()
        {
            var dict = GetAuthorization();
            Session = GetAuthor<Session>(dict["Auth"]);
            if (Session == null)
            {
                Result.InvalidAuth();
                return;
            }

            Basis = Session.ID < Sessions.Count ? Sessions[Session.ID] : GetSession(Session);
            if (Basis == null) return;

            if (Basis.LoginName == Session.LoginName) return;

            Identical = false;
            Basis = GetSession(Session);
        }

        /// <summary>
        /// 构造方法，使用简单规则验证
        /// </summary>
        /// <param name="rule">验证规则</param>
        public Verify(string rule)
        {
            Rule = Hash(rule);
            var dict = GetAuthorization();
            VerifyString = GetAuthor<string>(dict["Auth"]);
            if (VerifyString == null)
                Result.InvalidAuth();
        }

        /// <summary>
        /// 对Rule进行校验，返回验证结果
        /// </summary>
        /// <returns>bool</returns>
        public bool CompareUsageRule()
        {
            return Result.Successful && Rule == VerifyString;
        }

        /// <summary>
        /// 用户登录专用验证方法
        /// </summary>
        /// <returns>bool</returns>
        public void SignIn()
        {
            if (!Compare()) return;

            Basis.DeptId = Session.DeptId;
            Basis.DeptName = Session.DeptName;
            Basis.MachineId = Session.MachineId;
            Result.Success(Serialize(Basis));
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
        /// 转换一个用户ID，并对Session进行校验
        /// </summary>
        /// <param name="id">要转换的UserId</param>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool</returns>
        public bool ParseUserIdAndCompare(string id, string action = null)
        {
            if (Guid.TryParse(id, out Guid))
            {
                if (Basis.UserId == Guid) action = null;

                return Compare(action);
            }

            Result.InvalidGuid();
            return false;
        }

        /// <summary>
        /// 对Session进行校验，返回验证结果
        /// </summary>
        /// <param name="action">操作码，默认为空</param>
        /// <returns>bool</returns>
        public bool Compare(string action = null)
        {
            if (Basis == null)
            {
                Session.LoginResult = LoginResult.NotExist;
                Result.InvalidAuth(Serialize(Session));
                return false;
            }

            if (Basis.ID > MaxAuth)
            {
                Session.LoginResult = LoginResult.Unauthorized;
                Result.InvalidAuth(Serialize(Session));
                return false;
            }

            if (!Basis.Validity)
            {
                Session.LoginResult = LoginResult.Banned;
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
                Session.LoginResult = LoginResult.Failure;
                Result.InvalidAuth(Serialize(Session));
                return false;
            }

            Basis.OnlineStatus = true;
            Basis.FailureCount = 0;
            if (Basis.MachineId == Session.MachineId)
            {
                Basis.LoginResult = LoginResult.Success;
            }
            else
            {
                Basis.LoginResult = LoginResult.Multiple;
                Result.Multiple();
            }
            
            // 如Session.ID不一致，返回Session过期的信息
            if (Identical) Result.Success();
            else Result.Expired(Serialize(Basis));

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
        /// 根据用户账号获取用户Session
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>Session</returns>
        private Session GetSession(Session obj)
        {
            // 先在缓存中根据用户账号查找，找到即返回结果
            var session = Sessions.SingleOrDefault(s => string.Equals(s.LoginName, obj.LoginName, StringComparison.CurrentCultureIgnoreCase));
            if (session != null) return session;

            // 未在缓存中找到结果时，再在数据库中根据用户账号查找用户；
            // 找到后根据用户信息初始化Session数据、加入缓存并返回结果，否则返回null。
            var user = DataAccess.GetUser(obj.LoginName);
            if (user == null) return null;

            session = new Session
            {
                ID = Sessions.Count,
                UserId = user.ID,
                UserName = user.Name,
                OpenId = user.OpenId,
                LoginName = user.LoginName,
                Signature = Hash(user.LoginName.ToUpper() + user.Password),
                UserType = user.Type,
                Validity = user.Validity,
                MachineId = obj.MachineId
            };
            Sessions.Add(session);
            return session;
        }

    }
}
