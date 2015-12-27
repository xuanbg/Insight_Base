using System.ServiceProcess;
using static Insight.WS.Verify.Util;

namespace Insight.WS.Verify
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
