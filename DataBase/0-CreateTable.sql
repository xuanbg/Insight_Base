use insight_base
go

if exists (select * from sysobjects where id = object_id(N'ibs_log') and objectproperty(id, N'isusertable') = 1)
drop table ibs_log
go
if exists (select * from sysobjects where id = object_id(N'ibs_log_rule') and objectproperty(id, N'isusertable') = 1)
drop table ibs_log_rule
go

if exists (select * from sysobjects where id = object_id(N'ucr_role_data') and objectproperty(id, N'isusertable') = 1)
drop table ucr_role_data
go
if exists (select * from sysobjects where id = object_id(N'ucr_role_function') and objectproperty(id, N'isusertable') = 1)
drop table ucr_role_function
go
if exists (select * from sysobjects where id = object_id(N'ucr_role_member') and objectproperty(id, N'isusertable') = 1)
drop table ucr_role_member
go
if exists (select * from sysobjects where id = object_id(N'ucr_role') and objectproperty(id, N'isusertable') = 1)
drop table ucr_role
go

if exists (select * from sysobjects where id = object_id(N'ucc_data_conf') and objectproperty(id, N'isusertable') = 1)
drop table ucc_data_conf
go
if exists (select * from sysobjects where id = object_id(N'ucc_param') and objectproperty(id, N'isusertable') = 1)
drop table ucc_param
go
if exists (select * from sysobjects where id = object_id(N'ucs_function') and objectproperty(id, N'isusertable') = 1)
drop table ucs_function
go
if exists (select * from sysobjects where id = object_id(N'ucs_navigator') and objectproperty(id, N'isusertable') = 1)
drop table ucs_navigator
go
if exists (select * from sysobjects where id = object_id(N'ucs_application') and objectproperty(id, N'isusertable') = 1)
drop table ucs_application
go

if exists (select * from sysobjects where id = object_id(N'uco_org_member') and objectproperty(id, N'isusertable') = 1)
drop table uco_org_member
go
if exists (select * from sysobjects where id = object_id(N'uco_organization') and objectproperty(id, N'isusertable') = 1)
drop table uco_organization
go

if exists (select * from sysobjects where id = object_id(N'ucg_group_member') and objectproperty(id, N'isusertable') = 1)
drop table ucg_group_member
go
if exists (select * from sysobjects where id = object_id(N'ucg_group') and objectproperty(id, N'isusertable') = 1)
drop table ucg_group
go
if exists (select * from sysobjects where id = object_id(N'ucb_user') and objectproperty(id, N'isusertable') = 1)
drop table ucb_user
go


/*****组织机构、用户和用户组*****/

