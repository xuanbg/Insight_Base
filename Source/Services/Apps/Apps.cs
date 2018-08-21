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
        public void responseOptions()
        {
        }

        /// <summary>
        /// 根据关键词查询全部应用集合
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> getAllApps()
        {
            if (!verify("getApps")) return result;

            using (var context = new Entities())
            {
                var list = context.applications.Where(i => !i.isBuiltin).OrderBy(i => i.createTime).ToList();

                return result.success(list);
            }
        }

        /// <summary>
        /// 根据关键词查询租户绑定应用集合
        /// </summary>
        /// <returns>Result</returns>
        public Result<object> getApps()
        {
            if (!verify()) return result;

            using (var context = new Entities())
            {
                var list = from a in context.applications
                    join r in context.tenantApps on a.id equals r.appId
                    where !a.isBuiltin && r.tenantId == tenantId
                    orderby a.createTime
                    select a;

                return result.success(list.ToList());
            }
        }

        /// <summary>
        /// 获取指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> getApp(string id)
        {
            if (!verify("getTenants")) return result;

            var data = DbHelper.find<Application>(id);
            if (data == null) return result.notFound();

            var app = new App()
            {
                navs = new List<AppTree>()
            };

            return result.success(app);
        }

        /// <summary>
        /// 新增应用信息
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> addApp(Application app)
        {
            if (!verify("newApp")) return result;

            if (app == null) return result.badRequest();

            if (existed(app)) return result.dataAlreadyExists();

            app.id = Util.newId();
            app.creatorId = userId;
            app.createTime = DateTime.Now;

            return DbHelper.insert(app) ? result.created(app) : result.dataBaseError();
        }

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <param name="app">应用实体数据</param>
        /// <returns>Result</returns>
        public Result<object> editApp(string id, Application app)
        {
            if (!verify("editApp")) return result;

            if (app == null) return result.badRequest();

            var data = DbHelper.find<Application>(id);
            if (data == null) return result.notFound();

            data.index = app.index;
            data.name = app.name;
            data.alias = app.alias;
            data.host = app.host;
            data.tokenLife = app.tokenLife;
            data.icon = app.icon;
            data.iconurl = app.iconurl;
            data.remark = app.remark;

            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 删除指定ID的应用
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteApp(string id)
        {
            if (!verify("deleteApp")) return result;

            var data = DbHelper.find<Application>(id);
            if (data == null) return result.notFound();

            return DbHelper.delete(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 新增导航信息
        /// </summary>
        /// <param name="nav">导航实体数据</param>
        /// <returns>Result</returns>
        public Result<object> addNav(Navigator nav)
        {
            if (!verify("newNav")) return result;

            if (nav == null) return result.badRequest();

            if (existed(nav)) return result.dataAlreadyExists();

            nav.id = Util.newId();
            nav.creatorId = userId;
            nav.createTime = DateTime.Now;

            return DbHelper.insert(nav) ? result.created(nav) : result.dataBaseError();
        }

        /// <summary>
        /// 修改导航信息
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <param name="nav">应用导航实体数据</param>
        /// <returns>Result</returns>
        public Result<object> editNav(string id, Navigator nav)
        {
            if (!verify("editNav")) return result;

            if (nav == null) return result.badRequest();

            var data = DbHelper.find<Navigator>(id);
            if (data == null) return result.notFound();

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

            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 删除指定ID的导航
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteNav(string id)
        {
            if (!verify("deleteNav")) return result;

            var data = DbHelper.find<Navigator>(id);
            if (data == null) return result.notFound();

            return DbHelper.delete(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 新增功能信息
        /// </summary>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        public Result<object> addFun(Function fun)
        {
            if (!verify("newFun")) return result;

            if (fun == null) return result.badRequest();

            if (existed(fun)) return result.dataAlreadyExists();

            fun.id = Util.newId();
            fun.isVisible = true;
            fun.creatorId = userId;
            fun.createTime = DateTime.Now;

            return DbHelper.insert(fun) ? result.created(fun) : result.dataBaseError();
        }

        /// <summary>
        /// 修改功能信息
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <param name="fun">功能实体数据</param>
        /// <returns>Result</returns>
        public Result<object> editFun(string id, Function fun)
        {
            if (!verify("editFun")) return result;

            if (fun == null) return result.badRequest();

            var data = DbHelper.find<Function>(id);
            if (data == null) return result.notFound();

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

            return DbHelper.update(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 删除指定ID的功能
        /// </summary>
        /// <param name="id">功能ID</param>
        /// <returns>Result</returns>
        public Result<object> deleteFun(string id)
        {
            if (!verify("deleteFun")) return result;

            var data = DbHelper.find<Function>(id);
            if (data == null) return result.notFound();

            return DbHelper.delete(data) ? result.success() : result.dataBaseError();
        }

        /// <summary>
        /// 获取指定应用ID的导航数据
        /// </summary>
        /// <param name="id">应用ID</param>
        /// <returns>Result</returns>
        public Result<object> getNavigations(string id)
        {
            using (var context = new Entities())
            {
                var list = context.navigators.Where(i => i.appId == id).OrderBy(i => i.index);

                return result.success(list.ToList());
            }
        }

        /// <summary>
        /// 获取指定导航ID的功能集合
        /// </summary>
        /// <param name="id">导航ID</param>
        /// <returns>Result</returns>
        public Result<object> getFunctions(string id)
        {
            using (var context = new Entities())
            {
                var list = context.functions.Where(i => i.navigatorId == id).OrderBy(i => i.index);

                return result.success(list.ToList());
            }
        }

        /// <summary>
        /// 应用是否已存在
        /// </summary>
        /// <param name="app">应用实体数据</param>
        /// <returns>bool 是否已存在</returns>
        private static bool existed(Application app)
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
        private static bool existed(Navigator nav)
        {
            using (var context = new Entities())
            {
                return context.navigators.Any(i =>
                    i.id != nav.id && i.appId == nav.appId && i.parentId == nav.parentId && i.name == nav.name);
            }
        }

        /// <summary>
        /// 功能是否已存在
        /// </summary>
        /// <param name="fun">功能实体数据</param>
        /// <returns>bool 是否已存在</returns>
        private static bool existed(Function fun)
        {
            using (var context = new Entities())
            {
                return context.functions.Any(i =>
                    i.id != fun.id && i.navigatorId == fun.navigatorId && i.name == fun.name);
            }
        }
    }
}