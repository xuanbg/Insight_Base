using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    interface IModules
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetNavigation();

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "modules/{id}/actions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetAction(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "modules/{id}/params", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetModuleParam(string id);

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "modules/{id}/params/user", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetModuleUserParam(string id);

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "modules/{id}/params/dept", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetModuleDeptParam(string id);
    }
}
