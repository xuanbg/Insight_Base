using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Modules:IModules
    {
        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        public Result GetNavigation()
        {
            if (!Verify()) return _Result;

            var auth = new Authority(_UserId, _DeptId, InitType.Navigation);
            var data = new {Groups = auth.GetModuleGroups(), Modules = auth.GetModules()};
            _Result.Success(data);

            return _Result;
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result GetAction(string id)
        {
            if (!Verify()) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var auth = new Authority(_UserId, _DeptId, InitType.ToolBar);
            var data = auth.ModuleActions(parse.Value);
            if (data.Any()) _Result.Success(data);
            else _Result.NoContent();

            return _Result;
        }

        public JsonResult GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }

        public JsonResult SaveModuleParam(string id, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
        {
            throw new NotImplementedException();
        }

        private Result _Result = new Result();
        private Guid _UserId;
        private Guid? _DeptId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var verify = new Compare(action);
            _UserId = verify.Basis.UserId;
            _DeptId = verify.Basis.DeptId;
            _Result = verify.Result;

            return _Result.Successful;
        }
    }
}