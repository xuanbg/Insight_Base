using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.WS.Base.Common.Entity;
using static Insight.WS.Base.Common.SqlHelper;

namespace Insight.WS.Base
{
    public partial class Roles
    {

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <returns>角色信息结果集</returns>
        private IEnumerable<object> GetRoles()
        {
            using (var context = new BaseEntities())
            {
                var members = context.RoleMember;
                var users = context.RoleUser;
                var modules = context.RoleModulePermit;
                var actions = context.RoleActionPermit;
                var datas = context.RoleDataPermit;
                var roles = from r in context.SYS_Role.OrderBy(r => r.SN)
                            where r.Validity
                            select new
                            {
                                r.ID,
                                r.BuiltIn,
                                r.Name,
                                r.Description,
                                Members = members.Where(m => m.RoleId == r.ID).ToList(),
                                Users = users.Where(u => u.RoleId == r.ID).ToList(),
                                Modules = modules.Where(m => m.RoleId == r.ID).ToList(),
                                Actions = actions.Where(a => a.RoleId == r.ID).ToList(),
                                Datas = datas.Where(d => d.RoleId == r.ID).ToList()
                            };
                return roles.ToList();
            }
        }

        #region 获取数据

        /// <summary>
        /// 获取用户获得授权的所有模块的组信息
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <returns>DataTable 模块组列表</returns>
        public DataTable GetModuleGroup(Session us)
        {
            var sql = "select ID, [Index], Name, Icon from SYS_ModuleGroup where ID in ";
            sql += "(select ModuleGroupId from SYS_Module M join dbo.Get_PermModule(@UserId, @DeptId) P on P.ModuleId = M.ID) ";
            sql += "order by [Index]";
            var parm = new[]
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = us.UserId},
                new SqlParameter("@DeptId", SqlDbType.UniqueIdentifier) {Value = us.DeptId}
            };

            return SqlQuery(MakeCommand(sql, parm));
        }

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <returns>DataTable 模块列表</returns>
        public DataTable GetUserModule(Session us)
        {
            var sql = "select ModuleGroupId, ID, [Index], ProgramName, MainFrom, ApplicationName, Location, [Default], Icon from SYS_Module M ";
            sql += "join dbo.Get_PermModule(@UserId, @DeptId) P on P.ModuleId = M.ID where M.Validity = 1 order by M.[Index]";
            var parm = new[]
            {
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = us.UserId},
                new SqlParameter("@DeptId", SqlDbType.UniqueIdentifier) {Value = us.DeptId}
            };

            return SqlQuery(MakeCommand(sql, parm));
        }

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_Module 模块对象实体</returns>
        public SYS_Module GetModuleInfo(Session us, Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Module.SingleOrDefault(m => m.ID == id);
            }
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="id">模块ID</param>
        /// <returns>DataTable 工具栏功能列表</returns>
        public DataTable GetAction(Session us, Guid id)
        {
            var sql = "select A.*, cast(case when PA.ActionId is null then 0 else 1 end as bit) as [Enable] from SYS_ModuleAction A ";
            sql += "left join dbo.Get_PermAction(@ModuleId, @UserId, @DeptId) PA on PA.ActionId = A.ID ";
            sql += "where A.ModuleId = @ModuleId order by A.[Index]";
            var parm = new[]
            {
                new SqlParameter("@ModuleId", SqlDbType.UniqueIdentifier) {Value = id},
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = us.UserId},
                new SqlParameter("@DeptId", SqlDbType.UniqueIdentifier) {Value = us.DeptId}
            };

            return SqlQuery(MakeCommand(sql, parm));
        }

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="mid"></param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        public List<SYS_ModuleParam> GetModuleParam(Session us, Guid mid)
        {
            var ids = new List<Guid>();
            List<SYS_ModuleParam> mps;
            using (var context = new BaseEntities())
            {
                mps = context.SYS_ModuleParam.Where(p => p.ModuleId == mid && ((p.OrgId == null && p.UserId == null) || p.OrgId == us.DeptId || p.UserId == us.UserId)).ToList();
            }
            foreach (var pam in mps)
            {
                // 当前全局选项，判断是否存在同类非全局选项
                if (pam.OrgId == null && pam.UserId == null && mps.Exists(p => p.ParamId == pam.ParamId && (p.OrgId != null || p.UserId != null)))
                {
                    ids.Add(pam.ID);
                }
                // 当前部门选项，判断是否存在同类用户选项
                if (pam.OrgId != null && pam.UserId == null && mps.Exists(p => p.ParamId == pam.ParamId && p.UserId != null))
                {
                    ids.Add(pam.ID);
                }
            }
            ids.ForEach(pid => mps.Remove(mps.Find(p => p.ID == pid)));
            return mps;
        }

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="mid">模块ID</param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        public List<SYS_ModuleParam> GetModuleUserParam(Session us, Guid mid)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_ModuleParam.Where(p => p.ModuleId == mid && p.UserId == us.UserId).ToList();
            }
        }

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="mid">模块ID</param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        public List<SYS_ModuleParam> GetModuleDeptParam(Session us, Guid mid)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_ModuleParam.Where(p => p.ModuleId == mid && p.OrgId == us.DeptId && p.UserId == null).ToList();
            }
        }

        /// <summary>
        /// 保存模块选项参数
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="apl">新增参数集合</param>
        /// <param name="upl">更新参数集合</param>
        /// <returns>bool 是否保存成功</returns>
        public bool SaveModuleParam(Session us, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
        {
            const string sql = "insert SYS_ModuleParam (ModuleId, ParamId, Name, Value, OrgId, UserId, Description) select @ModuleId, @ParamId, @Name, @Value, @OrgId, @UserId, @Description";
            var cmds = apl.Select(p => new[]
            {
                new SqlParameter("@ModuleId", SqlDbType.UniqueIdentifier) {Value = p.ModuleId},
                new SqlParameter("@ParamId", SqlDbType.UniqueIdentifier) {Value = p.ParamId},
                new SqlParameter("@Name", p.Name),
                new SqlParameter("@Value", p.Value),
                new SqlParameter("@OrgId", SqlDbType.UniqueIdentifier) {Value = p.OrgId},
                new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) {Value = p.UserId},
                new SqlParameter("@Description", p.Description)
            }).Select(parm => MakeCommand(sql, parm)).ToList();
            cmds.AddRange(upl.Select(p => $"update SYS_ModuleParam set Value = '{p.Value}' where ID = '{p.ID}'").Select(s => MakeCommand(s)));
            return SqlExecute(cmds);
        }

        #endregion

    }
}
