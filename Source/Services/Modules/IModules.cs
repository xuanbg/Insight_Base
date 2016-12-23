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
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetNavigation();

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "modules/{id}/actions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetAction(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "modules/{id}/params", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleParam(string id);

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "modules/{id}/params/user", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleUserParam(string id);

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "modules/{id}/params/dept", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleDeptParam(string id);

        /// <summary>
        /// 保存模块选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="apl">新增参数集合</param>
        /// <param name="upl">更新参数集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "modules/{id}/params", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SaveModuleParam(string id, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl);

    }
}
