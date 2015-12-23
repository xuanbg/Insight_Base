using System.ServiceProcess;
using static Insight.WS.Server.Util;

namespace Insight.WS.Server
{
    public partial class VerifyServer : ServiceBase
    {

        #region 构造函数

        public VerifyServer()
        {
            InitializeComponent();
        }

        #endregion

        #region 服务行为

        protected override void OnStart(string[] args)
        {
            CreateHost();
            Host.Open();
        }

        protected override void OnStop()
        {
            Host.Abort();
            Host.Close();
        }

        #endregion

    }
}
