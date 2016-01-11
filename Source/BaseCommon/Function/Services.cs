using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Windows.Forms;
using Insight.WS.Base.Common.Entity;
using Microsoft.Samples.GZipEncoder;

namespace Insight.WS.Base.Common
{
    public class Services
    {

        #region 成员属性

        /// <summary>
        /// 服务基地址
        /// </summary>
        private readonly string BaseAddress = Util.GetAppSetting("Address");

        /// <summary>
        /// 服务起始端口
        /// </summary>
        private string Port = Util.GetAppSetting("Port");

        /// <summary>
        /// 服务绑定
        /// </summary>
        private CustomBinding Binding;

        /// <summary>
        /// 启动服务配置列表
        /// </summary>
        private static readonly List<ServiceInfo> List = new List<ServiceInfo>();
 
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造方法，初始化服务配置列表
        /// </summary>
        public Services()
        {
            var names = new[] { "Verify", "Organization" };
            var attains = new[] { "SessionManage", "OrgManger" };
            for (var i = 0; i < names.Length; i++)
            {
                var port = Convert.ToInt32(Port);
                var inter = InitInterface(port + i, names[i], attains[i], i > 0);
                List.Add(inter);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 启动服务主机
        /// </summary>
        /// <returns>ServiceHost List 已启动服务主机集合</returns>
        public List<ServiceHost> StartService()
        {
            var hosts = new List<ServiceHost>();
            foreach (var info in List)
            {
                InitBinding(info.Compress);
                var host = CreateHost(info);
                if (host == null) continue;

                hosts.Add(host);
                var td = new Thread(() => host.Open());
                td.Start();
            }
            return hosts;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化基本HTTP服务绑定
        /// </summary>
        /// <param name="isCompres">是否压缩传输</param>
        private void InitBinding(bool isCompres)
        {
            var encoder = new WebMessageEncodingBindingElement { ReaderQuotas = { MaxArrayLength = 67108864, MaxStringContentLength = 67108864 } };
            var transport = new HttpTransportBindingElement { ManualAddressing = true, MaxReceivedMessageSize = 1073741824, TransferMode = TransferMode.Streamed };
            Binding = new CustomBinding { SendTimeout = TimeSpan.FromSeconds(600), ReceiveTimeout = TimeSpan.FromSeconds(600) };

            if (isCompres)
            {
                encoder.ContentTypeMapper = new TypeMapper();
                var gZipEncode = new GZipMessageEncodingBindingElement(encoder);
                Binding.Elements.AddRange(gZipEncode, transport);
            }
            else
            {
                Binding.Elements.AddRange(encoder, transport);
            }
        }

        /// <summary>
        /// 创建WCF服务主机
        /// </summary>
        /// <param name="info">服务信息</param>
        /// <returns>ServiceHost WCF服务主机</returns>
        private ServiceHost CreateHost(ServiceInfo info)
        {
            string path = $"{Application.StartupPath}\\{info.Path}\\{info.Name}.dll";
            if (!File.Exists(path)) return null;

            var asm = Assembly.LoadFrom(path);
            try
            {
                var address = new Uri($"{BaseAddress}:{info.Port}");
                var host = new ServiceHost(asm.GetType(info.Attain), address);
                var endpoint = host.AddServiceEndpoint(asm.GetType(info.Interface), Binding, info.Name);
                endpoint.Behaviors.Add(new WebHttpBehavior());
                return host;
            }
            catch (Exception ex)
            {
                Util.LogToEvent(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 初始化接口配置
        /// </summary>
        /// <param name="port">服务端口</param>
        /// <param name="name">接口名称</param>
        /// <param name="attain">接口实现类</param>
        /// <param name="compress">是否压缩传输，默认压缩</param>
        /// <returns>InterfaceConfig 接口配置类型</returns>
        private ServiceInfo InitInterface(int port, string name, string attain, bool compress)
        {
            const string space = "Insight.WS.Base";
            return new ServiceInfo
            {
                Port = port.ToString(),
                Name = name,
                Interface = $"{space}.{name}.Interface",
                Attain = $"{space}.{name}.{attain}",
                Path = "Base",
                Compress = compress
            };
        }

        #endregion

    }

}
