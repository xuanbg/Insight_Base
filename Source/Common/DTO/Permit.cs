namespace Insight.Base.Common.DTO
{
    /// <summary>
    /// 导航/功能授权实体
    /// </summary>
    public class Permit
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int index { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string filePath { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public byte[] icon { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public bool? permit { get; set; }

        /// <summary>
        /// 是否开始分组：0、否；1、是
        /// </summary>
        public bool isBegin { get; set; }

        /// <summary>
        /// 是否显示文字：0、隐藏；1、显示
        /// </summary>
        public bool isShowText { get; set; }

        /// <summary>
        /// 是否默认启动：0、否；1、是
        /// </summary>
        public bool isDefault { get; set; }
    }

    /// <summary>
    /// 功能授权关键数据
    /// </summary>
    public class PermitFunc
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 授权码
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public int permit { get; set; }
    }

    /// <summary>
    /// 数据授权关键数据
    /// </summary>
    public class PermitData
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public string parentId { get; set; }

        /// <summary>
        /// 授权模式()数据权限
        /// </summary>
        public int mode { get; set; }

        /// <summary>
        /// 授权
        /// </summary>
        public int permit { get; set; }
    }
}