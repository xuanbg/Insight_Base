using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base.Organization
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class OrgManger :Interface
    {
        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetOrgs()
        {
            var result = General.Authorize("928C7527-A2F7-49A3-A548-12B3834D8822");
            if (!result.Successful) return result;

            const string sql = "select * from Organization order by NodeType desc, [Index]";
            var data = SqlQuery(MakeCommand(sql));
            return data.Rows.Count > 0 ? result.Success(Util.Serialize(data)) : result.NoContent();
        }

        public JsonResult AddOrg(SYS_Organization org, int index)
        {
            throw new NotImplementedException();
        }

        public JsonResult DeleteOrg(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult UpdateOrg(SYS_Organization obj, int index)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetOrg(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult AddOrgMerger(SYS_OrgMerger org)
        {
            throw new NotImplementedException();
        }

        public JsonResult UpdateOrgParentId(SYS_Organization org)
        {
            throw new NotImplementedException();
        }

        public JsonResult AddOrgMember(string id, List<Guid> uids)
        {
            throw new NotImplementedException();
        }

        public JsonResult DeleteOrgMember(List<Guid> ids)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetOrgMembers()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetOrgMemberBeSides(string id)
        {
            throw new NotImplementedException();
        }

    }
}
