using System;

namespace Insight.Base.Common.Entity
{
    public class GroupBase
    {
        protected SYS_UserGroup _Group;

        /// <summary>
        /// 用户组唯一ID
        /// </summary>
        public Guid ID
        {
            get { return _Group.ID; }
            set { _Group.ID = value; }
        }

        /// <summary>
        /// 用户组名称
        /// </summary>
        public string Name
        {
            get { return _Group.Name; }
            set { _Group.Name = value; }
        }

        /// <summary>
        /// 用户组描述
        /// </summary>
        public string Description
        {
            get { return _Group.Description; }
            set { _Group.Description = value; }
        }

        /// <summary>
        /// 是否内置用户组
        /// </summary>
        public bool BuiltIn
        {
            get { return _Group.BuiltIn; }
            set { _Group.BuiltIn = value; }
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Validity
        {
            get { return _Group.Visible; }
            set { _Group.Visible = value; }
        }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId
        {
            get { return _Group.CreatorUserId; }
            set { _Group.CreatorUserId = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _Group.CreateTime; }
            set { _Group.CreateTime = value; }
        }
    }
}
