using System.ServiceProcess;
using Insight.Base.Common;
using Insight.Utils.Common;
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
            foreach (var info in Parameters.Services)
            {
                var service = new Service.Info
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
                _Services.CreateHost(service);
            }
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