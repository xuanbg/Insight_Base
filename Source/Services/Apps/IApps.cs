using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;
using Function = Insight.Base.Common.Entity.Function;

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
        void responseOptions();

        /// <summary>
        /// 查询全部应用集合
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/all", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getAllApps();

        /// <summary>
        /// 查询租户绑定应用集合
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getApps();

        /// <summary>
        /// 获取指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getApp(string id);

        /// <summary>
        /// 新增应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "apps", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addApp(Application app);

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "apps/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> editApp(string id, Application app);

        /// <summary>
        /// 删除指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "apps/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> deleteApp(string id);

        /// <summary>
        /// 新增导航信息
        /// </summary>
        /// <param name="nav">导航实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "apps/navigations", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addNav(Navigator nav);

        /// <summary>
        /// 修改导航信息
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <param name="nav">应用导航实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "apps/navigations/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> editNav(string id, Navigator nav);

        /// <summary>
        /// 删除指定ID的导航
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "apps/navigations/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> deleteNav(string id);

        /// <summary>
        /// 新增功能信息
        /// </summary>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "apps/navigations/functions", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> addFun(Function fun);

        /// <summary>
        /// 修改功能信息
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "apps/navigations/functions/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> editFun(string id, Function fun);

        /// <summary>
        /// 删除指定ID的功能
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "apps/navigations/functions/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> deleteFun(string id);

        /// <summary>
        /// 获取指定应用ID的导航数据
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/{id}/navigations", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getNavigations(string id);

        /// <summary>
        /// 获取指定导航ID的功能集合
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "apps/navigations/{id}/functions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> getFunctions(string id);
    }
}
