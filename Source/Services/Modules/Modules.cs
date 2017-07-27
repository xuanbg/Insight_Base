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
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetNavigation()
        {
            if (!Verify()) return _Result;

            var auth = new Authority(_UserId, _DeptId, InitType.Navigation);
            var data = new {Groups = auth.GetModuleGroups(), Modules = auth.GetModules()};

            return _Result.Success(data);
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetAction(string id)
        {
            if (!Verify()) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var auth = new Authority(_UserId, _DeptId, InitType.ToolBar);
            var data = auth.ModuleActions(parse.Value);
            return data.Any() ? _Result.Success(data) : _Result.NoContent(new List<object>());
        }

        public Result<object> GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> SaveModuleParam(string id, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
        {
            throw new NotImplementedException();
        }

        private Result<object> _Result = new Result<object>();
        private Guid _UserId;
        private Guid? _DeptId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var compare = new Compare();
            _Result = compare.Result;
            if (!_Result.successful) return false;

            _UserId = compare.Basis.userId;
            _DeptId = compare.Token.deptId;
            _Result = compare.Verify(action);
            return _Result.successful;
        }
    }
}