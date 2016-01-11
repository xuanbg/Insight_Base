namespace Insight.WS.Base.Common.Entity
{
    /// <summary>
    /// 接口配置
    /// </summary>
    public class ServiceInfo
    {

        /// <summary>
        /// 接口服务端口
        /// </summary>
        public string Port;

        /// <summary>
        /// 接口名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 接口命名空间
        /// </summary>
        public string Interface;

        /// <summary>
        /// 接口实现命名空间
        /// </summary>
        public string Attain;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path;

        /// <summary>
        /// 是否启用Gzip压缩
        /// </summary>
        public bool Compress;

    }
}
