using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class UserGroups : ServiceBase, IUserGroups
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据对象实体数据新增一个用户组
        /// </summary>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        public Result<object> AddGroup(Group group)
        {
            if (!Verify("newGroup")) return result;

            group.id = Util.NewId();
            group.tenantId = tenantId;
            if (Existed(group)) return result.DataAlreadyExists();

            group.creatorId = userId;
            group.createTime = DateTime.Now;

            return DbHelper.Insert(group) ? result.Created(group) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveGroup(string id)
        {
            if (!Verify("deleteGroup")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 根据对象实体数据更新用户组信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">用户组对象</param>
        /// <returns>Result</returns>
        public Result<object> UpdateGroup(string id, Group @group)
        {
            if (!Verify("editGroup")) return result;

            var data = GetData(group.id);
            if (data == null) return result.NotFound();

            data.name = group.name;
            data.remark = group.remark;
            if (Existed(data)) return result.DataAlreadyExists();

            return DbHelper.Update(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID获取用户组对象实体
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result<object> GetGroup(string id)
        {
            if (!Verify("getGroups")) return result;

            var data = GetData(id);
            return data == null ? result.NotFound() : result.Success(data);
        }

        /// <summary>
        /// 获取全部用户组
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetGroups(int rows, int page, string key)
        {
            if (!Verify("getGroups")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var skip = rows * (page - 1);
                var list = context.groups.Where(i => i.tenantId == tenantId && (string.IsNullOrEmpty(key) || i.name.Contains(key) || i.remark.Contains(key)));
                var data = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(data, list.Count());
            }
        }

        /// <summary>
        /// 根据参数组集合批量插入用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        public Result<object> AddGroupMember(string id, Group @group)
        {
            if (!Verify("addGroupMember")) return result;

            var data = GetData(group.id);
            if (data == null) return result.NotFound();

            group.members.ForEach(i =>
            {
                i.groupId = group.id;
                i.creatorId = userId;
                i.createTime = DateTime.Now;
            });

            return DbHelper.Insert(group.members) ? result.Success(group) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID集合删除用户组成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="group">UserGroup</param>
        /// <returns>Result</returns>
        public Result<object> RemoveMember(string id, Group @group)
        {
            if (!Verify("removeGroupMember")) return result;

            var data = GetData(group.id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(group.members) ? result.Success(group) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID获取组成员之外的全部用户
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>Result</returns>
        public Result<object> GetOtherUser(string id)
        {
            if (!Verify("getGroups")) return result;

            using (var context = new Entities())
            {
                var list = from u in context.users
                           join r in context.tenantUsers.Where(i => i.tenantId == tenantId) on u.id equals r.userId
                           join m in context.groupMembers.Where(i => i.groupId == id) on u.id equals m.userId
                           into temp from t in temp.DefaultIfEmpty()
                           where t == null && !u.isInvalid
                           orderby u.createTime
                           select new {u.id, u.name, u.account};
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 获取指定ID的用户组
        /// </summary>
        /// <param name="id">用户组ID</param>
        /// <returns>用户组</returns>
        private static Group GetData(string id)
        {
            using (var context = new Entities())
            {
                return context.groups.SingleOrDefault(i => i.id == id);
            }
        }

        /// <summary>
        /// 用户组是否已存在
        /// </summary>
        /// <param name="group">用户组</param>
        /// <returns>是否已存在</returns>
        private static bool Existed(Group group)
        {
            using (var context = new Entities())
            {
                return context.groups.Any(i => i.id != group.id && i.tenantId == group.tenantId && i.name == group.name);
            }
        }
    }
}