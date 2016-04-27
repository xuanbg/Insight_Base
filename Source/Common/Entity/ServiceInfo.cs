
namespace Insight.Base.Common.Entity
{
    public class ServiceInfo
    {
        private string path;

        /// <summary>
        /// 服务基地址
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// 服务端口号
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// 访问路径
        /// </summary>
        public string Path
        {
            get { return path ?? ""; }
            set { path = value; }
        }

        /// <summary>
        /// 服务命名空间
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// Endpoint名称
        /// </summary>
        public string Interface { get; set; }

        /// <summary>
        /// 服务实现类型名称
        /// </summary>
        public string ComplyType { get; set; }

        /// <summary>
        /// 库文件路径
        /// </summary>
        public string ServiceFile { get; set; }

        /// <summary>
        /// 是否启用Gzip压缩
        /// </summary>
        public bool Compress { get; set; }
    }
}
