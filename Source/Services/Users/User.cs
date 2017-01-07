using System;
using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    public class User : UserBase
    {
        public Result Result = new Result();
        public Authority Authority;

        /// <summary>
        /// 用户是否已存在(按登录账号)
        /// </summary>
        public bool Existed
        {
            get
            {
                using (var context = new BaseEntities())
                {
                    var user = context.SYS_User.SingleOrDefault(u => u.LoginName == _User.LoginName);
                    var existed = user != null && user.ID != _User.ID;
                    if (existed) Result.AccountExists();

                    return existed;
                }
            }
        }

        /// <summary>
        /// 授予用户的功能权限
        /// </summary>
        public List<RoleAction> Actions => Authority?.GetActions();

        /// <summary>
        /// 授予用户的数据权限
        /// </summary>
        public List<RoleData> Datas => Authority?.GetDatas();

        /// <summary>
        /// 构造函数，构造新的用户实体
        /// </summary>
        public User()
        {
            Result.Success();
        }

        /// <summary>
        /// 构造函数，根据ID读取用户实体
        /// </summary>
        /// <param name="id">用户ID</param>
        public User(Guid id)
        {
            using (var context = new BaseEntities())
            {
                _User = context.SYS_User.SingleOrDefault(u => u.ID == id);
                if (_User == null)
                {
                    _User = new SYS_User();
                    Result.NotFound();
                }
                else
                {
                    Result.Success();
                }
            }
        }

        /// <summary>
        /// 构造函数，根据登录账号读取用户实体
        /// </summary>
        /// <param name="account">登录账号</param>
        public User(string account)
        {
            using (var context = new BaseEntities())
            {
                _User = context.SYS_User.SingleOrDefault(u => u.LoginName == account);
                if (_User == null)
                {
                    _User = new SYS_User();
                    Result.NotFound();
                }
                else Result.Success();
            }
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Add()
        {
            var result = DbHelper.Insert(_User);
            if (result)
                Result.Created();
            else
                Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Delete()
        {
            var result = DbHelper.Delete(_User);
            if (!result) Result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <returns>bool 是否成功</returns>
        public bool Update()
        {
            var result = DbHelper.Update(_User);
            if (!result) Result.DataBaseError();

            return result;
        }
    }
}