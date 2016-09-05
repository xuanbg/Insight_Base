using System;
using System.Linq;
using System.ServiceProcess;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.WCF;
using Insight.WCF.Entity;

namespace Insight.Base.Server
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
                    BaseAddress = Util.GetAppSetting("Address"),
                    Port = info.Port,
                    Path = info.Path,
                    Version = info.Version,
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
            using (var context = new BaseEntities())
            {
                Parameters.Rules = context.SYS_Logs_Rules.ToList();
            }

            Parameters.CheckStamp = bool.Parse(Util.GetAppSetting("CheckStamp"));
            Parameters.AutoExten = bool.Parse(Util.GetAppSetting("AutoExten"));
            Parameters.Expired = Convert.ToInt32(Util.GetAppSetting("Expired"));
        }
    }
}
