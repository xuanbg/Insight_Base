using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using FastReport;
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

        /// <summary>
        /// 获取模块有效选项参数
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <returns>Result</returns>
        public Result<object> GetModuleParam(string id)
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                var list = context.moduleParams.Where(i => i.tenantId == tenantId && i.moduleId == id);
                var param = list.Where(i => i.deptId == null || i.userId == null || i.deptId == deptId || i.userId == userId);

                return result.Success(param.ToList());
            }
        }

        /// <summary>
        /// 保存选项数据
        /// </summary>
        /// <param name="id">业务模块ID</param>
        /// <param name="list">选项数据集合</param>
        /// <returns>Result</returns>
        public Result<object> SaveModuleParam(string id, List<Parameter> list)
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                foreach (var param in list)
                {
                    var data = context.moduleParams.SingleOrDefault(i => i.id == param.id);
                    if (data == null)
                    {
                        param.tenantId = tenantId;
                        param.moduleId = id;
                        param.creatorId = userId;
                        param.createTime = DateTime.Now;

                        context.Set<Parameter>().Add(param);
                    }
                    else
                    {
                        data.value = param.value;
                    }
                }

                try
                {
                    context.SaveChanges();
                    return result.Success();
                }
                catch (Exception)
                {
                    return result.DataBaseError();
                }
            }
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
                var regions = list.OrderBy(i => i.code).ToList();

                return result.Success(regions);
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

            var list = Params.fileList.Where(i => i.Value.appId == id && i.Value.updateTime.AddMinutes(30) > DateTime.Now).ToDictionary(i => i.Key, i => i.Value);
            if (!list.Any())
            {
                var dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                var path = DbHelper.Find<Application>(id)?.alias;
                var root = $"{dirInfo.FullName}Client\\{path}";
                Util.GetClientFiles(Params.fileList, id, root, ".dll|.exe|.frl");
                list = Params.fileList.Where(i => i.Value.appId == id).ToDictionary(i => i.Key, i => i.Value);
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

        /// <summary>
        /// 获取指定ID的电子影像数据
        /// </summary>
        /// <param name="id">影像ID</param>
        /// <returns>Result</returns>
        public Result<object> GetImageData(string id)
        {
            if (!Verify()) return result;

            var image = DbHelper.Find<Image>(id);

            return image == null ? result.NotExists() : result.Success(image);
        }

        /// <summary>
        /// 生成指定业务数据ID的报表
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <param name="templateId">模板ID</param>
        /// <param name="deptName">部门名称</param>
        /// <returns>Result</returns>
        public Result<object> BuildImageData(string id, string templateId, string deptName)
        {
            if (!Verify()) return result;

            var fr = new Report();
            var template = DbHelper.Find<Template>(templateId);
            if (template == null) return result.NotExists();

            fr.LoadFromString(template.content);
            fr.Dictionary.Connections[0].ConnectionString = new Entities().Database.Connection.ConnectionString;
            fr.SetParameterValue("BusinessId", id);
            fr.SetParameterValue("DeptName", deptName);
            fr.SetParameterValue("UserName", userName);
            fr.SetParameterValue("DeptId", deptId);
            fr.SetParameterValue("UserId", userId);
            if (!fr.Prepare()) return result.BadRequest("生成报表失败");

            Stream stream = new MemoryStream();
            fr.SavePrepared(stream);
            var bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, bytes.Length);

            var img = new ImageData
            {
                name = id,
                expand = "fpx",
                pages = fr.PreparedPages.Count,
                size = stream.Length,
                image = bytes
            };

            return result.Success(img);
        }
    }
}