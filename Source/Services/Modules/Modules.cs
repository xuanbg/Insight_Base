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
            if (!Verify()) return result;


            return result.Success();
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetAction(string id)
        {
            if (!Verify()) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            return result.Success();
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

        private Result<object> result = new Result<object>();
        private Token token;
        private string tokenId;
        private string userId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string key = null)
        {
            var verify = new OAuth.Verify();
            result = verify.Compare(key);
            if (!result.successful) return false;

            token = verify.basis;
            tokenId = verify.tokenId;
            userId = token.userId;
            return result.successful;
        }
    }
}