using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.DataAccess;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base.Service
{
    public partial class BaseService
    {

        /// <summary>
        /// 从视图查询组织机构列表
        /// </summary>
        /// <returns>DataTable</returns>
        private DataTable GetOrgList()
        {
            const string sql = "select * from Organization order by NodeType desc, [Index]";
            return SqlQuery(MakeCommand(sql));
        }

        /// <summary>
        /// 根据对象实体数据新增一个组织机构节点
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="obj">组织节点对象</param>
        /// <param name="index">原序号</param>
        /// <returns>bool 是否成功</returns>
        private bool InsertData(Guid uid, SYS_Organization obj, int index)
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
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid}
            };
            cmds.Add(MakeCommand(ChangeIndex("SYS_Organization", index, obj.Index, obj.ParentId, false)));
            cmds.Add(MakeCommand(sql, parm));
            return SqlExecute(cmds);
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
            cmds.Add(MakeCommand($"Delete from SYS_Organization where ID = '{id}'"));
            cmds.Add(MakeCommand(ChangeIndex("SYS_Organization", obj.Index, 99999, obj.ParentId, false)));
            return SqlExecute(cmds);
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
            const string sql = "update SYS_Organization set [Index] = @Index, Code = @Code, Name = @Name, Alias = @Alias, FullName = @FullName, PositionId = @PositionId where ID = @ID";
            var parm = new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = obj.ID},
                new SqlParameter("@Index", obj.Index),
                new SqlParameter("@Code", obj.Code),
                new SqlParameter("@Name", obj.Name),
                new SqlParameter("@Alias", obj.Alias),
                new SqlParameter("@FullName", obj.FullName),
                new SqlParameter("@PositionId", SqlDbType.UniqueIdentifier) {Value = obj.PositionId}
            };

            cmds.Add(MakeCommand(ChangeIndex("SYS_Organization", index, obj.Index, obj.ParentId, false)));
            cmds.Add(MakeCommand(sql, parm));
            return SqlExecute(cmds);
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
        /// 根据对象实体数据新增一条组织机构节点合并记录
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="obj">组织节点合并对象</param>
        /// <returns>bool 是否成功</returns>
        private bool InsertData(Guid uid, SYS_OrgMerger obj)
        {
            var sql = "insert into SYS_OrgMerger ([OrgId], [MergerOrgId], [CreatorUserId]) ";
            sql += "select @OrgId, @MergerOrgId, @CreatorUserId ";
            sql += "select ID From SYS_OrgMerger where SN = SCOPE_IDENTITY()";
            var parm = new[]
            {
                new SqlParameter("@OrgId", SqlDbType.UniqueIdentifier) {Value = obj.OrgId},
                new SqlParameter("@MergerOrgId", SqlDbType.UniqueIdentifier) {Value = obj.MergerOrgId},
                new SqlParameter("@CreatorUserId", SqlDbType.UniqueIdentifier) {Value = uid}
            };
            return SqlNonQuery(MakeCommand(sql, parm)) > 0;
        }

        /// <summary>
        /// 根据对象实体数据更新组织机构表ParentId字段
        /// </summary>
        /// <param name="obj">组织节点对象</param>
        /// <returns>int 更新成功的记录数量</returns>
        private bool Update(SYS_Organization obj)
        {
            var org = GetOrg(obj.ID);
            var cmds = new List<SqlCommand>();

            const string sql = "update SYS_Organization set ParentId = @ParentId, [Index] = @Index where ID = @ID";
            var parm = new[]
            {
                new SqlParameter("@ID", SqlDbType.UniqueIdentifier) {Value = obj.ID},
                new SqlParameter("@Index", obj.Index),
                new SqlParameter("@ParentId", SqlDbType.UniqueIdentifier) {Value = obj.ParentId}
            };

            cmds.Add(MakeCommand(ChangeIndex("SYS_Organization", 999, obj.Index, obj.ParentId, false)));
            cmds.Add(MakeCommand(ChangeIndex("SYS_Organization", org.Index, 999, org.ParentId, false)));
            cmds.Add(MakeCommand(sql, parm));
            return SqlExecute(cmds);
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
            }).Select(parm => MakeCommand(sql, parm)).ToList();
            return SqlExecute(cmds);
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
            }).Select(parm => MakeCommand(sql, parm)).ToList();
            return SqlExecute(cmds);
        }

        /// <summary>
        /// 获取所有职位成员用户
        /// </summary>
        /// <returns>DataTable 节点成员信息结果集</returns>
        private DataTable GetOrgMemberList()
        {
            const string sql = "select * from TitleMember";
            return SqlQuery(MakeCommand(sql));
        }

        /// <summary>
        /// 获取职位成员之外的所有用户
        /// </summary>
        /// <param name="id">节点ID</param>
        /// <returns>DataTable 节点可添加成员信息结果集</returns>
        private DataTable GetOtherOrgMember(Guid id)
        {
            var sql = "select U.ID, U.Name as 用户名, U.LoginName as 登录名, U.[Description] as 描述 from SYS_User U left join MDG_Contact C on C.MID = U.ID ";
            sql += $"where not exists (select UserId from SYS_OrgMember where UserId = U.ID and OrgId = '{id}') and C.MID is null ";
            sql += "order by LoginName";
            return SqlQuery(MakeCommand(sql));
        }

    }
}
