using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace Insight.WS.Verify
{
    public static class Util
    {

        #region 静态字段

        /// <summary>
        /// 在线用户列表
        /// </summary>
        public static List<Session> Sessions = new List<Session>();

        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        public static ServiceHost Host;

        /// <summary>
        /// 短信验证码的缓存列表
        /// </summary>
        public static readonly List<VerifyRecord> SmsCodes = new List<VerifyRecord>();

        /// <summary>
        /// 用于生成短信验证码的随机数发生器
        /// </summary>
        public static readonly Random Random = new Random(Environment.TickCount);

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
            var user = GetUser(obj.LoginName);
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
                Version = obj.Version,
                ClientType = obj.ClientType,
                MachineId = obj.MachineId,
                BaseAddress = obj.BaseAddress
            };
            Sessions.Add(session);
            return session;
        }

        public static void CreateHost()
        {
            var address = new Uri(GetAppSetting("Address"));
            var binding = new NetTcpBinding();
            Host = new ServiceHost(typeof(SessionManage), address);
            Host.AddServiceEndpoint(typeof(Interface), binding, "VerifyServer");
            if (GetAppSetting("Mode") != "debug") return;

            // 添加元数据服务
            var behavior = new ServiceMetadataBehavior();
            var ExchangeBindings = MetadataExchangeBindings.CreateMexTcpBinding();
            Host.Description.Behaviors.Add(behavior);
            Host.AddServiceEndpoint(typeof(IMetadataExchange), ExchangeBindings, "VerifyServer/mex");
        }

        /// <summary>
        /// 根据用户登录名获取用户对象实体
        /// </summary>
        /// <param name="str">用户登录名</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(string str)
        {
            using (var context = new WSEntities())
            {
                return context.SYS_User.SingleOrDefault(s => s.LoginName == str);
            }
        }

        /// <summary>
        /// 根据用户ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(Guid id)
        {
            using (var context = new WSEntities())
            {
                return context.SYS_User.SingleOrDefault(u => u.ID == id);
            }
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="obj">用于会话</param>
        /// <param name="id">操作ID</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static bool Authority(Session obj, Guid id)
        {
            using (var context = new WSEntities())
            {
                return context.Authority(obj.UserId, obj.DeptId, id).Any();
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
        public static void LogToEvent(string msg, EventLogEntryType type = EventLogEntryType.Error, string source = "Insight VerifyServer Service")
        {
            EventLog.WriteEntry(source, msg, type);
        }

    }

    #endregion

}
