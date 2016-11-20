using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.WCF;

namespace Test
{
    class Program
    {
        private static Service Services;

        static void Main(string[] args)
        {
            using (var context = new BaseEntities())
            {
                Parameters.Rules = context.SYS_Logs_Rules.ToList();
            }
            var list = DataAccess.GetServiceList();
            Services = new Service();
            foreach (var info in list)
            {
                var service = new Service.Info
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
