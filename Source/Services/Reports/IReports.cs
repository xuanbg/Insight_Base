using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IReports
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 获取报表分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "reports/categorys?mid={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetCategorys(string id);

        /// <summary>
        /// 新建报表分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "reports/categorys", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddCategory(Catalog catalog);

        /// <summary>
        /// 编辑报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "reports/categorys/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditCategory(string id, Catalog catalog);

        /// <summary>
        /// 删除报表分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "reports/categorys/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteCategory(string id);

        /// <summary>
        /// 获取分类下全部报表
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">页码</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "reports?cid={id}&rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetReports(string id, int rows, int page);

        /// <summary>
        /// 获取报表
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "reports/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetReport(string id);

        /// <summary>
        /// 新建报表
        /// </summary>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "reports", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddReport(Definition report);

        /// <summary>
        /// 编辑报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <param name="report">报表</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "reports/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditReport(string id, ReportDefinition report);

        /// <summary>
        /// 删除报表
        /// </summary>
        /// <param name="id">报表ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "reports/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteReport(string id);
    }
}