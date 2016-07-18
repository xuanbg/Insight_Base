using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Base.Common.Utils;

namespace Insight.Base.Services
{
    public partial class Modules
    {

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_Module</returns>
        private SYS_Module GetModuleInfo(Guid id)
        {
            using (var context = new BaseEntities())
            {
                return context.SYS_Module.SingleOrDefault(m => m.ID == id);
            }
        }

        #region 获取数据

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="us">Session对象实体</param>
        /// <param name="mid"></param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        public List<SYS_ModuleParam> GetModuleParam(Token us, Guid mid)
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
        public List<SYS_ModuleParam> GetModuleUserParam(Token us, Guid mid)
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
        public List<SYS_ModuleParam> GetModuleDeptParam(Token us, Guid mid)
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
        public bool SaveModuleParam(Token us, List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl)
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
            }).Select(parm => SqlHelper.MakeCommand(sql, parm)).ToList();
            cmds.AddRange(upl.Select(p => $"update SYS_ModuleParam set Value = '{p.Value}' where ID = '{p.ID}'").Select(s => SqlHelper.MakeCommand(s)));
            return SqlHelper.SqlExecute(cmds);
        }

        #endregion

    }
}
