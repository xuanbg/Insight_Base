using Insight.Base.Common;
using Insight.Base.Common.Utils;
using Insight.WS.Service;

namespace Test
{
    class Program
    {
        private static Services Services;

        static void Main(string[] args)
        {
            var list = DataAccess.GetServiceList();
            Services = new Services();
            foreach (var info in list)
            {
                var service = new ServiceInfo
                {
                    BaseAddress = Util.GetAppSetting("Address"),
                    Port = info.Port ?? Util.GetAppSetting("Port"),
                    Path = info.Path,
                    NameSpace = info.NameSpace,
                    Interface = info.Interface,
                    ComplyType = info.Service,
                    ServiceFile = info.ServiceFile
                };
                Services.CreateHost(service);
            }
            Services.StartService();
        }
    }
}
