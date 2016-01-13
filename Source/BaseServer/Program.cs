using System.ServiceProcess;

namespace Insight.WS.Base
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
                new BaseServer() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
