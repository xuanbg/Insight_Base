using System;
using System.Collections.Generic;
using System.ServiceModel;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Modules:IModule
    {
        public JsonResult GetModuleGroup()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetUserModule()
        {
            throw new NotImplementedException();
        }

        public JsonResult GetModuleInfo(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetAction(string id)
        {
            throw new NotImplementedException();
        }

        public List<SYS_ModuleParam> GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public List<SYS_ModuleParam> GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public List<SYS_ModuleParam> GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }

        public bool SaveModuleParam(List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
        {
            throw new NotImplementedException();
        }
    }
}
