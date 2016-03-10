using System;
using System.ServiceProcess;
using System.Windows.Forms;
using Insight.WS.Base.Common;
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
            var list = DataAccess.GetServiceList();
            Services = new Services();
            foreach (var info in list)
            {
                var service = new ServiceInfo
                {
                    BaseAddress = GetAppSetting("Address"),
                    Port = info.Port ?? GetAppSetting("Port"),
                    Path = info.Path,
                    NameSpace = info.NameSpace,
                    Interface = info.Interface,
                    ComplyType = info.Service,
                    ServiceFile = info.ServiceFile
                };
                Services.CreateHost(service);
            }
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

            LogServer = GetAppSetting("LogServer");
            CheckOpenID = bool.Parse(GetAppSetting("CheckOpenID"));
            CheckMachineId = bool.Parse(GetAppSetting("CheckMachineId"));
            Expired = Convert.ToInt32(GetAppSetting("Expired"));
        }

    }
}
