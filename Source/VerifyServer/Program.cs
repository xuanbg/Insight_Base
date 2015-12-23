using System.ServiceProcess;

namespace Insight.WS.Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new VerifyServer() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
