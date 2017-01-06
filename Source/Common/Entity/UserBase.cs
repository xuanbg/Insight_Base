using System;

namespace Insight.Base.Common.Entity
{
    public class UserBase
    {
        protected SYS_User _User;

        /// <summary>
        /// 用户唯一ID
        /// </summary>
        public Guid ID
        {
            get { return _User.ID; }
            set { _User.ID = value; }
        }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string Name
        {
            get { return _User.Name; }
            set { _User.Name = value; }
        }

        /// <summary>
        /// 用户登录账号
        /// </summary>
        public string LoginName
        {
            get { return _User.LoginName; }
            set { _User.LoginName = value; }
        }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile
        {
            get { return _User.Mobile; }
            set { _User.Mobile = value; }
        }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password
        {
            get { return _User.Password; }
            set { _User.Password = value; }
        }

        /// <summary>
        /// 支付密码
        /// </summary>
        public string PayPassword
        {
            get { return _User.PayPassword; }
            set { _User.PayPassword = value; }
        }

        /// <summary>
        /// 用户描述
        /// </summary>
        public string Description
        {
            get { return _User.Description; }
            set { _User.Description = value; }
        }

        /// <summary>
        /// 用户类型
        /// </summary>
        public int Type
        {
            get { return _User.Type; }
            set { _User.Type = value; }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Validity
        {
            get { return _User.Validity; }
            set { _User.Validity = value; }
        }

        /// <summary>
        /// 是否内置用户
        /// </summary>
        public bool BuiltIn
        {
            get { return _User.BuiltIn; }
            set { _User.BuiltIn = value; }
        }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId
        {
            get { return _User.CreatorUserId; }
            set { _User.CreatorUserId = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get { return _User.CreateTime; }
            set { _User.CreateTime = value; }
        }
    }
}
