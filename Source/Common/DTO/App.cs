using System;
using System.Collections.Generic;

namespace Insight.Base.Common.DTO
{
    public class App
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 应用名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 应用别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 域名
        /// </summary>
        public string host { get; set; }

        /// <summary>
        /// 令牌生存周期(小时)
        /// </summary>
        public int tokenLife { get; set; }

        /// <summary>
        /// 图标url
        /// </summary>
        public string iconurl { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public byte[] icon { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string creatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createTime { get; set; }

        /// <summary>
        /// 导航
        /// </summary>
        public List<AppTree> navs { get; set; }
    }
}
