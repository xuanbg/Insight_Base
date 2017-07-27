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
        public Result AddOrg(Organization org)
        {
            if (!Verify("88AC97EF-52A3-4F7F-8121-4C311206535F")) return _Result;

            org.CreatorUserId = _UserId;
            org.CreateTime = DateTime.Now;
            if (org.Existed || !org.Add()) return org.Result;

            return _Result.Created(org);
        }

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result RemoveOrg(string id)
        {
            if (!Verify("71803766-97FE-4E6E-82DB-D5C90D2B7004")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var org = new Organization(parse.Value);
            return org.Result.successful && org.Delete() ? _Result : org.Result;
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result UpdateOrg(string id, Organization org)
        {
            if (!Verify("542D5E28-8102-40C6-9C01-190D13DBF6C6")) return _Result;

            return org.Existed || !org.Update() ? org.Result : _Result.Success(org);
        }

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>Result</returns>
        public Result GetOrg(string id)
        {
            if (!Verify("928C7527-A2F7-49A3-A548-12B3834D8822")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            var org = new Organization(parse.Value);
            return org.Result.successful ? _Result.Success(org) : org.Result;
        }

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>Result</returns>
        public Result GetOrgs()
        {
            if (!Verify("928C7527-A2F7-49A3-A548-12B3834D8822")) return _Result;

            using (var context = new BaseEntities())
            {
                var list = from o in context.SYS_Organization
                           select new {o.ID, o.ParentId, o.NodeType, o.Index, o.Code, o.Name, o.FullName, o.Alias, o.Validity, o.CreatorUserId, o.CreateTime};

                return list.Any() ? _Result.Success(list.ToList()) : _Result.NoContent();
            }
        }

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="org">组织节点对象（被合并节点）</param>
        /// <returns>Result</returns>
        public Result OrgMerger(string id, Organization org)
        {
            if (!Verify("DAE7F2C5-E379-4F74-8043-EB616D4A5F8B")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            org.CreatorUserId = _UserId;
            return _Result;
        }

        /// <summary>
        /// 新增职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result AddOrgMember(string id, Organization org)
        {
            if (!Verify("1F29DDEA-A4D7-4EF9-8136-0D4AFE88CB08")) return _Result;

            org.SetCreatorInfo(_UserId);
            return org.AddMember() ? _Result.Success(org) : org.Result;
        }

        /// <summary>
        /// 删除职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="org">组织节点对象</param>
        /// <returns>Result</returns>
        public Result RemoveOrgMember(string id, Organization org)
        {
            if (!Verify("70AC8EEB-F920-468D-8C8F-2DBA049ADAE9")) return _Result;

            return org.RemoveMembers() ? _Result.Success(org) : org.Result;
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        public Result GetOtherOrgMember(string id)
        {
            if (!Verify("928C7527-A2F7-49A3-A548-12B3834D8822")) return _Result;

            var parse = new GuidParse(id);
            if (!parse.Result.successful) return parse.Result;

            using (var context = new BaseEntities())
            {
                var list = from u in context.SYS_User
                           join m in context.SYS_OrgMember.Where(i => i.OrgId == parse.Value) on u.ID equals m.UserId
                           into temp from t in temp.DefaultIfEmpty()
                           where t == null && u.Validity && u.Type > 0
                           orderby u.SN
                           select new {ID = Guid.NewGuid(), UserId = u.ID, u.Name, u.LoginName};
                return list.Any() ? _Result.Success(list.ToList()) : _Result.NoContent();
            }
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>Result</returns>
        public Result GetLoginDepts(string account)
        {
            if (!Verify()) return _Result;

            using (var context = new BaseEntities())
            {
                var user = context.SYS_User.SingleOrDefault(u => u.LoginName == account);
                if (user == null) return _Result.NotFound();

                var list = from m in context.SYS_OrgMember.Where(m => m.UserId == user.ID)
                           join t in context.SYS_Organization on m.OrgId equals t.ID
                           join d in context.SYS_Organization on t.ParentId equals d.ID
                           select new {d.ID, Name = d.FullName, Description = d.Code};
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
            var compare = new Compare();
            _Result = compare.Result;
            if (!_Result.successful) return false;

            _UserId = compare.Basis.userId;
            _Result = compare.Verify(action);

            return _Result.successful;
        }
    }
}