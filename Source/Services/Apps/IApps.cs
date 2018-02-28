using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IApps
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 根据关键词查询全部应用集合
        /// </summary>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/all?key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetAllApps(string key);

        /// <summary>
        /// 根据关键词查询租户绑定应用集合
        /// </summary>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps?key={key}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetApps(string key);

        /// <summary>
        /// 获取指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetApp(string id);

        /// <summary>
        /// 新增应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "apps", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddApp(Application app);

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "apps", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditApp(Application app);

        /// <summary>
        /// 删除指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "apps/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteApp(string id);
    }
}
