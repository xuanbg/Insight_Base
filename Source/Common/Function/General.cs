using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base.Common
{
    public class General
    {
        /// <summary>
        /// 构造方法，使用简单规则验证
        /// </summary>
        /// <param name="rule">验证规则</param>
        public static JsonResult Verify(string rule)
        {
            var dict = GetAuthorization();
            var basis = GetAuthor<string>(dict["Auth"]);
            var result = new JsonResult();
            return basis == null || basis != Hash(rule) ? result.InvalidAuth() : result.Success();
        }

        /// <summary>
        /// 获取Http请求头部承载的验证信息
        /// </summary>
        /// <returns>string Http请求头部承载的验证字符串</returns>
        public static Dictionary<string, string> GetAuthorization()
        {
            var context = WebOperationContext.Current;
            if (context == null) return null;

            var headers = context.IncomingRequest.Headers;
            var response = context.OutgoingResponse;
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Accept, Content-Type");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Origin", "*");

            var accept = CompareVersion(headers);
            if (accept == null)
            {
                response.StatusCode = HttpStatusCode.NotAcceptable;
                return null;
            }

            var auth = headers[HttpRequestHeader.Authorization];
            if (string.IsNullOrEmpty(auth))
            {
                response.StatusCode = HttpStatusCode.Unauthorized;
                return null;
            }

            return new Dictionary<string, string>
            {
                {"Auth", auth},
                {"Version", accept[1].Substring(9)},
                {"Client", accept[2].Substring(8)}
            };
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <param name="auth">验证信息</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public static T GetAuthor<T>(string auth)
        {
            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                return Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                LogToLogServer("500101", ex.ToString());
                return default(T);
            }
        }

        /// <summary>
        /// 将事件消息写到日志服务器
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        public static void LogToLogServer(string code, string message = null)
        {
            LogToLogServer(code, message, null, null);
        }

        /// <summary>
        /// 将事件消息写到日志服务器
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        /// <param name="source">事件来源</param>
        /// <param name="action">操作名称</param>
        public static void LogToLogServer(string code, string message, string source, string action)
        {
            LogToLogServer(code, message, source, action, null, null);
        }

        /// <summary>
        /// 将事件消息写到日志服务器
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        /// <param name="source">事件来源</param>
        /// <param name="action">操作名称</param>
        /// <param name="userid">源用户ID</param>
        /// <param name="key">查询关键字段</param>
        public static void LogToLogServer(string code, string message, string source, string action, string userid, string key)
        {
            var url = LogServer + "logs";
            var dict = new Dictionary<string, string>
            {
                {"code", code},
                {"message", message},
                {"source", source },
                {"action", action },
                {"userid", userid },
                {"key", key }
            };
            var data = Serialize(dict);
            var author = Base64(Hash(code + Secret));
            HttpRequest(url, "POST", author, data);
        }

        /// <summary>
        /// HttpRequest方法
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <param name="data">接口参数</param>
        /// <returns>JsonResult</returns>
        public static JsonResult HttpRequest(string url, string method, string author, string data = "")
        {
            var request = GetWebRequest(url, method, author);
            if (method == "GET") return GetResponse(request);

            var buffer = Encoding.UTF8.GetBytes(data);
            request.ContentLength = buffer.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            return GetResponse(request);
        }

        #region 静态私有方法

        /// <summary>
        /// 获取WebRequest对象
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <returns>HttpWebRequest</returns>
        private static HttpWebRequest GetWebRequest(string url, string method, string author)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = $"application/json; version={CurrentVersion}; client=BaseServer";
            request.ContentType = "application/json";
            request.Headers.Add(HttpRequestHeader.Authorization, author);

            return request;
        }

        /// <summary>
        /// 获取Request响应数据
        /// </summary>
        /// <param name="request">WebRequest</param>
        /// <returns>JsonResult</returns>
        private static JsonResult GetResponse(WebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return new JsonResult().BadRequest();

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
                {
                    var result = reader.ReadToEnd();
                    responseStream.Close();
                    return Deserialize<JsonResult>(result);
                }
            }
            catch (Exception ex)
            {
                LogToEvent(ex.ToString());
                return new JsonResult().BadRequest();
            }
        }

        /// <summary>
        /// 验证版本是否兼容
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static string[] CompareVersion(WebHeaderCollection headers)
        {
            var accept = headers[HttpRequestHeader.Accept];
            if (accept == null) return null;

            var array = accept.Split(Convert.ToChar(";"));
            if (array.Length < 3) return null;

            var ver = Convert.ToInt32(array[1].Substring(9));
            var comp = ver >= Convert.ToInt32(CompatibleVersion) && ver <= Convert.ToInt32(UpdateVersion);
            return comp ? array : null;
        }

        #endregion

    }
}
