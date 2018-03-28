using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface ITemplates
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 获取模板分类
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "templates/categorys?mid={id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetCategorys(string id);

        /// <summary>
        /// 新建模板分类
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "templates/categorys", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddCategory(Catalog catalog);

        /// <summary>
        /// 编辑模板分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <param name="catalog"></param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "templates/categorys/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditCategory(string id, Catalog catalog);

        /// <summary>
        /// 删除模板分类
        /// </summary>
        /// <param name="id">报表分类ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "templates/categorys/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteCategory(string id);

        /// <summary>
        /// 获取所有报表模板
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "templates?cid={id}&rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetTemplates(string id, int rows, int page);

        /// <summary>
        /// 获取报表模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "templates/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetTemplate(string id);

        /// <summary>
        /// 导入报表模板
        /// </summary>
        /// <param name="template">报表模板</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "templates", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> ImportTemplate(Template template);

        /// <summary>
        /// 复制报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="template">报表模板</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "templates/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> CopyTemplate(string id, Template template);

        /// <summary>
        /// 编辑报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="template">报表模板</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "templates/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditTemplate(string id, Template template);

        /// <summary>
        /// 设计报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <param name="content">模板内容</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "templates/{id}/content", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DesignTemplet(string id, string content);

        /// <summary>
        /// 删除报表模板
        /// </summary>
        /// <param name="id">报表模板ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "templates/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteTemplate(string id);
    }
}