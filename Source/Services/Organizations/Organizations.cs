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
    public class Organizations : ServiceBase, IOrganizations
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result<object> AddOrg(Organization org)
        {
            if (!Verify("newOrg")) return result;

            org.id = Util.NewId();
            org.tenantId = tenantId;
            if (Existed(org)) return result.DataAlreadyExists();

            org.creatorId = userId;
            org.createTime = DateTime.Now;

            return DbHelper.Insert(org) ? result.Created(org) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveOrg(string id)
        {
            if (!Verify("deleteOrg")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result<object> UpdateOrg(string id, Organization org)
        {
            if (!Verify("editOrg")) return result;

            var data = GetData(org.id);
            if (data == null) return result.NotFound();

            data.nodeType = org.nodeType;
            data.code = org.code;
            data.name = org.name;
            data.alias = org.alias;
            data.fullname = org.fullname;

            return DbHelper.Update(data) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result<object> GetOrg(string id)
        {
            if (!Verify("getOrgs")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            return result.Success(data);
        }

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetOrgs()
        {
            if (!Verify("getOrgs")) return result;

            using (var context = new Entities())
            {
                var list = context.organizations.Where(i => i.tenantId == tenantId).ToList();

                return list.Any() ? result.Success(list) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result<object> AddOrgMember(string id, Organization org)
        {
            if (!Verify("addOrgMember")) return result;

            var data = GetData(org.id);
            if (data == null) return result.NotFound();

            org.members.ForEach(i =>
            {
                i.id = Util.NewId();
                i.orgId = org.id;
                i.creatorId = userId;
                i.createTime = DateTime.Now;
            });

            return DbHelper.Insert(org.members) ? result.Success(org) : result.DataBaseError();
        }

        /// <summary>
        /// 删除职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result<object> RemoveOrgMember(string id, Organization org)
        {
            if (!Verify("removeOrgMember")) return result;

            var data = GetData(org.id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(org.members) ? result.Success(org) : result.DataBaseError();
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        public Result<object> GetOtherOrgMember(string id)
        {
            if (!Verify("getOrgs")) return result;

            using (var context = new Entities())
            {
                var list = from u in context.users
                           join m in context.orgMembers.Where(i => i.orgId == id) on u.id equals m.userId
                           into temp from t in temp.DefaultIfEmpty()
                           where !u.isInvalid && t == null
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
        private static Organization GetData(string id)
        {
            using (var context = new Entities())
            {
                return context.organizations.SingleOrDefault(i => i.id == id);
            }
        }

        /// <summary>
        /// 用户组是否已存在
        /// </summary>
        /// <param name="org">用户组</param>
        /// <returns>是否已存在</returns>
        private static bool Existed(Organization org)
        {
            using (var context = new Entities())
            {
                return context.organizations.Any(i => i.id != org.id && i.tenantId == org.tenantId && (i.code == org.code || i.alias == org.alias || i.fullname == org.fullname));
            }
        }
    }
}