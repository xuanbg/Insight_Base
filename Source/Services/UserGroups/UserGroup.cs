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

        private List<SYS_UserGroupMember> _Members;

        /// <summary>
        /// 用户组是否已存在(按名称)
        /// </summary>
        public bool Exists
        {
            get
            {
                using (var context = new BaseEntities())
                {
                    var group = context.SYS_UserGroup.SingleOrDefault(u => u.Name == _Group.Name);
                    if (group != null) Result.DataAlreadyExists();

                    return group != null;
                }
            }
        }

        /// <summary>
        /// 用户组成员
        /// </summary>
        public List<MemberUser> Members
        {
            get { return GetMember(); }
            set { SetMember(value ?? new List<MemberUser>()); }
        }

        /// <summary>
        /// 构造函数，构造新的用户组实体
        /// </summary>
        public UserGroup()
        {
            Result.Success();
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
                if (_Group == null) Result.NotFound();
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
            if (DbHelper.Insert(_Members)) return true;

            Result.DataBaseError();
            return false;
        }

        /// <summary>
        /// 设置创建人ID
        /// </summary>
        /// <param name="id">创建人ID</param>
        public void SetCreatorUserId(Guid id)
        {
            _Members.ForEach(i => i.CreatorUserId = id);
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
                           GroupId = _Group.ID,
                           UserId = m.UserId,
                           CreatorUserId = m.CreatorUserId,
                           CreateTime = DateTime.Now
                       };
            _Members = list.ToList();
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