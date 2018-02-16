using System.ComponentModel.DataAnnotations.Schema;

namespace Insight.Base.Common.Entity
{
    [Table("ucc_data_conf")]
    public class DataConfig
    {
        /// <summary>
        /// ID，唯一标识
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// 类型：0、无归属；1、仅本人；2、仅本部门；3、部门所有；4、机构所有
        /// </summary>
        [Column("data_type")]
        public int dataType { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string alias { get; set; }
    }
}
