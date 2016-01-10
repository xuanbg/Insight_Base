using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base.Common
{
    public class General
    {
        /// <summary>
        /// 通过指定的Rule校验
        /// </summary>
        /// <param name="rule">规则字符串</param>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify(string rule)
        {
            var result = new JsonResult();
            var obj = GetAuthorization<string>();
            return obj != Hash(rule) ? result.InvalidAuth() : result.Success();
        }

        /// <summary>
        /// 通过Session验证
        /// </summary>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify()
        {
            Session obj;
            return Verify(out obj);
        }

        /// <summary>
        /// 通过Session验证
        /// </summary>
        /// <param name="session">Session</param>
        /// <param name="basis">是否返回基准Session，默认返回源Session</param>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify(out Session session, bool basis = false)
        {
            var result = new JsonResult();
            session = GetAuthorization<Session>();
            if (session == null) return result.InvalidAuth();

            var verify = new Verify(session);
            if (verify.Pass && basis) session = verify.Basis;
            return verify.Compare();
        }

    }
}
