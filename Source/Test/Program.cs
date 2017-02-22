using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.WCF;

namespace Test
{
    class Program
    {
        private static Service Services;

        static void Main(string[] args)
        {
            var list = new Dictionary<string, Session>();
            for (var i = 0; i < 10000000; i++)
            {
                list.Add(Guid.NewGuid().ToString(), new Session(""));
                if (i % 10000 == 0) Console.WriteLine(i.ToString());
            }
            Console.ReadLine();
            Services = new Service();
            Services.CreateHosts(Util.GetAppSetting("Address"));
            Services.StartService();
        }
    }
}