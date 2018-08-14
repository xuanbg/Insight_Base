using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.DTO;
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
        public Result<object> AddOrg(Org org)
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
        public Result<object> UpdateOrg(string id, Org org)
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

            using (var context = new Entities())
            {
                var data = context.orgs.SingleOrDefault(i => !i.isInvalid && i.tenantId == tenantId && i.id == id);
                if (data == null) return result.NotFound();

                var org = Util.ConvertTo<OrgDTO>(data);
                var list = from m in context.orgMembers
                    join u in context.users on m.userId equals u.id
                    where m.orgId == id
                    select new MemberUser
                    {
                        id = m.id,
                        parentId = id,
                        name = u.name,
                        account = u.account,
                        remark = u.remark,
                        isInvalid = u.isInvalid
                    };
                org.members = list.ToList();

                return result.Success(org);
            }
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
                var list = context.orgs.Where(i => i.tenantId == tenantId).ToList();

                return list.Any() ? result.Success(list) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members">成员集合</param>
        /// <returns>Result</returns>
        public Result<object> AddOrgMember(string id, List<string> members)
        {
            if (!Verify("addOrgMember")) return result;

            using (var context = new Entities())
            {
                var data = context.orgs.SingleOrDefault(i => !i.isInvalid && i.tenantId == tenantId && i.id == id);
                if (data == null) return result.NotFound();

                data.members = new List<OrgMember>();
                members.ForEach(i =>
                {
                    var member = new OrgMember
                    {
                        id = Util.NewId(),
                        orgId = id,
                        userId = i,
                        creatorId = userId,
                        createTime = DateTime.Now
                    };
                    data.members.Add(member);
                });
                if (!DbHelper.Insert(data.members)) return result.DataBaseError();

                var list = from m in context.orgMembers
                    join u in context.users on m.userId equals u.id
                    where m.orgId == id
                    select new MemberUser
                    {
                        id = m.id,
                        parentId = id,
                        name = u.name,
                        account = u.account,
                        remark = u.remark,
                        isInvalid = u.isInvalid
                    };

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 删除职位成员关系
        /// </summary>
        /// <param name="id"></param>
        /// <param name="members">成员集合</param>
        /// <returns>Result</returns>
        public Result<object> RemoveOrgMember(string id, List<string> members)
        {
            if (!Verify("removeOrgMember")) return result;

            using (var context = new Entities())
            {
                var data = context.orgs.SingleOrDefault(i => !i.isInvalid && i.tenantId == tenantId && i.id == id);
                if (data == null) return result.NotFound();

                data.members = context.orgMembers.Where(i => members.Any(m => m == i.id)).ToList();
                if (!DbHelper.Delete(data.members)) return result.DataBaseError();

                var list = from m in context.orgMembers
                    join u in context.users on m.userId equals u.id
                    where m.orgId == id
                    select new MemberUser
                    {
                        id = m.id,
                        parentId = id,
                        name = u.name,
                        account = u.account,
                        remark = u.remark,
                        isInvalid = u.isInvalid
                    };

                return result.Success(list.ToList());
            }
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
                    join r in context.tenantUsers on u.id equals r.userId
                    join m in context.orgMembers.Where(i => i.orgId == id) on u.id equals m.userId
                        into temp
                    from t in temp.DefaultIfEmpty()
                    where !u.isInvalid && r.tenantId == tenantId && t == null
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
        private static Org GetData(string id)
        {
            using (var context = new Entities())
            {
                return context.orgs.SingleOrDefault(i => i.id == id);
            }
        }

        /// <summary>
        /// 节点是否已存在
        /// </summary>
        /// <param name="org">用户组</param>
        /// <returns>是否已存在</returns>
        private static bool Existed(Org org)
        {
            using (var context = new Entities())
            {
                return context.orgs.Any(i => i.id != org.id && i.tenantId == org.tenantId 
                                             && (i.parentId == org.parentId && i.name == org.name 
                                                 || !string.IsNullOrEmpty(org.code) && i.code == org.code
                                                 || !string.IsNullOrEmpty(org.alias) && i.alias == org.alias
                                                 || !string.IsNullOrEmpty(org.fullname) && i.fullname == org.fullname));
            }
        }
    }
}