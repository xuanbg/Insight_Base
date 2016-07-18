using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceContract]
    public interface IModules
    {
        /// <summary>
        /// 获取用户获得授权的所有模块的组信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "groups", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleGroup();

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetUserModule();

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/actions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetAction(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/params", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleParam(string id);

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/params/user", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleUserParam(string id);

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "{id}/params/dept", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetModuleDeptParam(string id);

        /// <summary>
        /// 保存模块选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="apl">新增参数集合</param>
        /// <param name="upl">更新参数集合</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "{id}/params", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult SaveModuleParam(string id, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl);

    }
}
