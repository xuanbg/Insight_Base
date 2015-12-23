using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Insight.WS.Server.Common;

namespace Test.Services
{
    class Program
    {
        private static List<ServiceHost> Hosts { get; } = new List<ServiceHost>();

        static void Main(string[] args)
        {
            var comp = bool.Parse(Util.GetAppSetting("IsCompres"));
            var address = Util.GetAppSetting("Address");
            var tcpService = new Insight.WS.Server.Common.Services
            {
                BaseAddress = $"net.tcp://{address}:"
            };
            tcpService.InitTcpBinding(comp);
            Hosts.AddRange(tcpService.StartService("TCP", !comp));

            var httpService = new Insight.WS.Server.Common.Services
            {
                BaseAddress = $"http://{address}:"
            };
            httpService.InitHttpBinding(comp);
            Hosts.AddRange(httpService.StartService("HTTP", !comp));
            //Atom.IntrefaceSync();

            //var r = new Mall();

        }
    }
}