/*****用户表*****/
create table ucb_user(
[id]               varchar(36) constraint ix_ucb_user primary key,                                                                         --此id与主数据id相同
[name]             nvarchar(64) not null,                                                                                                  --姓名/昵称
[account]          nvarchar(32) constraint ix_ucb_user_account unique not null,                                                            --登录账号
[mobile]           varchar(11) constraint ix_ucb_user_mobile unique,                                                                       --手机号
[email]            nvarchar(64) constraint ix_ucb_user_email unique,                                                                       --注册邮箱
[password]         varchar(32) default 'e10adc3949ba59abbe56e057f20f883e' not null,                                                        --登录密码，保存密码的md5值，初始密码123456
[pay_pw]           varchar(32),                                                                                                            --支付密码，保存密码的md5值
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_id]       varchar(36) default '00000000-0000-0000-0000-000000000000' not null,                                                    --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****用户组表*****/
create table ucg_group(
[id]               varchar(36) constraint ix_ucg_group primary key,
[name]             nvarchar(64) not null,                                                                                                  --用户组名称
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[is_visible]       bit default 1 not null,                                                                                                 --是否可见：0、不可见；1、可见
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****用户组成员表*****/
create table ucg_group_member(
[id]               varchar(36) constraint ix_ucg_group_member primary key,
[group_id]         varchar(36) foreign key references ucg_group(id) on delete cascade not null,                                            --用户组id
[user_id]          varchar(36) foreign key references ucb_user(id) on delete cascade not null,                                             --用户id
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****组织机构表*****/
create table uco_organization(
[id]               varchar(36) constraint ix_uco_organization primary key,
[parent_id]        varchar(36) foreign key references uco_organization(id),                                                                --父节点id
[node_type]        int not null,                                                                                                           --节点类型：1、机构；2、部门；3、岗位
[index]            int not null,                                                                                                           --序号
[code]             varchar(32),                                                                                                            --编码
[name]             nvarchar(32) not null,                                                                                                  --名称
[alias]            nvarchar(16),                                                                                                           --别名/简称
[fullname]         nvarchar(32),                                                                                                           --全称
[position_id]      varchar(36),                                                                                                            --职能id，字典
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****职位成员表*****/
create table uco_org_member(
[id]               varchar(36) constraint ix_uco_org_member primary key,
[org_id]           varchar(36) foreign key references uco_organization(id) on delete cascade not null,                                    --组织机构（职位）id
[user_id]          varchar(36) foreign key references ucb_user(id) on delete cascade not null,                                            --用户id
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,               --创建人id
[created_time]     datetime default getdate() not null                                                                                    --创建时间
)
go


/*****模块注册数据*****/

/*****应用表*****/
create table ucs_application(
[id]               varchar(36) constraint ix_ucs_application primary key,
[name]             nvarchar(16) not null,                                                                                                  --应用名称
[alias]            nvarchar(16) not null,                                                                                                  --应用别名
[host]             varchar(128),                                                                                                           --域名
[token_life]       int default 24 not null,                                                                                                --令牌生存周期(小时)
[icon]             image,                                                                                                                  --图标
[iconurl]          varchar(128),                                                                                                           --图标url
[remark]           nvarchar(max),                                                                                                          --描述
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****导航表*****/
create table ucs_navigator(
[id]               varchar(36) constraint ix_ucs_navigator primary key,
[parent_id]        varchar(36),                                                                                                            --上级导航id
[app_id]           varchar(36) foreign key references ucs_application(id) on delete cascade not null,                                      --应用id
[index]            int default 0 not null,                                                                                                 --序号
[name]             nvarchar(16) not null,                                                                                                  --导航名称
[alias]            nvarchar(16) not null,                                                                                                  --应用名称
[class_name]       varchar(128),                                                                                                           --控制器命名空间
[file_path]        nvarchar(max),                                                                                                          --文件路径
[iconurl]          varchar(128),                                                                                                           --图标url
[icon]             image,                                                                                                                  --图标
[remark]           nvarchar(max),                                                                                                          --描述
[is_default]       bit default 0 not null,                                                                                                 --是否默认启动：0、否；1、是
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****模块功能表*****/
create table ucs_function(
[id]               varchar(36) constraint ix_ucs_function primary key,
[navigator_id]     varchar(36) foreign key references ucs_navigator(id) on delete cascade not null,                                        --导航id
[index]            int default 0 not null,                                                                                                 --序号
[name]             varchar(64) not null,                                                                                                   --名称
[alias]            nvarchar(16) not null,                                                                                                  --别名/简称
[iconurl]          varchar(128),                                                                                                           --图标url
[icon]             image,                                                                                                                  --图标
[remark]           nvarchar(max),                                                                                                          --描述
[is_begin]         bit default 0 not null,                                                                                                 --是否开始分组：0、否；1、是
[is_hide_text]     bit default 0 not null,                                                                                                 --是否隐藏文字：0、显示；1、隐藏
[is_visible]       bit default 1 not null,                                                                                                 --是否可见：0、不可见；1、可见
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****模块选项配置表*****/
create table ucc_param(
[id]               varchar(36) constraint ix_ucc_param primary key,
[module_id]        varchar(36) foreign key references ucs_navigator(id) on delete cascade not null,                                        --模块注册id
[param_id]         varchar(36) not null,                                                                                                   --选项id
[name]             nvarchar(64) not null,                                                                                                  --选项名称
[value]            nvarchar(max),                                                                                                          --选项参数值
[org_id]           varchar(36) foreign key references uco_organization(id),                                                                --生效机构id
[user_id]          varchar(36) foreign key references ucb_user(id),                                                                        --生效用户id
[remark]           nvarchar(max),                                                                                                           --描述
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****数据配置表*****/
create table ucc_data_conf(
[id]               varchar(36) constraint ix_ucc_data_conf primary key,
[data_type]        int not null,                                                                                                           --类型：0、无归属；1、仅本人；2、仅本部门；3、部门所有；4、机构所有；5、根域所有
[alias]            nvarchar(16) not null,                                                                                                  --别名/简称
)
go


/*****角色权限数据表*****/

/*****角色表*****/
create table ucr_role(
[id]               varchar(36) constraint ix_ucr_role primary key,
[name]             nvarchar(64) not null,                                                                                                  --名称
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****角色成员表*****/
create table ucr_role_member(
[id]               varchar(36) constraint ix_ucr_role_member primary key,
[member_type]      int default 1 not null,                                                                                                 --类型：1、用户；2、用户组；3、岗位；
[role_id]          varchar(36) foreign key references ucr_role(id) on delete cascade not null,                                             --角色id
[member_id]        varchar(36) not null,                                                                                                   --成员id
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****角色操作权限表*****/
create table ucr_role_function(
[id]               varchar(36) constraint ix_ucr_role_function primary key,
[role_id]          varchar(36) foreign key references ucr_role(id) on delete cascade not null,                                             --角色id
[function_id]      varchar(36) foreign key references ucs_function(id) on delete cascade not null,                                         --功能id
[permit]           int default 1 not null,                                                                                                 --操作行为：0、拒绝；1、允许
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****角色数据权限表*****/
create table ucr_role_data(
[id]               varchar(36) constraint ix_ucr_role_data primary key,
[role_id]          varchar(36) foreign key references ucr_role(id) on delete cascade not null,                                             --角色id
[module_id]        varchar(36) foreign key references ucs_navigator(id) on delete cascade not null,                                        --业务模块id
[mode_id]          varchar(36) not null,                                                                                                   --模式id或部门/用户id（绝对模式）
[mode]             int default 0 not null,                                                                                                 --授权模式：0、相对模式；1、用户模式；2、部门模式
[permit]           int default 0 not null,                                                                                                 --权限：0、只读；1、读写
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go


/*****日志数据表*****/

/*****日志规则表*****/
create table ibs_log_rule(
[id]               varchar(36) constraint ix_ibs_log_rule primary key,
[level]            int default 0 not null,                                                                                                 --日志等级：0、emergency；1、alert；2、critical；3、error；4、warning；5、notice；6、informational；7、debug
[code]             varchar(6) not null,                                                                                                    --操作代码
[source]           nvarchar(16),                                                                                                           --事件来源
[action]           nvarchar(16),                                                                                                           --操作名称
[message]          nvarchar(128),                                                                                                          --日志默认内容
[is_file]          bit default 1 not null,                                                                                                 --是否写到文件：0、否；1、是
[creator_id]       varchar(36) not null,                                                                                                   --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****日志表*****/

create table ibs_log(
[id]               varchar(36) constraint ix_ibs_log primary key,
[level]            int not null,                                                                                                           --日志等级：0、emergency；1、alert；2、critical；3、error；4、warning；5、notice；6、informational；7、debug
[code]             varchar(6),                                                                                                             --操作代码
[source]           nvarchar(16) not null,                                                                                                  --事件来源
[action]           nvarchar(16) not null,                                                                                                  --操作名称
[message]          nvarchar(max),                                                                                                          --日志内容
[key]              varchar(64),                                                                                                            --关键词
[user_id]          varchar(36),                                                                                                            --来源用户id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go


/*****初始化用户：系统管理员，密码：admin*****/
insert ucb_user (id, name, account, is_builtin)
select '00000000-0000-0000-0000-000000000000', '系统管理员', 'admin', 1
go

/*****初始化用户组：所有用户和系统管理员组*****/
insert ucg_group (id, name, remark, is_builtin, is_visible)
select lower(newid()), 'allusers', '所有用户', 1, 0 union all
select lower(newid()), 'administers', '系统管理员组', 1, 1
go

/*****初始化组成员：系统管理员*****/
insert ucg_group_member (id, group_id, user_id)
select lower(newid()), id, '00000000-0000-0000-0000-000000000000' from ucg_group
go

/*****初始化数据权限定义*****/
insert ucc_data_conf (id, data_type, alias)
select lower(newid()), 0, '无归属' union all
select lower(newid()), 1, '本人' union all
select lower(newid()), 2, '本部门' union all
select lower(newid()), 3, '部门所有' union all
select lower(newid()), 4, '机构所有'
go

/*****初始化日志规则*****/
insert ibs_log_rule (id, code, level, source, action, message, creator_id)
select lower(newid()), '200101', 2, '系统平台', 'sqlquery', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '200102', 2, '系统平台', 'sqlnonquery', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '200103', 2, '系统平台', 'sqlscalar', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '200104', 2, '系统平台', 'sqlexecute', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '200105', 2, '系统平台', 'sqlexecute', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '300601', 3, '日志服务', '新增规则', '插入数据失败', '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '300602', 3, '日志服务', '删除规则', '删除数据失败', '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '300603', 3, '日志服务', '编辑规则', '更新数据失败', '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '500101', 5, '系统平台', '接口验证', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '600601', 6, '日志服务', '新增规则', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '600602', 6, '日志服务', '删除规则', null, '00000000-0000-0000-0000-000000000000' union all
select lower(newid()), '600603', 6, '日志服务', '编辑规则', null, '00000000-0000-0000-0000-000000000000'
go
