using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Insight.Base.Common;
using Insight.Base.Common.DTO;
using Insight.Base.Common.Entity;
using Insight.Base.OAuth;
using Insight.Utils.Common;
using Insight.Utils.Entity;
using Function = Insight.Base.Common.Entity.Function;

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Apps : ServiceBase, IApps
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 根据关键词查询全部应用集合
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetAllApps()
        {
            if (!Verify("getApps")) return result;

            using (var context = new Entities())
            {
                var list = context.applications.Where(i => !i.isBuiltin).OrderBy(i => i.createTime).ToList();

                return result.Success(list);
            }
        }

        /// <summary>
        /// 根据关键词查询租户绑定应用集合
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> GetApps()
        {
            if (!Verify()) return result;

            using (var context = new Entities())
            {
                var list = from a in context.applications
                    join r in context.tenantApps on a.id equals r.appId
                    where !a.isBuiltin && r.tenantId == tenantId
                    orderby a.createTime
                    select a;

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 获取指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> GetApp(string id)
        {
            if (!Verify("getTenants")) return result;

            var data = DbHelper.Find<Application>(id);
            if (data == null) return result.NotFound();

            var app = new App()
            {
                navs = new List<AppTree>()
            };

            return result.Success(app);
        }

        /// <summary>
        /// 新增应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddApp(Application app)
        {
            if (!Verify("newApp")) return result;

            if (app == null) return result.BadRequest();

            if (Existed(app)) return result.DataAlreadyExists();

            app.id = Util.NewId();
            app.creatorId = userId;
            app.createTime = DateTime.Now;

            return DbHelper.Insert(app) ? result.Created(app) : result.DataBaseError();
        }

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditApp(string id, Application app)
        {
            if (!Verify("editApp")) return result;

            if (app == null) return result.BadRequest();

            var data = DbHelper.Find<Application>(id);
            if (data == null) return result.NotFound();

            data.index = app.index;
            data.name = app.name;
            data.alias = app.alias;
            data.host = app.host;
            data.tokenLife = app.tokenLife;
            data.icon = app.icon;
            data.iconurl = app.iconurl;
            data.remark = app.remark;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 删除指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteApp(string id)
        {
            if (!Verify("deleteApp")) return result;

            var data = DbHelper.Find<Application>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 新增导航信息
        /// </summary>
        /// <param name="nav">导航实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddNav(Navigator nav)
        {
            if (!Verify("newNav")) return result;

            if (nav == null) return result.BadRequest();

            if (Existed(nav)) return result.DataAlreadyExists();

            nav.id = Util.NewId();
            nav.creatorId = userId;
            nav.createTime = DateTime.Now;

            return DbHelper.Insert(nav) ? result.Created(nav) : result.DataBaseError();
        }

        /// <summary>
        /// 修改导航信息
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <param name="nav">应用导航实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditNav(string id, Navigator nav)
        {
            if (!Verify("editNav")) return result;

            if (nav == null) return result.BadRequest();

            var data = DbHelper.Find<Navigator>(id);
            if (data == null) return result.NotFound();

            data.parentId = nav.parentId;
            data.appId = nav.appId;
            data.index = nav.index;
            data.name = nav.name;
            data.alias = nav.alias;
            data.url = nav.url;
            data.iconurl = nav.iconurl;
            data.icon = nav.icon;
            data.remark = nav.remark;
            data.isDefault = nav.isDefault;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 删除指定ID的导航
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteNav(string id)
        {
            if (!Verify("deleteNav")) return result;

            var data = DbHelper.Find<Navigator>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 新增功能信息
        /// </summary>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        public Result<object> AddFun(Function fun)
        {
            if (!Verify("newFun")) return result;

            if (fun == null) return result.BadRequest();

            if (Existed(fun)) return result.DataAlreadyExists();

            fun.id = Util.NewId();
            fun.isVisible = true;
            fun.creatorId = userId;
            fun.createTime = DateTime.Now;

            return DbHelper.Insert(fun) ? result.Created(fun) : result.DataBaseError();
        }

        /// <summary>
        /// 修改功能信息
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        public Result<object> EditFun(string id, Function fun)
        {
            if (!Verify("editFun")) return result;

            if (fun == null) return result.BadRequest();

            var data = DbHelper.Find<Function>(id);
            if (data == null) return result.NotFound();

            data.navigatorId = fun.navigatorId;
            data.index = fun.index;
            data.name = fun.name;
            data.alias = fun.alias;
            data.routes = fun.routes;
            data.url = fun.url;
            data.iconurl = fun.iconurl;
            data.icon = fun.icon;
            data.remark = fun.remark;
            data.isBegin = fun.isBegin;
            data.isShowText = fun.isShowText;
            data.isVisible = fun.isVisible;

            return DbHelper.Update(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 删除指定ID的功能
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <returns>Result</returns>
        public Result<object> DeleteFun(string id)
        {
            if (!Verify("deleteFun")) return result;

            var data = DbHelper.Find<Function>(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result.Success() : result.DataBaseError();
        }

        /// <summary>
        /// 获取指定应用ID的导航数据
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> GetNavigations(string id)
        {
            using (var context = new Entities())
            {
                var list = context.navigators.Where(i => i.appId == id).OrderBy(i => i.index);

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 获取指定导航ID的功能集合
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        public Result<object> GetFunctions(string id)
        {
            using (var context = new Entities())
            {
                var list = context.functions.Where(i => i.navigatorId == id).OrderBy(i => i.index);

                return result.Success(list.ToList());
            }
        }

        /// <summary>
        /// 应用是否已存在
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>bool 是否已存在</returns>
        private static bool Existed(Application app)
        {
            using (var context = new Entities())
            {
                return context.applications.Any(i => i.id != app.id && (i.name == app.name || i.alias == app.alias));
            }
        }

        /// <summary>
        /// 导航是否已存在
        /// </summary>
        /// <param name="nav">导航实体数据</param>
        /// <returns>bool 是否已存在</returns>
        private static bool Existed(Navigator nav)
        {
            using (var context = new Entities())
            {
                return context.navigators.Any(i => i.id != nav.id && i.appId == nav.appId && i.parentId == nav.parentId && i.name == nav.name);
            }
        }

        /// <summary>
        /// 功能是否已存在
        /// </summary>
        /// <param name="fun">功能实体数据</param>
        /// <returns>bool 是否已存在</returns>
        private static bool Existed(Function fun)
        {
            using (var context = new Entities())
            {
                return context.functions.Any(i => i.id != fun.id && i.navigatorId == fun.navigatorId && i.name == fun.name);
            }
        }
    }
}
