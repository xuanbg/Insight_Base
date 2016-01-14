using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base.Service
{
    public partial class BaseService
    {

        /// <summary>
        /// 拼装插入用户数据的SqlCommand
        /// </summary>
        /// <param name="obj">用户对象</param>
        /// <returns>SqlCommand</returns>
        private bool InsertData(SYS_User obj)
        {
            var sql = "insert SYS_User (ID, Name, LoginName, Password, PayPassword, OpenId, Description, Type, CreatorUserId) ";
            sql += "select @ID, @Name, @LoginName, @Password, @PayPassword, @OpenId, @Description, @Type, @CreatorUserId";
            var parm = new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = obj.ID},
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@LoginName", obj.LoginName),
                new SqlParameter("@Password", obj.Password),
                new SqlParameter("@PayPassword", obj.PayPassword),
                new SqlParameter("@OpenId", obj.OpenId),
                new SqlParameter("@Description", obj.Description),
                new SqlParameter("@Type", SqlDbType.Int) {Value = obj.Type},
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = obj.CreatorUserId},
                new SqlParameter("@Read", SqlDbType.Int) {Value = 0}
            };
            return SqlNonQuery(MakeCommand(sql, parm)) > 0;
        }

        /// <summary>
        /// 根据ID删除用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>bool 是否删除成功</returns>
        private bool RemoveUser(Guid id)
        {
            var sql = $"Delete from SYS_User where ID = '{id}' and BuiltIn = 0";
            return SqlNonQuery(MakeCommand(sql)) > 0;
        }

        /// <summary>
        /// 更新用户登录密码
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <param name="pw">登录密码</param>
        /// <returns>bool 是否成功</returns>
        private bool? Update(Guid id, string pw)
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
        private bool? Update(SYS_User obj)
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
        private bool? Update(Guid id, bool validity)
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
        /// 根据用户ID获取用户对象实体
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>SYS_User 用户对象实体</returns>
        private SYS_User GetUser(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_User.SingleOrDefault(u => u.ID == id);
            }
        }

        /// <summary>
        /// 获取全部用户
        /// </summary>
        /// <returns>DataTable 全部用户结果集</returns>
        private DataTable GetUserList()
        {
            const string sql = "select ID, BuiltIn as 内置, Name as 名称, LoginName as 登录名, Description as 描述, Case Validity when 1 then '正常' else '封禁' end 状态 From SYS_User where Type > 0 order by SN";
            return SqlQuery(MakeCommand(sql));
        }

    }
}
