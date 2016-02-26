using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;
using Insight.WS.Service;
using static Insight.WS.Base.Common.Util;

namespace Insight.WS.Base
{
    public partial class BaseServer : ServiceBase
    {
        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private static Services Services;

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
            Services = new Services();
            Services.CreateHost(BaseService());
            Services.CreateHost(VerifyService());
            Services.StartService();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            Services.StopService();
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
            CheckOpenID = bool.Parse(GetAppSetting("CheckOpenID"));

            LogServer = GetAppSetting("LogServer");
        }

        /// <summary>
        /// 初始化基础服务主机
        /// </summary>
        /// <returns></returns>
        private static ServiceInfo BaseService()
        {
            var endpoints = new List<EndpointSet>
            {
                new EndpointSet {Interface = "IOrganizations", Path = "orgs"},
                new EndpointSet {Interface = "IUsers", Path = "users"},
                new EndpointSet {Interface = "IRoles", Path = "roles"},
                new EndpointSet {Interface = "ICodes", Path = "codes"}
            };
            return new ServiceInfo
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("BasePort"),
                ServiceFile = "BaseService.dll",
                NameSpace = "Insight.WS.Base",
                ComplyType = "BaseService",
                Endpoints = endpoints
            };
        }

        /// <summary>
        /// 初始化验证服务主机
        /// </summary>
        /// <returns></returns>
        private static ServiceInfo VerifyService()
        {
            var endpoints = new List<EndpointSet>
            {
                new EndpointSet {Interface = "IVerify"},
            };
            return new ServiceInfo
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("VerifyPort"),
                ServiceFile = "VerifyService.dll",
                NameSpace = "Insight.WS.Base",
                ComplyType = "VerifyService",
                Endpoints = endpoints
            };
        }

    }
}
