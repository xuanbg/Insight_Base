using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Services
{
    public partial class Organizations
    {

        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="obj">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>bool 是否成功</returns>
        private object InsertData(SYS_Organization obj, int index)
        {
            var cmds = new List<SqlCommand>();
            var sql = "insert SYS_Organization (ParentId, NodeType, [Index], Code, Name, Alias, FullName, PositionId, CreatorUserId)";
            sql += "select @ParentId, @NodeType, @Index, @Code, @Name, @Alias, @FullName, @PositionId, @CreatorUserId;";
            sql += "select ID from SYS_Organization where SN = scope_identity()";
            var parm = new[]
            {
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) {Value = obj.ParentId},
                new SqlParameter("@NodeType", obj.NodeType),
                new SqlParameter("@Index", obj.Index),
                new SqlParameter("@Code", obj.Code),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Alias", obj.Alias),
                new SqlParameter("@FullName", obj.FullName),
                new SqlParameter("@PositionId", SqlDbType.UniqueIdentifier) {Value = obj.PositionId},
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = obj.CreatorUserId},
                new SqlParameter("@Write", SqlDbType.Int) {Value = 0}
            };
            cmds.Add(SqlHelper.MakeCommand(DataAccess.ChangeIndex("SYS_Organization", index, obj.Index, obj.ParentId, false)));
            cmds.Add(SqlHelper.MakeCommand(sql, parm));
            return SqlHelper.SqlExecute(cmds, 0);
        }

        /// <summary>
        /// 根据ID删除组织机构节点
        /// </summary>
        /// <param name="id">组织节点ID</param>
        /// <returns>bool 是否删除成功</returns>
        private bool DeleteOrg(Guid id)
        {
            var cmds = new List<SqlCommand>();
            var obj = GetOrg(id);
            cmds.Add(SqlHelper.MakeCommand($"Delete from SYS_Organization where ID = '{id}'"));
            cmds.Add(SqlHelper.MakeCommand(DataAccess.ChangeIndex("SYS_Organization", obj.Index, 99999, obj.ParentId, false)));
            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构信息
        /// </summary>
        /// <param name="obj">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>int 更新成功的记录数量</returns>
        private bool Update(SYS_Organization obj, int index)
        {
            var cmds = new List<SqlCommand>();
            const string sql = "update SYS_Organization set ParentId = @ParentId, [Index] = @Index, Code = @Code, Name = @Name, Alias = @Alias, FullName = @FullName, PositionId = @PositionId where ID = @ID";
            var parm = new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = obj.ID},
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) {Value = obj.ParentId},
                new SqlParameter("@Index", obj.Index),
                new SqlParameter("@Code", obj.Code),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Alias", obj.Alias),
                new SqlParameter("@FullName", obj.FullName),
                new SqlParameter("@PositionId", SqlDbType.UniqueIdentifier) {Value = obj.PositionId}
            };

            cmds.Add(SqlHelper.MakeCommand(DataAccess.ChangeIndex("SYS_Organization", index, obj.Index, obj.ParentId, false)));
            cmds.Add(SqlHelper.MakeCommand(sql, parm));
            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 根据ID获取机构对象实体
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>SYS_Organization 节点对象</returns>
        private SYS_Organization GetOrg(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Organization.SingleOrDefault(e => e.ID == id);
            }
        }

        /// <summary>
        /// 从视图查询组织机构列表
        /// </summary>
        /// <returns>DataTable</returns>
        private IEnumerable<object> GetOrgList()
        {
            using (var context = new BaseEntities())
            {
                var list = from o in context.SYS_Organization
                           select new
                           {
                               o.ID,
                               o.ParentId,
                               o.Index,
                               o.NodeType,
                               o.Name,
                               o.FullName,
                               o.Alias,
                               o.Code,
                               Members = from m in context.SYS_OrgMember.Where(m => m.OrgId == o.ID)
                                         join u in context.SYS_User on m.UserId equals u.ID
                                         select new { m.ID, u.LoginName, u.Name, u.Validity }
                           };
                return list.ToList();
            }
        }

        /// <summary>
        /// 根据用户登录名获取可登录部门列表
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>DataTable 可登录部门列表</returns>
        private IEnumerable<object> GetDeptList(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var list = from m in context.SYS_OrgMember.Where(m => m.UserId == id)
                           join t in context.SYS_Organization on m.OrgId equals t.ID
                           join d in context.SYS_Organization on t.ParentId equals d.ID
                           select new { d.ID, d.FullName };
                return list.ToList();
            }
        }

        /// <summary>
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="id">合并目标ID</param>
        /// <param name="obj">组织节点对象（被合并节点）</param>
        /// <returns>bool 是否成功</returns>
        private bool Update(Guid id, SYS_Organization obj)
        {
            var cmd = SqlHelper.MakeCommand(DataAccess.ChangeIndex("SYS_Organization", obj.Index, 999, obj.ParentId, false));
            var cmds = new List<SqlCommand> {cmd};
            if (obj.NodeType < 3)
            {
                var org = GetOrg(id);
                cmds.Add(SqlHelper.MakeCommand($"update SYS_Organization set ParentId = '{org.ParentId}' where ParentId = '{obj.ParentId}'"));
            }
            else
            {
                cmds.Add(SqlHelper.MakeCommand($"update SYS_OrgMember set OrgId = '{id}' where OrgId = '{obj.ID}'"));
            }

            cmds.Add(SqlHelper.MakeCommand($"delete SYS_Organization where id = '{obj.ID}'"));
            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构表ParentId字段
        /// </summary>
        /// <param name="obj">组织节点对象</param>
        /// <returns>int 更新成功的记录数量</returns>
        private bool Update(SYS_Organization obj)
        {
            const string sql = "update SYS_Organization set ParentId = @ParentId, [Index] = @Index where ID = @ID";
            var parm = new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = obj.ID},
                new SqlParameter("@Index", obj.Index),
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) {Value = obj.ParentId}
            };

            var org = GetOrg(obj.ID);
            var cmds = new List<SqlCommand>
            {
                SqlHelper.MakeCommand(DataAccess.ChangeIndex("SYS_Organization", org.Index, 999, org.ParentId, false)),
                SqlHelper.MakeCommand(sql, parm)
            };

            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 根据参数组集合批量插入职位成员关系
        /// </summary>
        /// <param name="uid">用户ID</param>
        /// <param name="tid">职位ID</param>
        /// <param name="uids">用户ID集合</param>
        /// <returns>bool 是否成功</returns>
        private bool InsertData(Guid uid, Guid tid, IEnumerable<Guid> uids)
        {
            const string sql = "insert into SYS_OrgMember ([OrgId], [UserId], [CreatorUserId]) select @OrgId, @UserId, @CreatorUserId";
            var cmds = uids.Select(id => new[]
            {
                new SqlParameter("@OrgId", SqlDbType.UniqueIdentifier) {Value = tid},
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = id},
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid}
            }).Select(parm => SqlHelper.MakeCommand(sql, parm)).ToList();
            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 根据ID集合删除职位成员关系
        /// </summary>
        /// <param name="ids">职位成员关系ID集合</param>
        /// <returns>bool 是否删除成功</returns>
        private bool DeleteOrgMember(IEnumerable<Guid> ids)
        {
            const string sql = "Delete from SYS_OrgMember where ID = @ID";
            var cmds = ids.Select(id => new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = id}
            }).Select(parm => SqlHelper.MakeCommand(sql, parm)).ToList();
            return SqlHelper.SqlExecute(cmds);
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>DataTable 节点可添加成员信息结果集</returns>
        private IEnumerable<object> GetOtherOrgMember(Guid id)
        {
            using (var context = new BaseEntities())
            {
                var members = from m in context.SYS_OrgMember where m.OrgId == id select m.UserId;
                var list = from u in context.SYS_User
                           where u.Validity && u.Type > 0 && !members.Any(m => m == u.ID)
                           select new { u.ID, u.Name, u.LoginName };
                return list.OrderBy(u => u.LoginName).ToList();
            }
        }

    }
}
