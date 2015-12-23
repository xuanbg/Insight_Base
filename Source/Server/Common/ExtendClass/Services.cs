using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.Windows.Forms;
using Insight.WS.Server.Common.ORM;
using Microsoft.Samples.GZipEncoder;

namespace Insight.WS.Server.Common
{
    public class Services
    {

        #region 成员属性

        /// <summary>
        /// 服务基地址
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// 服务绑定
        /// </summary>
        private dynamic Binding;

        /// <summary>
        /// 元数据交换绑定
        /// </summary>
        private dynamic ExchangeBindings;

        /// <summary>
        /// 最大接收消息大小（字节）
        /// </summary>
        private const int MaxReceivedMessageSize = 1073741824;

        /// <summary>
        /// 最大数组长度
        /// </summary>
        private const int MaxArrayLength = 67108864;

        /// <summary>
        /// 最大字符串长度
        /// </summary>
        private const int MaxStringContentLength = 67108864;

        /// <summary>
        /// 发送超时（秒）
        /// </summary>
        private const int SendTimeout = 600;

        /// <summary>
        /// 接收超时（秒）
        /// </summary>
        private const int ReceiveTimeout = 600;

        #endregion

        #region 构造函数

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化自定义服务绑定
        /// </summary>
        /// <param name="isCompres"></param>
        public void InitTcpBinding(bool isCompres)
        {
            var encoder = new BinaryMessageEncodingBindingElement { ReaderQuotas = { MaxArrayLength = MaxArrayLength, MaxStringContentLength = MaxStringContentLength } };
            var transport = new TcpTransportBindingElement { MaxReceivedMessageSize = MaxReceivedMessageSize, TransferMode = TransferMode.Streamed };
            Binding = new CustomBinding { SendTimeout = TimeSpan.FromSeconds(SendTimeout), ReceiveTimeout = TimeSpan.FromSeconds(ReceiveTimeout) };
            ExchangeBindings = MetadataExchangeBindings.CreateMexTcpBinding();
            if (isCompres)
            {
                var gZipEncode = new GZipMessageEncodingBindingElement(encoder);
                Binding.Elements.AddRange(gZipEncode, transport);
            }
            else
            {
                Binding.Elements.AddRange(encoder, transport);
            }
        }

        /// <summary>
        /// 初始化基本HTTP服务绑定
        /// </summary>
        /// <param name="isCompres"></param>
        public void InitHttpBinding(bool isCompres)
        {
            var encoder = new WebMessageEncodingBindingElement { ReaderQuotas = { MaxArrayLength = MaxArrayLength, MaxStringContentLength = MaxStringContentLength } };
            var transport = new HttpTransportBindingElement { ManualAddressing = true, MaxReceivedMessageSize = MaxReceivedMessageSize, TransferMode = TransferMode.Streamed };
            Binding = new CustomBinding { SendTimeout = TimeSpan.FromSeconds(SendTimeout), ReceiveTimeout = TimeSpan.FromSeconds(ReceiveTimeout) };
            ExchangeBindings = MetadataExchangeBindings.CreateMexHttpBinding();

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
        /// 启动服务主机
        /// </summary>
        /// <param name="type">Binding类型</param>
        /// <param name="develop">是否开发模式</param>
        /// <returns>ServiceHost List 已启动服务主机集合</returns>
        public List<ServiceHost> StartService(string type, bool develop)
        {
            var service = DataAccess.GetServiceList(type);
            var hosts = new List<ServiceHost>();
            foreach (var host in service.Select(serv => CreateHost(serv, develop)).Where(host => host != null))
            {
                hosts.Add(host);
                var td = new Thread(() => host.Open());
                td.Start();
            }
            return hosts;
        }

        /// <summary>
        /// 启动服务主机
        /// </summary>
        /// <param name="Path">文件相对路径</param>
        /// <param name="Name">主机名</param>
        /// <param name="ClassName">类名</param>
        /// <param name="Interface">接口</param>
        /// <param name="DevelopMode">是否开发模式</param>
        /// <returns>ServiceHost 服务主机</returns>
        public ServiceHost StartService(string Path, string Name, string ClassName, string Interface, bool DevelopMode)
        {
            var servInfo = new SYS_Interface
            {
                Name = Name,
                Class = ClassName,
                Interface = Interface,
                Location = Path
            };
            var host = CreateHost(servInfo, DevelopMode);
            if (host == null) return null;

            host.Open();
            return host;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建WCF服务主机
        /// </summary>
        /// <param name="servinfo">服务信息</param>
        /// <param name="DevelopMode">是否开发模式</param>
        /// <returns>ServiceHost WCF服务主机</returns>
        private ServiceHost CreateHost(SYS_Interface servinfo, bool DevelopMode)
        {
            string path = $"{Application.StartupPath}\\{servinfo.Location}\\{servinfo.Name}.dll";
            if (!File.Exists(path)) return null;

            var asm = Assembly.LoadFrom(path);
            try
            {
                var address = new Uri(BaseAddress + servinfo.Port);
                var host = new ServiceHost(asm.GetType(servinfo.Class), address);
                ServiceEndpoint endpoint = host.AddServiceEndpoint(asm.GetType(servinfo.Interface), Binding, servinfo.Name);

                var behavior = new ServiceMetadataBehavior();
                if (servinfo.Binding == "HTTP")
                {
                    endpoint.Behaviors.Add(new WebHttpBehavior());
                    if (DevelopMode)
                    {
                        behavior.HttpGetEnabled = true;
                        behavior.HttpGetUrl = new Uri(address, servinfo.Name + "/mex");
                        host.Description.Behaviors.Add(behavior);
                    }
                }
                else if (DevelopMode)
                {
                    host.Description.Behaviors.Add(behavior);
                    host.AddServiceEndpoint(typeof (IMetadataExchange), ExchangeBindings, servinfo.Name + "/mex");
                }

                return host;
            }
            catch (Exception ex)
            {
                Util.LogToEvent(ex.ToString());
                return null;
            }
        }

        #endregion

    }

    public class TypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Json;
        }

    }

}
