using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserGroups : IUserGroups
    {
        /// <summary>
        /// 根据对象实体数据新增一个用户组
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        public Result AddGroup(UserGroup group)
        {
            const string action = "6E80210E-6F80-4FF7-8520-B602934D635C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (group.Existed) return group.Result;

            group.Validity = true;
            group.CreatorUserId = verify.Basis.UserId;
            group.CreateTime = DateTime.Now;

            if (!group.Add()) return group.Result;

            result.Created(group);
            return result;
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result RemoveGroup(string id)
        {
            const string action = "E46B7A1C-A8B0-49B5-8494-BF1B09F43452";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var group = new UserGroup(parse.Value);
            if (!group.Result.Successful) return group.Result;

            return group.Delete() ? result : group.Result;
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        public Result UpdateGroup(string id, UserGroup group)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            if (group.Existed || !group.Update()) return group.Result;

            result.Success(group);
            return result;
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetGroup(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var data = new UserGroup(parse.Value);
            if (!data.Result.Successful) return data.Result;

            result.Success(data);
            return result;
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetGroups(string rows, string page)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.Successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.Successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1)
            {
                result.BadRequest();
                return result;
            }

            using (var context = new BaseEntities())
            {
                var list = from g in context.SYS_UserGroup
                           where g.Visible
                           orderby g.SN
                           select new {g.ID, g.Name, g.Description, g.BuiltIn, g.CreatorUserId, g.CreateTime};
                var skip = ipr.Value * (ipp.Value - 1);
                var data = new
                {
                    Total = list.Count(),
                    Items = list.Skip(skip).Take(ipr.Value).ToList()
                };
                result.Success(data);
                return result;
            }
        }

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        public Result AddGroupMember(string id, UserGroup group)
        {
            const string action = "6C41724C-E118-4BCD-82AD-6B13D05C7894";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            group.SetCreatorUserId(verify.Basis.UserId);
            if (!group.AddMember()) return group.Result;

            result.Success(group);
            return result;
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ids">户组成员关系ID集合</param>
        /// <returns>Result</returns>
        public Result RemoveMember(string id, List<Guid> ids)
        {
            const string action = "686C115A-CE2E-4E84-8F25-B63C15AC173C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from m in context.SYS_UserGroupMember.Where(i => ids.Any(a => a == i.ID))
                           select m;
                context.SYS_UserGroupMember.RemoveRange(list);
                try
                {
                    context.SaveChanges();
                    var data = new UserGroup(parse.Value);
                    if (!data.Result.Successful) return data.Result;

                    result.Success(data);
                }
                catch
                {
                    result.DataBaseError();
                }
                return result;
            }
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetOtherUser(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User
                           join m in context.SYS_UserGroupMember.Where(i => i.GroupId == parse.Value) on u.ID equals m.UserId
                           into temp
                           from t in temp.DefaultIfEmpty()
                           where t == null && u.Validity && u.Type > 0
                           select new {ID = Guid.NewGuid(), UserId = u.ID, u.Name, u.LoginName};
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }
    }
}
