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
            Services = new Service();
            Services.CreateHosts(Util.GetAppSetting("Address"));
            Services.StartService();
        }
    }
}