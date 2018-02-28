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

namespace Insight.Base.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Roles : ServiceBase, IRoles
    {
        /// <summary>
        /// 为跨域请求设置响应头信息
        /// </summary>
        public void ResponseOptions()
        {
        }

        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="info">RoleInfo</param>
        /// <returns>Result</returns>
        public Result<object> AddRole(RoleInfo info)
        {
            if (!Verify("newRole")) return result;

            var role = InitRole(info);
            if (Existed(role)) return result.DataAlreadyExists();

            return DbHelper.Insert(role) ? result.Created(info) : result.DataBaseError();
        }

        /// <summary>
        /// 根据ID删除角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveRole(string id)
        {
            if (!Verify("deleteRole")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            return DbHelper.Delete(data) ? result.Success(data) : result.DataBaseError();
        }

        /// <summary>
        /// 编辑角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="info"></param>
        /// <returns>Result</returns>
        public Result<object> EditRole(string id, RoleInfo info)
        {
            if (!Verify("editRole")) return result;

            var data = GetData(info.id);
            if (data == null) return result.NotFound();

            var role = InitRole(info);
            if (Existed(role)) return result.DataAlreadyExists();

            return DbHelper.Delete(data) && DbHelper.Insert(role) ? result.Success(info) : result.DataBaseError();
        }

        /// <summary>
        /// 获取指定角色
        /// </summary>ID
        /// <param name="id">角色</param>
        /// <returns>Result</returns>
        public Result<object> GetRole(string id)
        {
            if (!Verify("getRoles")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            var role = new RoleInfo {members = new List<MemberInfo>()};
            using (var context = new Entities())
            {
                var list = context.roleMemberInfos.Where(i => i.roleId == id).ToList();
                role.members.AddRange(list.Select(i => i.memberType)
                    .Distinct()
                    .Select(type => new MemberInfo
                    {
                        id = $"00000000-0000-0000-0000-00000000000{type}",
                        nodeType = type,
                        name = type == 1 ? "用户" : type == 2 ? "用户组" : "职位"
                    }));
                var members = list.Select(i => new MemberInfo
                    {
                        id = i.id,
                        parentId = $"00000000-0000-0000-0000-00000000000{i.memberType}",
                        memberId = i.memberId,
                        name = i.name
                    }).ToList();
                role.members.AddRange(members);
            }
            role.funcs = GetRoleFuncs(id, data.appId);
            role.datas = new List<AppTree>();

            return result.Success(role);
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <param name="key">查询关键词</param>
        /// <returns>Result</returns>
        public Result<object> GetRoles(int rows, int page, string key)
        {
            if (!Verify("getRoles")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var list = from r in context.roles
                    join a in context.applications on r.appId equals a.id into temp
                    from t in temp.DefaultIfEmpty()
                    where r.tenantId == tenantId && (string.IsNullOrEmpty(key) || r.name.Contains(key) || r.remark.Contains(key))
                    select new RoleInfo
                    {
                        id = r.id,
                        appId = r.appId,
                        appName = t.alias,
                        name = r.name,
                        remark = r.remark,
                        isBuiltin = r.isBuiltin,
                        createTime = r.createTime
                    };
                var skip = rows * (page - 1);
                var roles = list.OrderBy(i => i.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(roles, list.Count());
            }
        }

        /// <summary>
        /// 根据参数组集合插入角色成员关系
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="members">成员对象集合</param>
        /// <returns>Result</returns>
        public Result<object> AddRoleMember(string id, List<RoleMember> members)
        {
            if (!Verify("addRoleMember")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            members.ForEach(i =>
            {
                i.id = Util.NewId();
                i.roleId = data.id;
                i.creatorId = userId;
                i.createTime = DateTime.Now;
            });

            return DbHelper.Insert(members) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 根据成员类型和ID删除角色成员
        /// </summary>
        /// <param name="id">角色成员ID</param>
        /// <returns>Result</returns>
        public Result<object> RemoveRoleMember(string id)
        {
            if (!Verify("removeRoleMember")) return result;

            var data = GetData(id);
            if (data == null) return result.NotFound();

            return result;
        }

        /// <summary>
        /// 根据角色ID获取角色成员用户集合
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="rows">每页行数</param>
        /// <param name="page">当前页</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberUsers(string id, int rows, int page)
        {
            if (!Verify("getRoles")) return result;

            if (page < 1 || rows > 100) return result.BadRequest();

            using (var context = new Entities())
            {
                var skip = rows * (page - 1);
                var list = context.roleUsers.Where(u => u.roleId == id);
                var members = list.OrderBy(m => m.createTime).Skip(skip).Take(rows).ToList();

                return result.Success(members, list.Count());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的组织机构列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfTitle(string id)
        {
            if (!Verify("getRoles")) return result;

            using (var context = new Entities())
            {
                var list = from o in context.organizations
                    join r in context.roleMembers.Where(i => i.roleId == id && i.memberType == 3) on o.id equals r.memberId into temp
                    from t in temp.DefaultIfEmpty()
                    where t == null
                    orderby o.index
                    select new {o.id, o.parentId, o.index, o.nodeType, o.name};
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户组列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfGroup(string id)
        {
            if (!Verify("getRoles")) return result;

            using (var context = new Entities())
            {
                var list = from g in context.groups
                    join r in context.roleMembers.Where(r => r.roleId == id && r.memberType == 2) on g.id equals r
                        .memberId
                    into temp
                    from t in temp.DefaultIfEmpty()
                    where t == null
                    orderby g.createTime
                    select new {g.id, g.name, g.remark};
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 根据角色ID获取可用的用户列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>Result</returns>
        public Result<object> GetMemberOfUser(string id)
        {
            if (!Verify("getRoles")) return result;

            using (var context = new Entities())
            {
                var list = from u in context.users
                    join r in context.roleMembers.Where(r => r.roleId == id && r.memberType == 1) on u.id equals r
                        .memberId
                    into temp
                    from t in temp.DefaultIfEmpty()
                    where !u.isInvalid && t == null
                    orderby u.createTime
                    select new {u.id, u.name, u.account, u.remark};
                return list.Any() ? result.Success(list.ToList()) : result.NoContent(new List<object>());
            }
        }

        /// <summary>
        /// 获取可用的权限资源列表
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="aid">应用ID（可为空）</param>
        /// <returns>Result</returns>
        public Result<object> GetAppTree(string id, string aid)
        {
            if (!Verify("getRoles")) return result;

            var role = new RoleInfo {funcs = new List<AppTree>(), datas = new List<AppTree>()};
            using (var context = new Entities())
            {
                var navList = context.navigators.ToList();
                var funList = context.functions.ToList();

                var apps = from a in context.applications
                    join r in context.tenantApps on a.id equals r.appId
                    where r.tenantId == tenantId && (string.IsNullOrEmpty(aid) || a.id == aid)
                    orderby a.createTime
                    select new AppTree {id = a.id, index = a.index, name = a.alias};
                role.funcs.AddRange(apps);

                var groups = from n in navList
                    join a in role.funcs on n.appId equals a.id
                    where n.parentId == null
                    orderby n.index
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.appId,
                        nodeType = 1,
                        index = n.index,
                        name = n.name
                    };
                role.funcs.AddRange(groups);

                var permits = from f in context.functions
                    join r in context.roleFunctions on f.id equals r.functionId
                    where r.roleId == id
                    select new {f.id, f.navigatorId, r.permit};
                var modules = from n in navList
                    join g in role.funcs on n.parentId equals g.id
                    let p = permits.Any(i => i.navigatorId == n.id)
                    orderby n.index
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.parentId,
                        nodeType = 2,
                        index = n.index,
                        name = n.name,
                        permit = p ? (bool?) p : null
                    };
                role.funcs.AddRange(modules);

                var functions = from f in funList
                    join m in role.funcs on f.navigatorId equals m.id
                    let p = permits.FirstOrDefault(i => i.id == f.id)?.permit
                    orderby f.index
                    select new AppTree
                    {
                        id = f.id,
                        parentId = f.navigatorId,
                        nodeType = 3,
                        index = f.index,
                        name = f.name,
                        remark = p == null ? null : p == 1 ? "允许" : "拒绝",
                        permit = p == null ? null : (bool?) (p == 1)
                    };
                role.funcs.AddRange(functions);
            }

            return result.Success(role);
        }

        /// <summary>
        /// 获取指定ID的角色
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <returns>角色</returns>
        private static Role GetData(string id)
        {
            using (var context = new Entities())
            {
                return context.roles.SingleOrDefault(i => i.id == id);
            }
        }

        /// <summary>
        /// 角色是否已存在
        /// </summary>
        /// <param name="role">角色</param>
        /// <returns>是否已存在</returns>
        private static bool Existed(Role role)
        {
            using (var context = new Entities())
            {
                return context.roles.Any(i => i.id != role.id && i.tenantId == role.tenantId && i.name == role.name);
            }
        }

        /// <summary>
        /// 构造角色数据
        /// </summary>
        /// <param name="info"></param>
        /// <returns>Role 角色数据</returns>
        private Role InitRole(RoleInfo info)
        {
            var role = new Role
            {
                id = info.id,
                tenantId = tenantId,
                appId = info.appId,
                name = info.name,
                remark = info.remark,
            };

            role.funcs = info.funcs.Where(i => i.nodeType > 2 && i.permit.HasValue).Select(i => new RoleFunction
            {
                id = Util.NewId(),
                roleId = role.id,
                functionId = i.id,
                permit = i.permit.Value ? 1 : 0,
                creatorId = userId,
                createTime = DateTime.Now
            }).ToList();

            role.datas = info.datas.Where(i => i.nodeType > 2 && i.permit.HasValue).Select(i => new RoleData
            {
                id = Util.NewId(),
                roleId = role.id,
                moduleId = i.parentId,
                modeId = i.id,
                permit = i.permit.Value ? 1 : 0,
                creatorId = userId,
                createTime = DateTime.Now
            }).ToList();

            using (var context = new Entities())
            {
                var data = context.roles.SingleOrDefault(i => i.id == role.id);
                role.isBuiltin = data?.isBuiltin ?? false;
                role.creatorId = data?.creatorId ?? userId;
                role.createTime = data?.createTime ?? DateTime.Now;
                role.members = context.roleMembers.Where(i => i.roleId == role.id).ToList();
            }

            return role;
        }

        /// <summary>
        /// 获取角色功能权限
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="aid">角色所属应用ID</param>
        /// <returns>Result</returns>
        private List<AppTree> GetRoleFuncs(string id, string aid)
        {
            var list = new List<AppTree>();
            using (var context = new Entities())
            {
                var navList = context.navigators.ToList();
                var appList = context.applications.ToList();
                var mids = (from f in context.functions
                        join p in context.roleFunctions on f.id equals p.functionId
                        join n in context.navigators on f.navigatorId equals n.id
                        join r in context.tenantApps on n.appId equals r.appId
                        where p.roleId == id && (string.IsNullOrEmpty(aid) || n.appId == aid) && r.tenantId == tenantId
                        select f.navigatorId).Distinct().ToList();
                var gids = navList.Join(mids, f => f.id, p => p, (f, p) => f.parentId).Distinct().ToList();
                var aids = navList.Join(gids, f => f.id, p => p, (f, p) => f.appId).Distinct().ToList();

                var apps = from a in appList
                    join p in aids on a.id equals p
                    orderby a.createTime
                    select new AppTree {id = a.id, name = a.alias};
                list.AddRange(apps);

                var groups = from n in navList
                    join p in gids on n.id equals p
                    orderby n.index
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.appId,
                        nodeType = 1,
                        name = n.name
                    };
                list.AddRange(groups);

                var modules = from n in navList
                    join p in mids on n.id equals p
                    orderby n.index
                    select new AppTree
                    {
                        id = n.id,
                        parentId = n.parentId,
                        nodeType = 2,
                        name = n.name
                    };
                list.AddRange(modules);

                var functions = from f in context.functions
                    join m in mids on f.navigatorId equals m
                    join p in context.roleFunctions.Where(i => i.roleId == id) on f.id equals p.functionId
                    into temp
                    from t in temp.DefaultIfEmpty()
                    orderby f.index
                    select new AppTree
                    {
                        id = f.id,
                        parentId = f.navigatorId,
                        nodeType = (t == null ? 2 : t.permit) + 3,
                        name = f.name,
                        remark = t == null ? null : (t.permit == 1 ? "允许" : "拒绝")
                    };
                list.AddRange(functions);
            }

            return list;
        }
    }
}