using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using static Insight.Base.Common.Parameters;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Logs : ILogs
    {
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="code">事件代码（必须有）</param>
        /// <param name="message">事件消息，为空则使用默认消息文本</param>
        /// <param name="source">来源（可为空）</param>
        /// <param name="action">操作（可为空）</param>
        /// <param name="key">查询用的关键字段</param>
        /// <param name="userid">事件源用户ID（可为空）</param>
        /// <returns>Result</returns>
        public Result WriteToLog(string code, string message, string source, string action, string key, string userid)
        {
            if (!Verify()) return _Result;

            var parse = new GuidParse(userid, true);
            if (!parse.Result.Successful) return parse.Result;

            var logger = new Logger(code, message, source, action, key, parse.Guid);
            var succe = logger.Write();
            if (!succe.HasValue)
            {
                var result = new JsonResult();
                result.InvalidEventCode();
                return result;
            }

            if (!succe.Value) _Result.DataBaseError();

            return _Result;
        }

        /// <summary>
        /// 新增日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>Result</returns>
        public Result AddRule(SYS_Logs_Rules rule)
        {
            if (!Verify("60A97A33-0E6E-4856-BB2B-322FEEEFD96A")) return _Result;

            var result = new JsonResult();
            if (string.IsNullOrEmpty(rule.Code) || !Regex.IsMatch(rule.Code, @"^\d{6}$"))
            {
                result.InvalidEventCode();
                return result;
            }

            var level = Convert.ToInt32(rule.Code.Substring(0, 1));
            if (level <= 1 || level == 7)
            {
                result.EventWithoutConfig();
                return result;
            }

            if (Rules.Any(r => r.Code == rule.Code))
            {
                result.EventCodeUsed();
                return result;
            }

            rule.CreatorUserId = _UserId;
            if (!Insert(rule))
            {
                _Result.DataBaseError();
                return _Result;
            }

            var log = new
            {
                UserID = _UserId,
                Message = $"事件代码【{rule.Code}】已由{_UserName}创建和配置为：{Util.Serialize(rule)}"
            };
            var logger = new Logger("600601", Util.Serialize(log));
            logger.Write();
            return _Result;
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>Result</returns>
        public Result RemoveRule(string id)
        {
            if (!Verify("BBC43098-A030-46CA-A681-0C3D1ECC15AB")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            if (!DeleteRule(parse.Value))
            {
                _Result.DataBaseError();
                return _Result;
            }

            var log = new
            {
                UserID = _UserId,
                Message = $"事件配置【{id}】已被{_UserName}删除"
            };
            var logger = new Logger("600602", Util.Serialize(log));
            logger.Write();
            return _Result;
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>Result</returns>
        public Result EditRule(SYS_Logs_Rules rule)
        {
            if (!Verify("9FF1547D-2E3F-4552-963F-5EA790D586EA")) return _Result;

            if (!Update(rule))
            {
                _Result.DataBaseError();
                return _Result;
            }

            var log = new
            {
                UserID = _UserId,
                Message = $"事件代码【{rule.Code}】已被{_UserName}修改为：{Util.Serialize(rule)}"
            };
            var logger = new Logger("600603", Util.Serialize(log));
            logger.Write();
            return _Result;
        }

        /// <summary>
        /// 获取日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>Result</returns>
        public Result GetRule(string id)
        {
            if (!Verify("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var rule = Rules.SingleOrDefault(r => r.ID == parse.Value);
            if (rule == null) _Result.NotFound();
            else _Result.Success(rule);

            return _Result;
        }

        /// <summary>
        /// 获取全部日志规则
        /// </summary>
        /// <returns>Result</returns>
        public Result GetRules()
        {
            if (!Verify("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F")) return _Result;

            if (Rules.Any()) _Result.Success(Rules);
            else _Result.NoContent();

            return _Result;
        }

        private Result _Result = new Result();
        private Guid _UserId;
        private string _UserName;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var verify = new Compare(action);
            _UserId = verify.Basis.UserId;
            _UserName = verify.Basis.UserName;
            _Result = verify.Result;

            return _Result.Successful;
        }
    }
}
