using System;
using System.Data;
using System.Data.SqlClient;
using Insight.Base.Common.Utils;

namespace Insight.Base.Services
{
    public partial class Codes
    {

        /// <summary>
        /// 调用存储过程生成编码
        /// </summary>
        /// <param name="sid">编码方案ID</param>
        /// <param name="did">部门ID（可为空）</param>
        /// <param name="uid">用户ID</param>
        /// <param name="bid">业务记录ID</param>
        /// <param name="mid">业务模块ID（可为空）</param>
        /// <param name="mark">编码中的变量</param>
        /// <returns>根据编码方案生成的编码</returns>
        private object GetCode(Guid sid, Guid? did, Guid uid, Guid bid, Guid? mid, string mark)
        {
            const string sql = "exec GetCode @SchemeId, @DeptId, @UserId, @BusinessId, @ModuleId, @Char";
            var parm = new[]
            {
                new SqlParameter("@SchemeId", SqlDbType.UniqueIdentifier) {Value = sid},
                new SqlParameter("@DeptId", SqlDbType.UniqueIdentifier) {Value = did},
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = uid},
                new SqlParameter("@BusinessId", SqlDbType.UniqueIdentifier) {Value = bid},
                new SqlParameter("@ModuleId", SqlDbType.UniqueIdentifier) {Value = mid},
                new SqlParameter("@Char", mark)
            };
            return SqlHelper.SqlScalar(SqlHelper.MakeCommand(sql, parm));
        }

    }
}
