using System.Data.Entity;

namespace Insight.Base.Common.Entity
{
    public class Entities : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Entities() : base("name=Entities")
        {
        }

        /// <summary>
        /// 模型创建时
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        /// <summary>
        /// 行政区划
        /// </summary>
        public virtual DbSet<Region> regions { get; set; }

        /// <summary>
        /// 租户
        /// </summary>
        public virtual DbSet<Tenant> tenants { get; set; }

        /// <summary>
        /// 租户-应用关系
        /// </summary>
        public virtual DbSet<TenantApp> tenantApps { get; set; }

        /// <summary>
        /// 租户-用户关系
        /// </summary>
        public virtual DbSet<TenantUser> tenantUsers { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public virtual DbSet<User> users { get; set; }

        /// <summary>
        /// 用户组
        /// </summary>
        public virtual DbSet<Group> groups { get; set; }

        /// <summary>
        /// 用户组成员
        /// </summary>
        public virtual DbSet<GroupMember> groupMembers { get; set; }

        /// <summary>
        /// 组织机构
        /// </summary>
        public virtual DbSet<Organization> organizations { get; set; }

        /// <summary>
        /// 组织机构成员
        /// </summary>
        public virtual DbSet<OrgMember> orgMembers { get; set; }

        /// <summary>
        /// 数据权限设置
        /// </summary>
        public virtual DbSet<DataConfig> dataConfigs { get; set; }

        /// <summary>
        /// 日志记录
        /// </summary>
        public virtual DbSet<Log> logs { get; set; }

        /// <summary>
        /// 日志规则
        /// </summary>
        public virtual DbSet<LogRule> logRules { get; set; }

        /// <summary>
        /// 应用
        /// </summary>
        public virtual DbSet<Application> applications { get; set; }

        /// <summary>
        /// 导航
        /// </summary>
        public virtual DbSet<Navigator> navigators { get; set; }

        /// <summary>
        /// 功能
        /// </summary>
        public virtual DbSet<Function> functions { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public virtual DbSet<Role> roles { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public virtual DbSet<RoleMember> roleMembers { get; set; }

        /// <summary>
        /// 角色功能权限
        /// </summary>
        public virtual DbSet<RoleFunction> roleFunctions { get; set; }

        /// <summary>
        /// 角色数据权限
        /// </summary>
        public virtual DbSet<RoleData> roleDatas { get; set; }

        /// <summary>
        /// 角色成员
        /// </summary>
        public virtual DbSet<RoleMemberInfo> roleMemberInfos { get; set; }

        /// <summary>
        /// 角色成员用户
        /// </summary>
        public virtual DbSet<RoleUser> roleUsers { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public virtual DbSet<UserRole> userRoles { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public virtual DbSet<Category> categories { get; set; }

        /// <summary>
        /// 电子影像
        /// </summary>
        public virtual DbSet<ImageData> imageDatas { get; set; }

        /// <summary>
        /// 模块选项
        /// </summary>
        public virtual DbSet<ModuleParam> moduleParams { get; set; }
    }
}