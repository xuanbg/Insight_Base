using System;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class UserGroup : GroupBase
    {
        public Result Result = new Result();

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
    }
}