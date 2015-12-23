using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceProcess;
using Insight.WS.Server.Common;

namespace Insight.WS.Server
{
    public partial class InsightServer : ServiceBase
    {

        #region 成员属性

        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private List<ServiceHost> Hosts { get; } = new List<ServiceHost>();

        #endregion

        #region 构造函数

        public InsightServer()
        {
            InitializeComponent();
        }

        #endregion

        #region 服务行为

        protected override void OnStart(string[] args)
        {
            // 启动WCF服务主机
            var comp = bool.Parse(Util.GetAppSetting("IsCompres"));
            var address = Util.GetAppSetting("Address");
            var tcpService = new Services
            {
                BaseAddress = $"net.tcp://{address}:"
            };
            tcpService.InitTcpBinding(comp);
            Hosts.AddRange(tcpService.StartService("TCP", !comp));

            var httpService = new Services
            {
                BaseAddress = $"http://{address}:"
            };
            httpService.InitHttpBinding(comp);
            Hosts.AddRange(httpService.StartService("HTTP", !comp));
        }

        protected override void OnStop()
        {
            foreach (var host in Hosts)
            {
                host.Abort();
                host.Close();
            }
        }

        #endregion

    }
}
