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
        public Result AddGroup(SYS_UserGroup group)
        {
            const string action = "6E80210E-6F80-4FF7-8520-B602934D635C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = new UserGroup(group);
            if (!data.Result.Successful) return data.Result;

            data.CreatorUserId = verify.Basis.UserId;
            data.CreateTime = DateTime.Now;

            return data.Add() ? result : data.Result;
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
        public Result UpdateGroup(string id, SYS_UserGroup group)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var data = new UserGroup(parse.Value);
            if (!data.Result.Successful) return data.Result;

            data.Name = group.Name;
            data.Description = group.Description;
            return data.Update() ? result : data.Result;
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetGroup(string id)
        {
            const string action = "6910FD14-5654-4CF0-B159-8FE1DF68619F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var group = context.SYS_UserGroup.SingleOrDefault(e => e.ID == parse.Value);
                if (group == null) result.NotFound();
                else result.Success(group);

                return result;
            }
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
        /// <param name="uids">用户ID集合</param>
        /// <returns>Result</returns>
        public Result AddGroupMember(string id, List<Guid> uids)
        {
            const string action = "6C41724C-E118-4BCD-82AD-6B13D05C7894";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in uids
                           select new SYS_UserGroupMember
                           {
                               ID = Guid.NewGuid(),
                               GroupId = parse.Value,
                               UserId = u,
                               CreatorUserId = verify.Basis.UserId,
                               CreateTime = DateTime.Now
                           };
                context.SYS_UserGroupMember.AddRange(list);
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    result.DataBaseError();
                }
                return result;
            }
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="ids">户组成员关系ID集合</param>
        /// <returns>Result</returns>
        public Result RemoveMember(List<Guid> ids)
        {
            const string action = "686C115A-CE2E-4E84-8F25-B63C15AC173C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var list = from m in context.SYS_UserGroupMember.Where(i => ids.Any(id => id == i.ID))
                           select m;
                context.SYS_UserGroupMember.RemoveRange(list);
                try
                {
                    context.SaveChanges();
                }
                catch
                {
                    result.DataBaseError();
                }
                return result;
            }
        }

        /// <summary>
        /// 获取用户组的所有成员信息
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetGroupMembers(string id)
        {
            const string action = "B5992AA3-4AD3-4795-A641-2ED37AC6425C";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from m in context.SYS_UserGroupMember
                           join u in context.SYS_User on m.UserId equals u.ID
                           where m.GroupId == parse.Value && u.Validity
                           orderby u.SN
                           select new { m.ID, u.Name, u.LoginName, u.Description, u.Validity };
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

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
                           let m = context.SYS_UserGroupMember.Where(i => i.GroupId == parse.Value)
                           where u.Validity && u.Type > 0 && m == null
                           select new { u.ID, u.Name, u.LoginName };
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }
    }
}
