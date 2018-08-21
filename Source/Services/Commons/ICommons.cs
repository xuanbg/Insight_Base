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
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void responseOptions();

        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getNavigation();

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations/{id}/functions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getFunctions(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "navigations/{id}/params", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getModuleParam(string id);

        /// <summary>
        /// 保存选项数据
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <param name="list">选项数据集合</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "navigations/{id}/params", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> saveModuleParam(string id, List<Parameter> list);

        /// <summary>
        /// 获取行政区划
        /// </summary>
        /// <param name="id">上级区划ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "regions?pid={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getRegions(string id);

        /// <summary>
        /// 获取应用客户端文件信息集合
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/{id}/files", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getFiles(string id);

        /// <summary>
        /// 获取指定名称的文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/files/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getFile(string id);

        /// <summary>
        /// 获取指定ID的电子影像数据
        /// </summary>
        /// <param name="id">影像ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "images/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getImageData(string id);

        /// <summary>
        /// 生成指定业务数据ID的报表
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <param name="templateId">模板ID</param>
        /// <param name="deptName">部门名称</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "images/{id}", ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> buildImageData(string id, string templateId, string deptName);

        /// <summary>
        /// 获取报表模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "templates/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getTemplate(string id);
    }
}