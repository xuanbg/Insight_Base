using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class UserGroup
    {
        private readonly SYS_UserGroup _Group;
        private List<SYS_UserGroupMember> _Members;
        internal Result<object> Result = new Result<object>();

        /// <summary>
        /// 用户组是否已存在(按名称)
        /// </summary>
        public bool Existed
        {
            get
            {
                using (var context = new Entities())
                {
                    var group = context.SYS_UserGroup.SingleOrDefault(u => u.Name == _Group.Name);
                    var existed = group != null && group.ID != _Group.ID;
                    if (existed) Result.DataAlreadyExists();

                    return existed;
                }
            }
        }

        /// <summary>
        /// 用户组唯一ID
        /// </summary>
        public Guid ID
        {
            get => _Group.ID;
            set => _Group.ID = value;
        }

        /// <summary>
        /// 用户组名称
        /// </summary>
        public string Name
        {
            get => _Group.Name;
            set => _Group.Name = value;
        }

        /// <summary>
        /// 用户组描述
        /// </summary>
        public string Description
        {
            get => _Group.Description;
            set => _Group.Description = value;
        }

        /// <summary>
        /// 是否内置用户组
        /// </summary>
        public bool BuiltIn
        {
            get => _Group.BuiltIn;
            set => _Group.BuiltIn = value;
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Validity
        {
            get => _Group.Visible;
            set => _Group.Visible = value;
        }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public Guid CreatorUserId
        {
            get => _Group.CreatorUserId;
            set => _Group.CreatorUserId = value;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get => _Group.CreateTime;
            set => _Group.CreateTime = value;
        }

        /// <summary>
        /// 用户组成员
        /// </summary>
        public List<MemberUser> Members
        {
            get => GetMember();
            set => SetMember(value ?? new List<MemberUser>());
        }

        /// <summary>
        /// 构造函数，构造新的用户组实体
        /// </summary>
        public UserGroup()
        {
            _Group = new SYS_UserGroup();
            Result.Success();
        }

        /// <summary>
        /// 构造函数，根据ID读取用户组实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        public UserGroup(Guid id)
        {
            using (var context = new Entities())
            {
                _Group = context.SYS_UserGroup.SingleOrDefault(g => g.ID == id);
                if (_Group == null)
                {
                    _Group = new SYS_UserGroup();
                    Result.NotFound();
                }
                else
                {
                    Result.Success();
                }
            }
        }

        /// <summary>
        /// 新增用户组
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Add()
        {
            var result = DbHelper.Insert(_Group);
            if (result)
                Result.Created();
            else
                Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 删除用户组
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Delete()
        {
            if (BuiltIn)
            {
                Result.NotBeDeleted();
                return false;
            }

            var result = DbHelper.Delete(_Group);
            if (!result) Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 更新用户组
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Update()
        {
            var result = DbHelper.Update(_Group);
            if (!result) Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 设置用户组创建信息
        /// </summary>
        /// <param name="id">创建人ID</param>
        public void SetCreatorInfo(Guid id)
        {
            _Members.ForEach(i =>
            {
                i.CreatorUserId = id;
                i.CreateTime = DateTime.Now;
            });
        }

        /// <summary>
        /// 新增用户组成员
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool AddMember()
        {
            if (DbHelper.Insert(_Members)) return true;

            Result.DataBaseError();
            return false;
        }

        /// <summary>
        /// 移除用户组成员
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool RemoveMembers()
        {
            if (DbHelper.Delete(_Members)) return true;

            Result.DataBaseError();
            return false;
        }

        /// <summary>
        /// 获取用户组成员
        /// </summary>
        /// <returns></returns>
        private List<MemberUser> GetMember()
        {
            using (var context = new Entities())
            {
                var list = from m in context.SYS_UserGroupMember
                           join u in context.SYS_User on m.UserId equals u.ID
                           where m.GroupId == _Group.ID
                           orderby u.SN
                           select new MemberUser
                           {
                               ID = m.ID,
                               Name = u.Name,
                               LoginName = u.LoginName,
                               Description = u.Description,
                               Validity = u.Validity
                           };
                return list.ToList();
            }
        }

        /// <summary>
        /// 转换用户组成员为成员关系数据
        /// </summary>
        /// <param name="members"></param>
        private void SetMember(IEnumerable<MemberUser> members)
        {
            var list = from m in members
                       select new SYS_UserGroupMember
                       {
                           ID = m.ID,
                           GroupId = ID,
                           UserId = m.UserId,
                           CreatorUserId = m.CreatorUserId,
                           CreateTime = m.CreateTime
                       };
            _Members = list.ToList();
        }
    }
}