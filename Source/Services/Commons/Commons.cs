using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Commons : ServiceBase, ICommons
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 获取登录用户的导航信息
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetNavigation()
        {
            if (!Verify()) return result;

            var permits = Core.GetNavigation(tenantId, appId, userId, deptId);

            return result.Success(permits);
        }

        /// <summary>
        /// 获取用户启动模块的工具栏操作信息
        /// </summary>
        /// <param name="id">模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetFunctions(string id)
        {
            if (!Verify()) return result;

            var functions = Core.GetFunctions(tenantId, userId, deptId, id);

            return result.Success(functions);
        }

        public Result<object> GetModuleParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleUserParam(string id)
        {
            throw new NotImplementedException();
        }

        public Result<object> GetModuleDeptParam(string id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取行政区划
        /// </summary>
        /// <param name="id">上级区划ID</param>
        /// <returns>Result</returns>
        public Result<object> GetRegions(string id)
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                var list = context.regions.Where(i => i.parentId == (id == "" ? null : id));
                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 获取应用客户端文件信息集合
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> GetFiles(string id)
        {
            if (!Verify()) return result;

            var list = Params.fileList.Where(i => i.Value.appId == id && i.Value.upDateTime < DateTime.Now.AddMinutes(30)).ToDictionary(i => i.Key, i => i.Value);
            if (!list.Any())
            {
                var dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                var path = DbHelper.Find<Application>(id)?.alias;
                var root = $"{dirInfo.FullName}Client\\{path}";
                Util.GetClientFiles(Params.fileList, id, root, ".dll|.exe|.frl");
                list = Params.fileList.Where(i => i.Value.appId == id && i.Value.upDateTime < DateTime.Now.AddMinutes(30)).ToDictionary(i => i.Key, i => i.Value);
            }

            return result.Success(list);
        }

        /// <summary>
        /// 获取指定名称的文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>Result</returns>
        public Result<object> GetFile(string id)
        {
            if (!Verify()) return result;

            if (!Params.fileList.ContainsKey(id)) return result.NotFound();

            var file = Params.fileList[id];
            var bytes = File.ReadAllBytes(file.fullPath);
            var str = Convert.ToBase64String(Util.Compress(bytes));

            return result.Success(str);
        }
    }
}