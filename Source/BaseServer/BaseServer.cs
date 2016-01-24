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
        private static ServiceHost Host;

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
            string path = $"{Application.StartupPath}\\BaseService.dll";
            if (!File.Exists(path)) return;

            var endpoints = new List<EndpointSet>
            {
                new EndpointSet { Name = "IVerify", Path = "verify" },
                new EndpointSet { Name = "IUsers", Path = "users" },
                new EndpointSet { Name = "IOrganizations", Path = "organizations" },
                new EndpointSet { Name = "IRoles", Path = "roles" }
            };
            var serv = new Services
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("Port"),
                NameSpace = "Insight.WS.Base.Service",
                ServiceType = "BaseService",
                Endpoints = endpoints
            };
            Host = serv.CreateHost(path);
            Host.Open();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            Host.Abort();
            Host.Close();
        }

        #endregion

        /// <summary>
        /// 初始化环境变量
        /// </summary>
        public static void InitSeting()
        {
            var version = new Version(Application.ProductVersion);
            var build = $"{version.Major}{version.Minor}{version.Build.ToString("D4").Substring(0, 2)}";
            CurrentVersion = Convert.ToInt32(build);
            CompatibleVersion = GetAppSetting("CompatibleVersion");
            UpdateVersion = GetAppSetting("UpdateVersion");

            LogServer = GetAppSetting("LogServer");
        }

    }
}
