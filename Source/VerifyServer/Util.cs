using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace Insight.WS.Server
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
        /// 系统连结字符串字典
        /// </summary>
        public static Dictionary<string, string> ConStr;

        #endregion

        #region 静态构造方法

        /// <summary>
        /// 静态构造函数，初始化系统连结字符串字典
        /// </summary>
        static Util()
        {
            var list = ConfigurationManager.ConnectionStrings;
            ConStr = new Dictionary<string, string> { { "Template", null } };
            for (var i = 0; i < list.Count; i++)
            {
                var name = list[i].Name;
                if (name.Contains("Local")) continue;

                ConStr.Add(name, new Entities(name).ConnectionString);
            }
        }

        #endregion

        #region 静态公共方法

        public static void CreateHost()
        {
            var address = new Uri(GetAppSetting("Address"));
            var binding = new NetTcpBinding();
            Host = new ServiceHost(typeof(OnlineManage), address);
            Host.AddServiceEndpoint(typeof(Interface), binding, "VerifyServer");

            // 添加元数据服务
            //var behavior = new ServiceMetadataBehavior();
            //var ExchangeBindings = MetadataExchangeBindings.CreateMexTcpBinding();
            //Host.Description.Behaviors.Add(behavior);
            //Host.AddServiceEndpoint(typeof(IMetadataExchange), ExchangeBindings, "VerifyServer/mex");
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
        public static string GetHash(string str)
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
        public static void LogToEvent(string msg, EventLogEntryType type = EventLogEntryType.Error, string source = "Insight VerifyServer Service")
        {
            EventLog.WriteEntry(source, msg, type);
        }

        /// <summary>
        /// 返回第一行第一列内容的方法
        /// </summary>
        /// <param name="cmd">SqlCommand</param>
        /// <param name="dataSouc">数据源名称</param>
        /// <returns>执行SQL语句后的第一行第一列内容</returns>
        public static object SqlScalar(SqlCommand cmd, string dataSouc = "WSEntities")
        {
            using (var conn = new SqlConnection(ConStr[dataSouc]))
            {
                conn.Open();
                cmd.Connection = conn;
                try
                {
                    return cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    LogToEvent(cmd.CommandText);
                    LogToEvent(ex.ToString());
                    return null;
                }
            }
        }

        /// <summary>
        /// 组装SqlCommand对象
        /// </summary>
        /// <param name="sql">sql命令</param>
        /// <param name="parameters">可变长参数组</param>
        /// <returns>SqlCommand 组装完成的SqlCommand对象</returns>
        public static SqlCommand MakeCommand(string sql, params SqlParameter[] parameters)
        {
            var cmd = new SqlCommand(sql);
            foreach (var p in parameters)
            {
                if (p.Value == null || p.Value.ToString() == "")
                {
                    p.Value = DBNull.Value;
                }
                cmd.Parameters.Add(p);
            }
            return cmd;
        }

    }

    #endregion

    #region 类型定义

    public class Entities : DbContext
    {

        /// <summary>
        /// 数据库连结字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 带连接字符串名称的构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        public Entities(string connectionString) : this(connectionString, false)
        {
            ConnectionString = Database.Connection.ConnectionString;
        }

        public Entities(string connectionString, bool proxyCreationEnabled) : base(connectionString)
        {
            Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        }

    }

    public class Session
    {

        /// <summary>
        /// 自增ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 登录用户ID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 用户账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户签名，用户名（大写）+ 密码MD5值的结果的MD5值
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 登录部门ID
        /// </summary>
        public Guid? DeptId { get; set; }

        /// <summary>
        /// 登录部门全称
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public bool Validity { get; set; }

        /// <summary>
        /// 客户端软件版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 客户端类型，0、Desktop；1、Browser；2、iOS；3、Android；4、WindowsPhone；5、Other
        /// </summary>
        public int ClientType { get; set; }

        /// <summary>
        /// 用户机器码
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 上次连接时间
        /// </summary>
        public DateTime LastConnect { get; set; }

        /// <summary>
        /// 用户登录结果
        /// </summary>
        public LoginResult LoginResult { get; set; }

        /// <summary>
        /// 用户在线状态
        /// </summary>
        public bool OnlineStatus { get; set; }

        /// <summary>
        /// WCF服务基地址
        /// </summary>
        public string BaseAddress { get; set; }

    }

    /// <summary>
    /// 用户登录结果
    /// </summary>
    public enum LoginResult
    {
        Success,
        Multiple,
        Online,
        Failure,
        Banned,
        NotExist,
        Unauthorized
    }

    #endregion

}
