using System;
using System.Collections.Generic;
using System.Data;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Service
{
    public partial class BaseService : Iroles
    {
        public JsonResult AddRole(SYS_Role role, DataTable action, DataTable data, DataTable custom)
        {
            throw new NotImplementedException();
        }

        public JsonResult RemoveRole(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult EditRole(SYS_Role obj, List<object> adl, List<object> ddl, List<object> cdl, DataTable adt, DataTable ddt, DataTable cdt)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRole(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetAllRole()
        {
            throw new NotImplementedException();
        }

        public JsonResult AddRoleMember(string id, List<string> tids, List<string> gids, List<string> uids)
        {
            throw new NotImplementedException();
        }

        public JsonResult DeleteRoleMember(int type, string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleMember()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleUser()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetMemberOfTitle(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetMemberOfGroup(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetMemberOfUser(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleActions(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleRelData(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleModulePermit()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleActionPermit()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetRoleDataPermit()
        {
            throw new NotImplementedException();
        }
    }
}
