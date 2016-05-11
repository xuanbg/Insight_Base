using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;
using Insight.WS.Service;
using ServiceInfo = Insight.WS.Service.ServiceInfo;

namespace Test
{
    class Program
    {
        private static Services Services;

        static void Main(string[] args)
        {

            using (var context = new BaseEntities())
            {
                var obj = context.SYS_User.Single(u => u.LoginName == "Admin");
                obj.LoginName = "Admin";
                var r = context.ChangeTracker.HasChanges();
                obj.LoginName = "admin";
                var t = context.ChangeTracker.HasChanges();
            }

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
