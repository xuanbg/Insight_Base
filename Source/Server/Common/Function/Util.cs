using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Insight.WS.Server.Common
{
    
    public class Util
    {

        #region 静态全局变量

        public const string Secret = "842A381C91CE43A98720825601C22A56";
        public static bool Compres = bool.Parse(GetAppSetting("IsCompres"));

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

        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string Serialize(object obj, Encoding encoding)
        {
            using (var stream = new MemoryStream())
            {
                SerializeInternal(stream, obj, encoding);

                stream.Position = 0;
                using (var reader = new StreamReader(stream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="xml">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T Deserialize<T>(string xml, Encoding encoding)
        {
            using (var ms = new MemoryStream(encoding.GetBytes(xml)))
            {
                using (var sr = new StreamReader(ms, encoding))
                {
                    var mySerializer = new XmlSerializer(typeof(T));
                    return (T)mySerializer.Deserialize(sr);
                }
            }
        }

        /// <summary>
        /// 序列化预处理
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        /// <param name="encoding"></param>
        private static void SerializeInternal(Stream stream, object obj, Encoding encoding)
        {
            var serializer = new XmlSerializer(obj.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineChars = "\r\n",
                Encoding = encoding,
                IndentChars = "    "
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, obj);
            }
        }

        #endregion

        #region HttpRequest

        /// <summary>
        /// HttpRequest方法
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <param name="data">接口参数</param>
        /// <returns>指定类型的数据对象</returns>
        public static T HttpRequest<T>(string url, string method, string author = "", string data = "")
        {
            if (method == "GET") url += (data == "" ? "" : "?") + data;
            var request = GetWebRequest(url, method, author);

            if (method != "GET")
            {
                var buffer = Compres ? Compress(Encoding.UTF8.GetBytes(data)) : Encoding.UTF8.GetBytes(data);
                request.ContentLength = buffer.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }

            return GetResponse<T>(request);
        }

        /// <summary>
        /// 获取WebRequest对象
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <returns>HttpWebRequest</returns>
        public static HttpWebRequest GetWebRequest(string url, string method, string author)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = Compres ? "application/x-gzip" : "application/json";
            if (author != null)
            {
                request.Headers.Add(HttpRequestHeader.Authorization, author);
            }

            return request;
        }

        /// <summary>
        /// 获取Request响应数据，并转换为指定的类型
        /// </summary>
        /// <typeparam name="T">要转换的类型</typeparam>
        /// <param name="request">WebRequest对象</param>
        /// <returns>T 指定类型的对象</returns>
        public static T GetResponse<T>(WebRequest request)
        {
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                responseStream.Close();
                return default(T);
            }

            var encoding = response.Headers[HttpResponseHeader.ContentEncoding];
            if (encoding != null && encoding.ToLower().Contains("gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }

            using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
            {
                var result = reader.ReadToEnd();
                responseStream.Close();

                var type = response.Headers[HttpResponseHeader.ContentType];
                switch (type)
                {
                    case "application/json":
                        return Deserialize<T>(result);

                    case "application/xml":
                        return Deserialize<T>(result, Encoding.UTF8);

                    default:
                        return (T) Convert.ChangeType(result, typeof (string));
                }
            }
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public static T GetAuthorization<T>()
        {
            var woc = WebOperationContext.Current;
            var auth = woc.IncomingRequest.Headers[HttpRequestHeader.Authorization];
            if (string.IsNullOrEmpty(auth))
            {
                woc.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return default(T);
            }

            SetResponseParam();
            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                return Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                LogToEvent(ex.ToString());
                woc.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return default(T);
            }
        }

        /// <summary>
        /// 在启用Gzip压缩时设置Response参数
        /// </summary>
        public static void SetResponseParam()
        {
            if (!Compres) return;

            var response = WebOperationContext.Current.OutgoingResponse;
            response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            response.ContentType = "application/x-gzip";
        }

        #endregion

        #region 常用静态方法

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
        /// <param name="source">事件源（默认Insight Workstation 3 Service）</param>
        public static void LogToEvent(string msg, EventLogEntryType type = EventLogEntryType.Error, string source = "Insight Workstation 3 Service")
        {
            EventLog.WriteEntry(source, msg, type);
        }

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

        /// <summary>
        /// 获取图片格式名称
        /// </summary>
        /// <param name="img">图片</param>
        /// <returns>string 图片格式</returns>
        public static string GetImageExtension(Image img)
        {
            var list = typeof(ImageFormat).GetProperties(BindingFlags.Static | BindingFlags.Public);
            var name = from n in list
                let format = (ImageFormat) n.GetValue(null, null)
                where format.Guid.Equals(img.RawFormat.Guid)
                select n;
            var names = name.ToList();
            return names.Count > 0 ? names[0].Name : "unknown";
        }

        /// <summary>
        /// 计算输入日期后的特定日期
        /// </summary>
        /// <param name="date">日期</param>
        /// <param name="n">特定日期（0-9）</param>
        /// <returns>DateTime 特定日期</returns>
        public static DateTime FormatDate(DateTime date, int n)
        {
            var day = date.Day;
            var mod = day % 10;
            day = (day - mod + n + (mod > n ? 10 : 0)) % 30;

            return DateTime.Parse(date.ToString("yyyy-MM-dd").Substring(0, 8) + day.ToString("00")).AddMonths(date.Day > 20 + n ? 1 : 0);
        }

        #endregion

    }
}
