using System;
using System.Linq;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base.Common
{
    public class DataAccess
    {

        /// <summary>
        /// 更新用户登录密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">登录密码</param>
        /// <returns>bool 是否成功</returns>
        public static bool? UpdatePassword(Guid id, string pw)
        {
            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.ID == id);
                if (user == null) return null;

                if (user.Password == pw) return true;

                user.Password = pw;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="obj">用户数据对象</param>
        /// <returns>bool 是否成功</returns>
        public static bool? UpdateUserInfo(SYS_User obj)
        {
            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.ID == obj.ID);
                if (user == null) return null;

                user.LoginName = obj.LoginName;
                user.Name = obj.Name;
                user.Type = obj.Type;
                user.OpenId = obj.OpenId;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 更新用户状态
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="validity">是否有效</param>
        /// <returns>bool 是否成功</returns>
        public static bool? SetUserStatus(Guid id, bool validity)
        {
            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.ID == id);
                if (user == null) return null;

                if (user.Validity == validity) return true;

                user.Validity = validity;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 根据用户登录名获取用户对象实体
        /// </summary>
        /// <param name="str">用户登录名</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(string str)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(s => s.LoginName == str);
            }
        }

        /// <summary>
        /// 根据用户ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(u => u.ID == id);
            }
        }

        /// <summary>
        /// 根据操作ID返回鉴权结果
        /// </summary>
        /// <param name="obj">用于会话</param>
        /// <param name="id">操作ID</param>
        /// <returns>bool 是否授权</returns>
        public static bool Authority(Session obj, Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.Authority(obj.UserId, obj.DeptId, id).Any();
            }
        }

    }
}
