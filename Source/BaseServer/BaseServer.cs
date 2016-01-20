using System.IO;
using System.ServiceModel;
using System.ServiceProcess;
using System.Windows.Forms;

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

            Host = Services.CreateHost(path);
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

    }
}
