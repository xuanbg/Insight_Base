using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Base.Common;
using Insight.WS.Base.Common.Entity;

namespace Insight.WS.Base
{
    [ServiceContract]
    interface IModule
    {
        /// <summary>
        /// 获取用户获得授权的所有模块的组信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [OperationContract]
        JsonResult GetModuleGroup();

        /// <summary>
        /// 获取用户获得授权的所有模块信息
        /// </summary>
        /// <returns>JsonResult</returns>
        [OperationContract]
        JsonResult GetUserModule();

        /// <summary>
        /// 根据ID获取模块对象实体
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [OperationContract]
        JsonResult GetModuleInfo(string id);

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>JsonResult</returns>
        [OperationContract]
        JsonResult GetAction(string id);

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id"></param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        [OperationContract]
        List<SYS_ModuleParam> GetModuleParam(string id);

        /// <summary>
        /// 获取模块个人选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        [OperationContract]
        List<SYS_ModuleParam> GetModuleUserParam(string id);

        /// <summary>
        /// 获取模块部门选项参数
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>SYS_ModuleParam List 参数集合</returns>
        [OperationContract]
        List<SYS_ModuleParam> GetModuleDeptParam(string id);

        /// <summary>
        /// 保存模块选项参数
        /// </summary>
        /// <param name="apl">新增参数集合</param>
        /// <param name="upl">更新参数集合</param>
        /// <returns></returns>
        [OperationContract]
        bool SaveModuleParam(List<SYS_ModuleParam> apl, List<SYS_ModuleParam> upl);

    }
}
