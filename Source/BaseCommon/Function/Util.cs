using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Common
{
    public static class Util
    {

        #region 静态字段

        /// <summary>
        /// 安全码
        /// </summary>
        public const string Secret = "842A381C91CE43A98720825601C22A56";

        /// <summary>
        /// 在线用户列表
        /// </summary>
        public static List<Session> Sessions = new List<Session>();

        /// <summary>
        /// 短信验证码的缓存列表
        /// </summary>
        public static readonly List<VerifyRecord> SmsCodes = new List<VerifyRecord>();

        /// <summary>
        /// 用于生成短信验证码的随机数发生器
        /// </summary>
        public static readonly Random Random = new Random(Environment.TickCount);

        /// <summary>
        /// 日志服务路径
        /// </summary>
        public static readonly string LogServer = GetAppSetting("LogServer");

        /// <summary>
        /// 当前程序集版本
        /// </summary>
        public static int Version;

        /// <summary>
        /// 接口最后兼容版本
        /// </summary>
        private static readonly string CompatibleVersion = GetAppSetting("CompatibleVersion");

        /// <summary>
        /// 接口最新版本
        /// </summary>
        private static readonly string UpdateVersion = GetAppSetting("UpdateVersion");

        #endregion

        #region 静态公共方法

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
            if (!CompareVersion(headers))
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

            var accept = headers[HttpRequestHeader.Accept];
            var val = accept.Split(Convert.ToChar(";"));
            var dict = new Dictionary<string, string>
            {
                {"Auth", auth},
                {"Version", val[1].Substring(9)},
                {"Client", val[2].Substring(8)}
            };

            var type = headers[HttpRequestHeader.ContentType];
            if (type != "application/x-gzip") return dict;

            response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            response.ContentType = "application/x-gzip";

            return dict;
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
        /// 读取配置项的值
        /// </summary>
        /// <param name="key">配置项</param>
        /// <returns>配置项的值</returns>
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 将对象转换为Base64编码的字符串
        /// </summary>
        /// <typeparam name="T">输入类型</typeparam>
        /// <param name="obj">用于转换的数据对象</param>
        /// <returns>string Base64编码的字符串</returns>
        public static string Base64<T>(T obj)
        {
            var json = Serialize(obj);
            var buff = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// 计算字符串的Hash值
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>String Hash值</returns>
        public static string Hash(string str)
        {
            var md5 = MD5.Create();
            var s = md5.ComputeHash(Encoding.UTF8.GetBytes(str.Trim()));
            return s.Aggregate("", (current, c) => current + c.ToString("X2"));
        }

        /// <summary>
        /// 将事件消息写入系统日志
        /// </summary>
        /// <param name="msg">Log消息</param>
        /// <param name="type">Log类型（默认Error）</param>
        /// <param name="source">事件源（默认Insight VerifyServer Service）</param>
        public static void LogToEvent(string msg, EventLogEntryType type = EventLogEntryType.Error, string source = "Insight Base Service")
        {
            EventLog.WriteEntry(source, msg, type);
        }

        /// <summary>
        /// 将事件消息写到日志服务器
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息</param>
        public static void LogToLogServer(string code, string message)
        {
            var dict = new Dictionary<string, string>
            {
                {"code", code},
                {"message", message},
                {"userid", null}
            };
            var data = Serialize(dict);
            var author = Base64(Hash(code + Secret));
            HttpRequest(LogServer, "POST", author, data);
        }

        /// <summary>
        /// HttpRequest方法
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <param name="data">接口参数</param>
        /// <param name="compress">是否压缩传输，默认压缩</param>
        /// <returns>JsonResult</returns>
        public static JsonResult HttpRequest(string url, string method, string author, string data = "", bool compress = true)
        {
            var request = GetWebRequest(url, method, author, compress);
            if (method == "GET") return GetResponse(request);

            var buffer = compress ? Compress(Encoding.UTF8.GetBytes(data)) : Encoding.UTF8.GetBytes(data);
            request.ContentLength = buffer.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            return GetResponse(request);
        }

        #endregion

        #region Serialize/Deserialize

        /// <summary>
        /// 将一个对象序列化为Json字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>string Json字符串</returns>
        public static string Serialize<T>(T obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        /// <summary>
        /// 将一个Json字符串反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <returns>T 反序列化的对象</returns>
        public static T Deserialize<T>(string json)
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }

        #endregion

        #region Compress/Decompress

        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            var ms = new MemoryStream();
            var stream = new GZipStream(ms, CompressionMode.Compress, true);
            stream.Write(data, 0, data.Length);
            stream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="dada"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] dada)
        {
            var ms = new MemoryStream(dada);
            var stream = new GZipStream(ms, CompressionMode.Decompress);
            var buffer = new MemoryStream();
            var block = new byte[1024];
            while (true)
            {
                var read = stream.Read(block, 0, block.Length);
                if (read <= 0) break;
                buffer.Write(block, 0, read);
            }
            stream.Close();
            return buffer.ToArray();
        }

        #endregion

        #region 静态私有方法

        /// <summary>
        /// 在启用Gzip压缩时设置Response参数
        /// </summary>
        private static void SetResponseParam()
        {
            var response = WebOperationContext.Current.OutgoingResponse;
            response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            response.ContentType = "application/x-gzip";
        }

        /// <summary>
        /// 获取WebRequest对象
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <param name="compress">是否压缩传输</param>
        /// <returns>HttpWebRequest</returns>
        private static HttpWebRequest GetWebRequest(string url, string method, string author, bool compress)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = $"application/x-gzip/json; version={Version}; client=5";
            request.ContentType = compress ? "application/x-gzip" : "application/json";
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

                var encoding = response.Headers["Content-Encoding"];
                if (encoding != null && encoding.ToLower().Contains("gzip"))
                {
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                }

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
                {
                    var result = reader.ReadToEnd();
                    responseStream.Close();
                    return Deserialize<JsonResult>(result);
                }
            }
            catch (Exception ex)
            {
                LogToLogServer("100101", ex.ToString());
                return new JsonResult().BadRequest();
            }
        }

        /// <summary>
        /// 验证版本是否兼容
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static bool CompareVersion(WebHeaderCollection headers)
        {
            var accept = headers[HttpRequestHeader.Accept];
            if (accept == null) return false;

            var val = accept.Split(Convert.ToChar(";"));
            if (accept.Length < 3) return false;

            var ver = Convert.ToInt32(val[1].Substring(9));
            return ver >= Convert.ToInt32(CompatibleVersion) && ver <= Convert.ToInt32(UpdateVersion);
        }

        #endregion
    }
}
