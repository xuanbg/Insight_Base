using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceContract]
    public interface ICodes
    {
        /// <summary>
        /// 新增编码方案
        /// </summary>
        /// <param name="obj">SYS_Code_Scheme</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "POST", UriTemplate = "", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result AddScheme(SYS_Code_Scheme obj);

        /// <summary>
        /// 根据ID删除编码方案或设为不可用
        /// </summary>
        /// <param name="id">编码方案ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result DeleteScheme(string id);

        /// <summary>
        /// 更新编码方案
        /// </summary>
        /// <param name="id">编码方案ID</param>
        /// <param name="scheme">SYS_Code_Scheme</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result UpdateScheme(string id, SYS_Code_Scheme scheme);

        /// <summary>
        /// 根据ID更新编码方案状态
        /// </summary>
        /// <param name="id">编码方案ID</param>
        /// <returns>Result</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "{id}/validity", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        Result EnableScheme(string id);

        /// <summary>
        /// 根据ID获取编码方案对象实体
        /// </summary>
        /// <param name="id">编码方案ID</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetScheme(string id);

        /// <summary>
        /// 获取全部编码方案
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetSchemes();

        /// <summary>
        /// 获取全部流水码使用记录
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "serials", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetSerialRecord();

        /// <summary>
        /// 获取全部流水码分配记录
        /// </summary>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "allots", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetAllotRecord();

        /// <summary>
        /// 根据传入参数获取编码方案预览
        /// </summary>
        /// <param name="id">编码方案ID</param>
        /// <param name="code">编码格式</param>
        /// <param name="mark">分组规则</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "{id}/preview?code={code}&mark={mark}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetCodePreview(string id, string code, string mark);

        /// <summary>
        /// 根据传入参数获取编码方案预览
        /// </summary>
        /// <param name="name">编码方案名称</param>
        /// <param name="id">业务记录ID</param>
        /// <param name="mark">标识符</param>
        /// <returns>Result</returns>
        [WebGet(UriTemplate = "{name}/newcode?id={id}&mark={mark}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetCode(string name, string id, string mark);

    }
}
