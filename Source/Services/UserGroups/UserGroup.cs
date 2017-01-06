using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class UserGroup : GroupBase
    {
        public Result Result = new Result();

        private IEnumerable<SYS_UserGroupMember> _Members;

        public List<MemberUser> Members
        {
            get { return GetMember(); }
            set { SetMember(value); }
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
        /// 构造函数，传入用户组实体
        /// </summary>
        /// <param name="group">用户组实体</param>
        public UserGroup(SYS_UserGroup group)
        {
            using (var context = new BaseEntities())
            {
                _Group = context.SYS_UserGroup.SingleOrDefault(u => u.Name == group.Name);
                if (_Group == null)
                {
                    _Group = group;
                    Result.Success();
                }
                else
                    Result.AccountExists();
            }
        }

        /// <summary>
        /// 构造函数，根据ID读取用户组实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        public UserGroup(Guid id)
        {
            using (var context = new BaseEntities())
            {
                _Group = context.SYS_UserGroup.SingleOrDefault(g => g.ID == id);
                if (_Group == null)
                {
                    _Group = new SYS_UserGroup();
                    Result.NotFound();
                }
                else Result.Success();
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
        /// 新增用户组成员
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool AddMember()
        {
            return DbHelper.Insert(_Members);
        }

        /// <summary>
        /// 转换用户组成员为成员关系数据
        /// </summary>
        /// <param name="members"></param>
        private void SetMember(IEnumerable<MemberUser> members)
        {
            _Members = from m in members
                       select new SYS_UserGroupMember
                       {
                           ID = m.ID,
                           GroupId = _Group.ID,
                           UserId = m.UserId,
                           CreatorUserId = m.CreatorUserId,
                           CreateTime = DateTime.Now
                       };
        }

        /// <summary>
        /// 获取用户组成员
        /// </summary>
        /// <returns></returns>
        private List<MemberUser> GetMember()
        {
            using (var context = new BaseEntities())
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
    }
}