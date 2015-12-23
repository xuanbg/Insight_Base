using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Server.Common.ORM;
using Insight.WS.Server.Common.Service;
using static Insight.WS.Server.Common.SqlHelper;

namespace Insight.WS.Server.Common
{
    public class DataAccess
    {

        #region 公共数据接口

        /// <summary>
        /// 修改指定用户的密码
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="pw">新密码Hash值</param>
        /// <returns>bool 是否修改成功</returns>
        public static bool UpdataPassword(Session us, string pw)
        {
            using (var context = new WSEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.ID == us.UserId);
                if (user == null) return false;

                user.Password = pw;
                return context.SaveChanges() > 0 && General.UpdateSignature(us.ID, pw);
            }
        }

        /// <summary>
        /// 根据用户登录名获取用户对象实体
        /// </summary>
        /// <param name="str">用户登录名</param>
        /// <returns>SYS_User 用户对象实体</returns>
        public static SYS_User GetUser(string str)
        {
            using (var context = new WSEntities())
            {
                return context.SYS_User.SingleOrDefault(s => s.LoginName == str);
            }
        }

        /// <summary>
        /// 获取可用服务列表
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<SYS_Interface> GetServiceList(string type)
        {
            using (var context = new WSEntities())
            {
                return context.SYS_Interface.Where(i => i.Binding == type).ToList();
            }
        }

        /// <summary>
        /// 拼装插入用户数据的SqlCommand
        /// </summary>
        /// <param name="obj">用户对象</param>
        /// <returns>SqlCommand</returns>
        public static SqlCommand AddUser(SYS_User obj)
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
            return MakeCommand(sql, parm);
        }

        #endregion
    }
}
