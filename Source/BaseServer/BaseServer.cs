using System.ServiceProcess;
using Insight.WCF;

namespace Insight.Base.Server
{
    public partial class BaseServer : ServiceBase
    {
        private static readonly Service _Services = new Service();

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
            _Services.CreateHosts();
            _Services.StartService();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            _Services.StopService();
        }
    }
}