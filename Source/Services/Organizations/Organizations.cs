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
    public class Organizations : IOrganizations
    {
        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result AddOrg(Org org)
        {
            const string action = "88AC97EF-52A3-4F7F-8121-4C311206535F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            org.CreatorUserId = verify.Basis.UserId;
            if (org.Existed || !org.Add()) return org.Result;

            result.Created(org);
            return result;
        }

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result RemoveOrg(string id)
        {
            const string action = "71803766-97FE-4E6E-82DB-D5C90D2B7004";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var org = new Org(parse.Value);
            if (!org.Result.Successful) return org.Result;

            return org.Delete() ? result : org.Result;
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result UpdateOrg(string id, Org org)
        {
            const string action = "542D5E28-8102-40C6-9C01-190D13DBF6C6";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (org.Existed || !org.Update()) return org.Result;

            result.Success(org);
            return result;
        }

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result GetOrg(string id)
        {
            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            var org = new Org(parse.Value);
            if (!org.Result.Successful) return org.Result;

            result.Success(org);
            return result;
        }

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>Result</returns>
        public Result GetOrgs()
        {
            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var list = from o in context.SYS_Organization
                           select new {o.ID, o.ParentId, o.NodeType, o.Index, o.Code, o.Name, o.FullName, o.Alias, o.Validity, o.CreatorUserId, o.CreateTime};

                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="org">组织节点对象（被合并节点）</param>
        /// <returns>Result</returns>
        public Result OrgMerger(string id, Org org)
        {
            const string action = "DAE7F2C5-E379-4F74-8043-EB616D4A5F8B";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var oid = new GuidParse(id).Guid;
            if (!oid.HasValue)
            {
                result.BadRequest();
                return result;
            }

            org.CreatorUserId = verify.Basis.UserId;

            return result;
        }

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result AddOrgMember(string id, Org org)
        {
            const string action = "1F29DDEA-A4D7-4EF9-8136-0D4AFE88CB08";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!org.AddMember()) return org.Result;

            result.Success(org);
            return result;
        }

        /// <summary>
        /// 删除职位成员关系
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result RemoveOrgMember(Org org)
        {
            const string action = "70AC8EEB-F920-468D-8C8F-2DBA049ADAE9";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!org.RemoveMembers()) return org.Result;

            result.Success(org);
            return result;
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        public Result GetOtherOrgMember(string id)
        {
            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var parse = new GuidParse(id);
            if (!parse.Result.Successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User
                           join m in context.SYS_OrgMember.Where(i => i.OrgId == parse.Value) on u.ID equals m.UserId
                           into temp from t in temp.DefaultIfEmpty()
                           where t == null && u.Validity && u.Type > 0
                           orderby u.SN
                           select new {ID = Guid.NewGuid(), UserId = u.ID, u.Name, u.LoginName};
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result GetLoginDepts(string account)
        {
            var verify = new Compare();
            var result = verify.Result;
            if (!result.Successful) return result;

            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.LoginName == account);
                if (user == null)
                {
                    result.NotFound();
                    return result;
                }

                var list = from m in context.SYS_OrgMember.Where(m => m.UserId == user.ID)
                           join t in context.SYS_Organization on m.OrgId equals t.ID
                           join d in context.SYS_Organization on t.ParentId equals d.ID
                           select new { d.ID, d.FullName };
                if (list.Any()) result.Success(list.ToList());
                else result.NoContent();

                return result;
            }
        }
    }
}
