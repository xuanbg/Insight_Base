using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
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
        /// 根据用户账号获取用户Session
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <returns>Session</returns>
        public static Session GetSession(Session obj)
        {
            // 先在缓存中根据用户账号查找，找到即返回结果
            var session = Sessions.SingleOrDefault(s => string.Equals(s.LoginName, obj.LoginName, StringComparison.CurrentCultureIgnoreCase));
            if (session != null) return session;

            // 未在缓存中找到结果时，再在数据库中根据用户账号查找用户；
            // 找到后根据用户信息初始化Session数据、加入缓存并返回结果，否则返回null。
            var user = DataAccess.GetUser(obj.LoginName);
            if (user == null) return null;

            session = new Session
            {
                ID = Sessions.Count,
                UserId = user.ID,
                UserName = user.Name,
                OpenId = user.OpenId,
                LoginName = user.LoginName,
                Signature = Hash(user.LoginName.ToUpper() + user.Password),
                UserType = user.Type,
                Validity = user.Validity,
                MachineId = obj.MachineId
            };
            Sessions.Add(session);
            return session;
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public static T GetAuthorization<T>()
        {
            var woc = WebOperationContext.Current;
            var headers = woc.IncomingRequest.Headers;
            if (!CompareVersion(headers))
            {
                woc.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
                return default(T);
            }

            var auth = headers[HttpRequestHeader.Authorization];
            if (string.IsNullOrEmpty(auth))
            {
                woc.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return default(T);
            }

            var type = headers[HttpRequestHeader.ContentType];
            if (type == "application/x-gzip") SetResponseParam();

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
        /// <param name="source">事件源（默认Insight VerifyServer Service）</param>
        public static void LogToEvent(string msg, EventLogEntryType type = EventLogEntryType.Error, string source = "Insight Base Service")
        {
            EventLog.WriteEntry(source, msg, type);
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
        /// 验证版本是否兼容
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static bool CompareVersion(WebHeaderCollection headers)
        {
            var accept = headers[HttpRequestHeader.Accept];
            if (accept == null) return false;

            var val = accept.Split(Convert.ToChar(";"));
            if (accept.Length < 2) return false;

            var ver = Convert.ToInt32(val[1].Substring(9));
            return ver >= Convert.ToInt32(CompatibleVersion) && ver <= Convert.ToInt32(UpdateVersion);
        }

        #endregion
    }


}
