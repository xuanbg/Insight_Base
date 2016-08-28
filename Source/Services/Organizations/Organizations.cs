using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public partial class Organizations : IOrganizations
    {
        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="org">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddOrg(SYS_Organization org, int index)
        {
            const string action = "88AC97EF-52A3-4F7F-8121-4C311206535F";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            org.CreatorUserId = verify.Basis.UserId;
            var id = InsertData(org, index);
            if (id == null) result.DataBaseError();
            else result.Created(id.ToString());

            return result;
        }

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveOrg(string id)
        {
            var result = new JsonResult();
            Guid oid;
            if (!Guid.TryParse(id, out oid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "71803766-97FE-4E6E-82DB-D5C90D2B7004";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteOrg(oid)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>JsonResult</returns>
        public JsonResult UpdateOrg(string id, SYS_Organization org, int index)
        {
            const string action = "542D5E28-8102-40C6-9C01-190D13DBF6C6";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!Update(org, index)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetOrg(string id)
        {
            var result = new JsonResult();
            Guid oid;
            if (!Guid.TryParse(id, out oid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var org = GetOrg(oid);
            if (org == null) result.NotFound();
            else result.Success(org);

            return result;
        }

        /// <summary>
        /// 获取组织机构树
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetOrgTree()
        {
            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            var data = GetOrgList();
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="org">组织节点对象（被合并节点）</param>
        /// <returns>JsonResult</returns>
        public JsonResult OrgMerger(string id, SYS_Organization org)
        {
            var result = new JsonResult();
            Guid oid;
            if (!Guid.TryParse(id, out oid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "DAE7F2C5-E379-4F74-8043-EB616D4A5F8B";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            org.CreatorUserId = verify.Basis.UserId;
            if (!Update(oid, org)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构表ParentId字段
        /// </summary>
        /// <param name="id"></param>
        /// <param name="org">组织节点对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult SetOrgParent(string id, SYS_Organization org)
        {
            const string action = "DB1A4EA2-1B3E-41AD-91FA-A3945AB7D901";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!Update(org)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据参数组集合批量插入职位成员关系
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <param name="ids">用户ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddOrgMember(string id, List<Guid> ids)
        {
            var result = new JsonResult();
            Guid oid;
            if (!Guid.TryParse(id, out oid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "1F29DDEA-A4D7-4EF9-8136-0D4AFE88CB08";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            if (!InsertData(verify.Basis.UserId, oid, ids)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 根据ID集合删除职位成员关系
        /// </summary>
        /// <param name="ids">职位成员关系ID集合</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveOrgMember(List<Guid> ids)
        {
            const string action = "70AC8EEB-F920-468D-8C8F-2DBA049ADAE9";
            var verify = new Compare(action);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (!DeleteOrgMember(ids)) result.DataBaseError();

            return result;
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        public JsonResult GetOtherOrgMember(string id)
        {
            var result = new JsonResult();
            Guid oid;
            if (!Guid.TryParse(id, out oid))
            {
                result.InvalidGuid();
                return result;
            }

            const string action = "928C7527-A2F7-49A3-A548-12B3834D8822";
            var verify = new Compare(action);
            result = verify.Result;
            if (!result.Successful) return result;

            var data = GetOtherOrgMember(oid);
            if (data.Any()) result.Success(data);
            else result.NoContent();

            return result;
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="account">用户登录名</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetLoginDepts(string account)
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

                var data = GetDeptList(user.ID);
                if (data.Any()) result.Success(data);
                else result.NoContent();

                return result;
            }
        }
    }
}
