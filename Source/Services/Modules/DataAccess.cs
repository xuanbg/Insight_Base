using System.Collections.Generic;
using System.Linq;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;

namespace Insight.Base.Services
{
    public partial class Modules
    {

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_Module</returns>
        private Navigator GetModuleInfo(string id)
        {
            using (var context = new Entities())
            {
                return context.navigators.SingleOrDefault(m => m.id == id);
            }
        }

        #region 获取数据

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="session">Session对象实体</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="moduleId">模块ID</param>
        /// <returns>参数集合</returns>
        public List<ModuleParam> GetModuleParam(Token session, string deptId, string moduleId)
        {
            var ids = new List<string>();
            List<ModuleParam> mps;
            using (var context = new Entities())
            {
                mps = context.moduleParams.Where(p => p.moduleId == moduleId && (p.orgId == null && p.userId == null || p.orgId == deptId || p.userId == session.userId)).ToList();
            }
            foreach (var pam in mps)
            {
                // 当前全局选项，判断是否存在同类非全局选项
                if (pam.orgId == null && pam.userId == null && mps.Exists(p => p.paramId == pam.paramId && (p.orgId != null || p.userId != null)))
                {
                    ids.Add(pam.id);
                }
                // 当前部门选项，判断是否存在同类用户选项
                if (pam.orgId != null && pam.userId == null && mps.Exists(p => p.paramId == pam.paramId && p.userId != null))
                {
                    ids.Add(pam.id);
                }
            }
            ids.ForEach(pid => mps.Remove(mps.Find(p => p.id == pid)));
            return mps;
        }

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="session">Session对象实体</param>
        /// <param name="mid">模块ID</param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        public List<ModuleParam> GetModuleUserParam(Token session, string mid)
        {
            using (var context = new Entities())
            {
                return context.moduleParams.Where(p => p.moduleId == mid && p.userId == session.userId).ToList();
            }
        }

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="session">Session对象实体</param>
        /// <param name="deptId">登录部门ID</param>
        /// <param name="moduleId">模块ID</param>
        /// <returns>参数集合</returns>
        public List<ModuleParam> GetModuleDeptParam(Token session, string deptId, string moduleId)
        {
            using (var context = new Entities())
            {
                return context.moduleParams.Where(p => p.moduleId == moduleId && p.orgId == deptId && p.userId == null).ToList();
            }
        }

        #endregion

    }
}
