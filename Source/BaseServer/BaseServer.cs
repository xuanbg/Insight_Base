using System.ServiceProcess;
using Insight.WCF;

namespace Insight.Base.Server
{
    public partial class BaseServer : ServiceBase
    {
        private static readonly Service services = new Service();

        /// <summary>
        /// 构造方法
        /// </summary>
        public BaseServer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            services.createHosts();
            services.startService();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            services.stopService();
        }
    }
}