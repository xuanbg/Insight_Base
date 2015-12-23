using System;
using System.Collections.Generic;
using System.Text;
using static Insight.WS.Test.Interface.Util;

namespace Insight.WS.Test.Interface
{
    static class Program
    {
        //private const string BassAddress = "http://localhost:6280/Interface/";
        private const string BassAddress = "http://120.27.142.125:6280/Interface/";

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Util.Session = Register();
            UserSession = Login();


            Logout();
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        private static Session Register()
        {
            var url = BassAddress + "user";
            var mobile = "18600740256";
            var session = new Session
            {
                LoginName = mobile,
                UserName = "张三",
                Signature = Hash(mobile + "123456" + Hash("111111")),
                Version = 10000,
                ClientType = 2,
                MachineId = Hash("MachineId")
            };
            var json = Serialize(session);
            var buff = Encoding.UTF8.GetBytes(json);
            var author = Convert.ToBase64String(buff);
            var dict = new Dictionary<string, string> {{"smsCode", "123456"}, {"password", Hash("111111")}};
            var data = Serialize(dict);
            var result = HttpRequest(url, "PUT", author, data);
            return !result.Successful ? null : Deserialize<Session>(result.Data);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        private static Session Login()
        {
            var mobile = "18600740256";
            var password = Hash("111111");
            var url = BassAddress + "user/signin";
            var us = new Session
            {
                LoginName = mobile,
                Signature = Hash(mobile.ToUpper() + password),
                Version = 10000,
                ClientType = 2,
                MachineId = Hash("MachineId")
            };
            var data = Serialize(us);
            var result = HttpRequest(url, "POST", null, data);
            if (result.Successful)
            {
                var session = Deserialize<Session>(result.Data);
                Console.Write($"用户 {mobile} 登录结果：{session.LoginResult}");
                Console.ReadLine();
                return session;
            }

            Console.Write(result.Message);
            Console.ReadLine();
            return null;
        }

        /// <summary>
        /// 注销
        /// </summary>
        private static void Logout()
        {
            var url = BassAddress + "user/signout";
            var data = Serialize(UserSession.ID);
            var result = HttpRequest(url, "POST", "", data);
            if (result.Successful)
            {
                Console.Write($"用户 {UserSession.LoginName} 注销成功");
                Console.ReadLine();
            }
            else
            {
                Console.Write(result.Message);
                Console.ReadLine();
            }
        }

    }

}
