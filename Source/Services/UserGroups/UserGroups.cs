using System;
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
            if (!Verify("6E80210E-6F80-4FF7-8520-B602934D635C")) return _Result;
            if (group.Existed) return group.Result;

            group.Validity = true;
            group.CreatorUserId = _UserId;
            group.CreateTime = DateTime.Now;

            return group.Add() ? _Result.Created(group) : group.Result;
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result RemoveGroup(string id)
        {
            if (!Verify("E46B7A1C-A8B0-49B5-8494-BF1B09F43452")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var group = new UserGroup(parse.Value);
            return group.Result.successful && group.Delete() ? _Result : group.Result;
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        public Result UpdateGroup(string id, UserGroup group)
        {
            if (!Verify("6910FD14-5654-4CF0-B159-8FE1DF68619F")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            return group.Existed || !group.Update() ? group.Result : _Result.Success(group);
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetGroup(string id)
        {
            if (!Verify("B5992AA3-4AD3-4795-A641-2ED37AC6425C")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var data = new UserGroup(parse.Value);
            return data.Result.successful ? _Result.Success(data) : data.Result;
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result GetGroups(string rows, string page)
        {
            if (!Verify("B5992AA3-4AD3-4795-A641-2ED37AC6425C")) return _Result;

            var ipr = new IntParse(rows);
            if (!ipr.Result.successful) return ipr.Result;

            var ipp = new IntParse(page);
            if (!ipp.Result.successful) return ipp.Result;

            if (ipr.Value > 500 || ipp.Value < 1) return _Result.BadRequest();

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

                return _Result.Success(data);
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
            if (!Verify("6C41724C-E118-4BCD-82AD-6B13D05C7894")) return _Result;

            group.SetCreatorInfo(_UserId);
            return group.AddMember() ? _Result.Success(group) : group.Result;
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        public Result RemoveMember(string id, UserGroup group)
        {
            if (!Verify("686C115A-CE2E-4E84-8F25-B63C15AC173C")) return _Result;

            return group.RemoveMembers() ? _Result.Success(group) : group.Result;
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result GetOtherUser(string id)
        {
            if (!Verify("B5992AA3-4AD3-4795-A641-2ED37AC6425C")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User
                           join m in context.SYS_UserGroupMember.Where(i => i.GroupId == parse.Value) on u.ID equals m.UserId
                           into temp from t in temp.DefaultIfEmpty()
                           where t == null && u.Validity && u.Type > 0
                           orderby u.SN
                           select new {ID = Guid.NewGuid(), UserId = u.ID, u.Name, u.LoginName};
                return list.Any() ? _Result.Success(list.ToList()) : _Result.NoContent();
            }
        }

        private Result _Result = new Result();
        private Guid _UserId;

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        private bool Verify(string action = null)
        {
            var verify = new Compare(action);
            _UserId = verify.Basis.userId;
            _Result = verify.Result;

            return _Result.successful;
        }
    }
}