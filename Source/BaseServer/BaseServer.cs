using System;
using System.ServiceProcess;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using static Insight.Base.Common.Utils.Util;

namespace Insight.Base
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
            var ver = GetAppSetting("Version");
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
                Services.CreateHost(service, ver);
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
            LogServer = GetAppSetting("LogServer");
            CheckOpenID = bool.Parse(GetAppSetting("CheckOpenID"));
            CheckMachineId = bool.Parse(GetAppSetting("CheckMachineId"));
            Expired = Convert.ToInt32(GetAppSetting("Expired"));
        }

    }
}
