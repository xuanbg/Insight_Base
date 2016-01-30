using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceProcess;
using System.Windows.Forms;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base
{
    public partial class BaseServer : ServiceBase
    {
        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private static List<ServiceHost> Hosts;

        #region 构造函数

        /// <summary>
        /// 构造方法，初始化服务控件
        /// </summary>
        public BaseServer()
        {
            InitializeComponent();
            InitSeting();
        }

        #endregion

        #region 服务行为

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            Hosts = new List<ServiceHost>
            {
                BaseService(),
                VerifyService()
            };
            foreach (var host in Hosts)
            {
                host.Open();
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            foreach (var host in Hosts)
            {
                host.Abort();
                host.Close();
            }
        }

        #endregion

        /// <summary>
        /// 初始化环境变量
        /// </summary>
        private static void InitSeting()
        {
            var version = new Version(Application.ProductVersion);
            var build = $"{version.Major}{version.Minor}{version.Build.ToString("D4").Substring(0, 2)}";
            CurrentVersion = Convert.ToInt32(build);
            CompatibleVersion = GetAppSetting("CompatibleVersion");
            UpdateVersion = GetAppSetting("UpdateVersion");

            LogServer = GetAppSetting("LogServer");
        }

        /// <summary>
        /// 初始化基础服务主机
        /// </summary>
        /// <returns></returns>
        private static ServiceHost BaseService()
        {
            string path = $"{Application.StartupPath}\\BaseService.dll";
            if (!File.Exists(path)) return null;

            var endpoints = new List<EndpointSet>
            {
                new EndpointSet {Name = "IOrganizations", Path = "orgs"},
                new EndpointSet {Name = "IUsers", Path = "users"},
                new EndpointSet {Name = "IRoles", Path = "roles"},
                new EndpointSet {Name = "ICodes", Path = "codes"}
            };
            var serv = new Services
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("BasePort"),
                NameSpace = "Insight.WS.Base",
                ServiceType = "BaseService",
                Endpoints = endpoints
            };
            return serv.CreateHost(path);
        }

        /// <summary>
        /// 初始化验证服务主机
        /// </summary>
        /// <returns></returns>
        private static ServiceHost VerifyService()
        {
            string path = $"{Application.StartupPath}\\VerifyService.dll";
            if (!File.Exists(path)) return null;

            var endpoints = new List<EndpointSet>
            {
                new EndpointSet {Name = "IVerify"},
            };
            var serv = new Services
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("VerifyPort"),
                NameSpace = "Insight.WS.Base",
                ServiceType = "VerifyService",
                Endpoints = endpoints
            };
            return serv.CreateHost(path);
        }

    }
}
