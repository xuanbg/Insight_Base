using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    [Table("ibs_log_rule")]
    public class LogRule
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 日志等级：0、emergency；1、alert；2、critical；3、error；4、warning；5、notice；6、informational；7、debug
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// 操作代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 事件来源
        /// </summary>
        public string source { get; set; }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 是否写到文件：0、否；1、是
        /// </summary>
        [Column("is_file")]
        public bool isFile { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("creator_id")]
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }

    [Table("ibs_log")]
    public class Log
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 日志等级：0、emergency；1、alert；2、critical；3、error；4、warning；5、notice；6、informational；7、debug
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// 操作代码
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 事件来源
        /// </summary>
        public string source { get; set; }

        /// <summary>
        /// 操作名称
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 来源用户ID
        /// </summary>
        [Column("user_id")]
        public string userId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_time")]
        public DateTime createTime { get; set; }
    }
}
