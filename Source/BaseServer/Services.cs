using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Insight.WS.Base.Common.Entity;
using Microsoft.Samples.GZipEncoder;
using Insight.WS.Base.Common;

namespace Insight.WS.Base
{
    public class Services
    {

        #region 成员属性

        /// <summary>
        /// 服务基地址
        /// </summary>
        private static readonly string BaseAddress = Util.GetAppSetting("Address");

        /// <summary>
        /// 服务端口
        /// </summary>
        private static readonly string Port = Util.GetAppSetting("Port");

        /// <summary>
        /// Endpoints名称列表
        /// </summary>
        private static readonly string[] Endpoints = { "verify", "organizations", "users" };

        #endregion

        #region 静态公共方法

        /// <summary>
        /// 创建WCF服务主机
        /// </summary>
        /// <returns>ServiceHost WCF服务主机</returns>
        public static ServiceHost CreateHost(string path)
        {
            var asm = Assembly.LoadFrom(path);
            var address = new Uri($"{BaseAddress}:{Port}");
            var host = new ServiceHost(asm.GetType("Insight.WS.Base.Service.BaseService"), address);
            for (var i = 0; i < Endpoints.Length; i++)
            {
                var binding = InitBinding(i > 0);
                var name = Endpoints[i];
                var inter = $"Insight.WS.Base.Service.I{name}";
                var endpoint = host.AddServiceEndpoint(asm.GetType(inter), binding, name);
                endpoint.Behaviors.Add(new WebHttpBehavior());
            }
            return host;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化基本HTTP服务绑定
        /// </summary>
        /// <param name="isCompres">是否压缩传输</param>
        private static CustomBinding InitBinding(bool isCompres)
        {
            var encoder = new WebMessageEncodingBindingElement { ReaderQuotas = { MaxArrayLength = 67108864, MaxStringContentLength = 67108864 } };
            var transport = new HttpTransportBindingElement { ManualAddressing = true, MaxReceivedMessageSize = 1073741824, TransferMode = TransferMode.Streamed };
            var binding = new CustomBinding { SendTimeout = TimeSpan.FromSeconds(600), ReceiveTimeout = TimeSpan.FromSeconds(600) };

            if (isCompres)
            {
                encoder.ContentTypeMapper = new TypeMapper();
                var gZipEncode = new GZipMessageEncodingBindingElement(encoder);
                binding.Elements.AddRange(gZipEncode, transport);
            }
            else
            {
                binding.Elements.AddRange(encoder, transport);
            }
            return binding;
        }

        #endregion

    }

}
