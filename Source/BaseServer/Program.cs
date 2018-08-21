using System.ServiceProcess;

namespace Insight.Base.Server
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        // ReSharper disable once InconsistentNaming
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
