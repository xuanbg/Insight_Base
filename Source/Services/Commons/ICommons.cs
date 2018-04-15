using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface ICommons
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
        [WebGet(UriTemplate = "navigations/{id}/functions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetFunctions(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations/{id}/params", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetModuleParam(string id);

        /// <summary>
        /// 保存选项数据
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <param name="list">选项数据集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "navigations/{id}/params", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> SaveModuleParam(string id, List<Parameter> list);

        /// <summary>
        /// 获取行政区划
        /// </summary>
        /// <param name="id">上级区划ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "regions?pid={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetRegions(string id);

        /// <summary>
        /// 获取应用客户端文件信息集合
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/{id}/files", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetFiles(string id);

        /// <summary>
        /// 获取指定名称的文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/files/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetFile(string id);
    }
}
