using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceProcess;
using Insight.WS.Base.Common;

namespace Insight.WS.Base
{
    public partial class BaseServer : ServiceBase
    {
        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private static readonly List<ServiceHost> Hosts = new List<ServiceHost>();

        #region 构造函数

        /// <summary>
        /// 构造方法，初始化服务控件
        /// </summary>
        public BaseServer()
        {
            InitializeComponent();
        }

        #endregion

        #region 服务行为

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            var httpService = new Services();
            Hosts.AddRange(httpService.StartService());
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

    }
}
