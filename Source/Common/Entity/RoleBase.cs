using System;

namespace Insight.Base.Common.Entity
{
    public class RoleBase
    {
        protected SYS_Role _Role;

        /// <summary>
        /// 角色唯一ID
        /// </summary>
        public Guid ID
        {
            get { return _Role.ID; }
            set { _Role.ID = value; }
        }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name
        {
            get { return _Role.Name; }
            set { _Role.Name = value; }
        }

        /// <summary>
        /// 角色描述
        /// </summary>
        public string Description
        {
            get { return _Role.Description; }
            set { _Role.Description = value; }
        }

        /// <summary>
        /// 是否内置
        /// </summary>
        public bool BuiltIn
        {
            get { return _Role.BuiltIn; }
            set { _Role.BuiltIn = value; }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Validity
        {
            get { return _Role.Validity; }
            set { _Role.Validity = value; }
        }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId
        {
            get { return _Role.CreatorUserId; }
            set { _Role.CreatorUserId = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _Role.CreateTime; }
            set { _Role.CreateTime = value; }
        }
    }
}
