using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface IRules
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        void ResponseOptions();

        /// <summary>
        /// 获取所有报表分期规则
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "rules?rows={rows}&page={page}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetRules(int rows, int page);

        /// <summary>
        /// 获取报表分期
        /// </summary>
        /// <param name="id">分期ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "rules/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result<object> GetRule(string id);

        /// <summary>
        /// 新建报表分期
        /// </summary>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "rules", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> AddRule(ReportRule rule);

        /// <summary>
        /// 编辑报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <param name="rule">报表分期</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "rules/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> EditRule(string id, ReportRule rule);

        /// <summary>
        /// 删除报表分期
        /// </summary>
        /// <param name="id">报表分期ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "rules/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result<object> DeleteRule(string id);
    }
}