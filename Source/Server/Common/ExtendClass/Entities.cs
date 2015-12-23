using System.Data.Entity;

namespace Insight.WS.Server.Common
{

    public class Entities : DbContext
    {

        /// <summary>
        /// 数据库连结字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 带连接字符串名称的构造函数
        /// </summary>
        /// <param name="connectionString"></param>
        public Entities(string connectionString) : this(connectionString, false)
        {
            ConnectionString = Database.Connection.ConnectionString;
        }

        public Entities(string connectionString, bool proxyCreationEnabled) : base(connectionString)
        {
            Configuration.ProxyCreationEnabled = proxyCreationEnabled;
        }

    }

}
