using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Insight.WS.Base.Common;

namespace Insight.WS.Base.Users
{
    public class XFBInterface
    {
        /// <summary>
        /// 登录信分宝
        /// </summary>
        /// <param name="loginname">登录账号</param>
        /// <param name="password">登录密码</param>
        /// <returns>JsonLogin</returns>
        public static JsonLogin DoLogin(string loginname, string password)
        {
            var url = Util.GetAppSetting("Interface") + "customer/sslogin.do";
            var data = $"loginName={loginname}&passWord={password}";
            return HttpRequest<JsonLogin>(url, "POST", data, "123", password);
        }

        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <param name="loginname">登录账号</param>
        /// <param name="password">新登录密码</param>
        /// <param name="old">旧登录密码</param>
        /// <returns>JsonGeneral</returns>
        public static JsonGeneral ChangXFBPassword(string loginname, string password, string old)
        {
            var userid = DoLogin(loginname, old)?.userId;
            var url = Util.GetAppSetting("Interface") + "customer/changeInfo.do";
            var data = $"loginName={loginname}&newPassWord={password}&oldPassword={old}";
            return HttpRequest<JsonGeneral>(url, "POST", data, userid, old);
        }

        /// <summary>
        /// HttpRequest方法
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="data">接口参数</param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <returns>JsonResult</returns>
        private static T HttpRequest<T>(string url, string method, string data, string userid, string password)
        {
            var request = GetWebRequest(url, method, userid, password);
            if (method == "GET") return GetResponse<T>(request);

            var buffer = Encoding.UTF8.GetBytes(data);
            request.ContentLength = buffer.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            return GetResponse<T>(request);
        }

        /// <summary>
        /// 获取Request响应数据
        /// </summary>
        /// <param name="request">WebRequest</param>
        /// <returns>JsonResult</returns>
        private static T GetResponse<T>(WebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return default(T);

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
                {
                    var result = reader.ReadToEnd();
                    responseStream.Close();
                    return Util.Deserialize<T>(result);
                }
            }
            catch (Exception ex)
            {
                Util.LogToEvent(ex.ToString());
                return default(T);
            }
        }

        /// <summary>
        /// 获取WebRequest对象
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <returns>HttpWebRequest</returns>
        private static HttpWebRequest GetWebRequest(string url, string method, string userid, string password)
        {
            var tick = (int)DateTime.Now.Subtract(DateTime.Parse("1970-1-1")).TotalSeconds;
            var secretkey = Util.Hash($"{tick}passw0rd!{userid}");
            var header = new WebHeaderCollection
            {
                {"userId", userid},
                {"password", password},
                {"authentication", $"timestamp={tick};secretKey={secretkey}"},
                {"sysType", "IOS"},
                {"sysVersion", "V1.2.0"},
                {"uuid", "A6872607-3614-4361-8B10-319D66114309"},
                {"OSversion", "8.4.1"},
                {"phoneModel", "iPhone 6 (A1549/A1586)"},
                {"carrierName", "CU"}
            };
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add(header);

            return request;
        }

    }

    public class JsonGeneral
    {
        public string resultCode { get; set; }
        public string resultMessage { get; set; }
    }

    public class JsonLogin
    {
        public string resultCode { get; set; }
        public string resultMessage { get; set; }
        public string userId { get; set; }
        public Customer customer { get; set; }
    }

    public class Customer
    {
        public string customerId { get; set; }
        public string openNick { get; set; }
        public string openId { get; set; }
        public string openType { get; set; }
        public string userId { get; set; }
        public string loginName { get; set; }
        public string realName { get; set; }
        public string recomCode { get; set; }
        public string recomUser { get; set; }
        public string cardId { get; set; }
        public string regiestTime { get; set; }
        public string lastLogIntegerime { get; set; }
        public string userToken { get; set; }
        public string recomNum { get; set; }
        public string userType { get; set; }
    }
}
