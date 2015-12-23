using System;
using System.Collections.Generic;
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
using Insight.WS.Server.Common.Service;

namespace Insight.WS.Server.Common
{
    
    public class Util
    {

        public static Session Session;
        public static bool Compres = bool.Parse(GetAppSetting("IsCompres"));

        /// <summary>
        /// 构建用于接口返回值的Json对象
        /// </summary>
        /// <typeparam name="T">传入的对象类型</typeparam>
        /// <param name="obj">传入的对象</param>
        /// <param name="code">错误代码</param>
        /// <param name="name"></param>
        /// <param name="message">错误消息</param>
        /// <returns>JsonResult</returns>
        public static JsonResult GetJson<T>(T obj, string code = "404", string name = "ResourceNotFound", string message = "未能读取任何数据")
        {
            SetResponseParam();
            var result = new JsonResult { Code = code, Name = name, Message = message };
            if (obj != null)
            {
                result.Successful = true;
                result.Code = "200";
                result.Name = "OK";
                result.Message = "接口调用成功";
                result.Data = Serialize(obj);
            }
            return result;
        }

        /// <summary>
        /// 构建用于接口返回值的Json对象
        /// </summary>
        /// <typeparam name="T">传入的集合的对象类型</typeparam>
        /// <param name="objs">传入的对象集合</param>
        /// <param name="code">错误代码</param>
        /// <param name="name"></param>
        /// <param name="message">错误消息</param>
        /// <returns>JsonResult</returns>
        public static JsonResult GetJson<T>(List<T> objs, string code = "404", string name = "ResourceNotFound", string message = "未能读取任何数据")
        {
            SetResponseParam();
            var result = new JsonResult { Code = code, Name = name, Message = message};
            if (objs.Count > 0)
            {
                result.Successful = true;
                result.Code = "200";
                result.Name = "OK";
                result.Message = "接口调用成功";
                result.Data = Serialize(objs);
            }
            return result;
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
        /// 保存图片到image文件夹，通过URL访问
        /// </summary>
        /// <param name="pic">图片字节流</param>
        /// <param name="catalog">分类</param>
        /// <param name="name">文件名（可选）</param>
        /// <returns>string 图片保存路径</returns>
        public static string SaveImage(byte[] pic, string catalog, string name = null)
        {
            var path = $"{GetAppSetting("ImageLocal")}\\{catalog}\\";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (name == null)
                {
                    var img = Image.FromStream(new MemoryStream(pic));
                    name = $"{Guid.NewGuid()}.{GetImageExtension(img)}";
                }

                File.WriteAllBytes(path + name, pic);
                return $"/{catalog}/{name}";
            }
            catch (Exception ex)
            {
                LogToEvent(ex.ToString());
                LogToEvent($"文件写入路径：{path}；字节流：{pic}");
                return null;
            }
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
            request.ContentType = Compres ? "application/x-gzip" : "application/json";
            if (author == "")
            {
                var json = Serialize(Session);
                var buff = Encoding.UTF8.GetBytes(json);
                author = Convert.ToBase64String(buff);
            }

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
        private static T GetResponse<T>(WebRequest request)
        {
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            if (responseStream == null)
            {
                responseStream.Close();
                return default(T);
            }

            var encoding = response.Headers["Content-Encoding"];
            if (encoding != null && encoding.ToLower().Contains("gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }

            using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
            {
                var result = reader.ReadToEnd();
                responseStream.Close();
                return Deserialize<T>(result);
            }
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
        /// 将一个集合序列化为Json字符串
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="objs">集合</param>
        /// <returns>string Json字符串</returns>
        public static string Serialize<T>(List<T> objs)
        {
            return new JavaScriptSerializer().Serialize(objs);
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
        /// 将一个Json字符串反序列化为指定类型的对象集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <returns>List/<T/> 反序列化的对象集合</returns>
        public static List<T> DeserializeToList<T>(string json)
        {
            return new JavaScriptSerializer().Deserialize<List<T>>(json);
        }

        /// <summary>
        /// 将一个对象序列化为XML字符串
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>序列化产生的XML字符串</returns>
        public static string Serialize(object o, Encoding encoding)
        {
            using (var stream = new MemoryStream())
            {
                SerializeInternal(stream, o, encoding);

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
        /// <param name="s">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T Deserialize<T>(string s, Encoding encoding)
        {
            using (var ms = new MemoryStream(encoding.GetBytes(s)))
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
        /// <param name="o"></param>
        /// <param name="encoding"></param>
        private static void SerializeInternal(Stream stream, object o, Encoding encoding)
        {
            var serializer = new XmlSerializer(o.GetType());
            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineChars = "\r\n",
                Encoding = encoding,
                IndentChars = "    "
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, o);
            }
        }
    }

}
