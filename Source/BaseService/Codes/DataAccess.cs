using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.DataAccess;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base
{
    public partial class BaseService
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="did"></param>
        /// <param name="uid"></param>
        /// <param name="bid"></param>
        /// <param name="mid"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
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
                new SqlParameter("@Char", mark),
            };
            return SqlScalar(MakeCommand(sql, parm));
        }

    }
}
