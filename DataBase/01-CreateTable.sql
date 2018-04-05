
if exists (select * from sysobjects where id = object_id('ibr_instanc_user') and objectproperty(id, 'isusertable') = 1)
drop table ibr_instanc_user
go
if exists (select * from sysobjects where id = object_id('ibr_instances') and objectproperty(id, 'isusertable') = 1)
drop table ibr_instances
go

if exists (select * from sysobjects where id = object_id('ibr_schedular') and objectproperty(id, 'isusertable') = 1)
drop table ibr_schedular
go
if exists (select * from sysobjects where id = object_id('ibr_member') and objectproperty(id, 'isusertable') = 1)
drop table ibr_member
go
if exists (select * from sysobjects where id = object_id('ibr_entity') and objectproperty(id, 'isusertable') = 1)
drop table ibr_entity
go
if exists (select * from sysobjects where id = object_id('ibr_period') and objectproperty(id, 'isusertable') = 1)
drop table ibr_period
go
if exists (select * from sysobjects where id = object_id('ibr_definition') and objectproperty(id, 'isusertable') = 1)
drop table ibr_definition
go
if exists (select * from sysobjects where id = object_id('ibr_templates') and objectproperty(id, 'isusertable') = 1)
drop table ibr_templates
go
if exists (select * from sysobjects where id = object_id('ibr_rule') and objectproperty(id, 'isusertable') = 1)
drop table ibr_rule
go

if exists (select * from sysobjects where id = object_id(N'ibd_param') and objectproperty(id, N'isusertable') = 1)
drop table ibd_param
go
if exists (select * from sysobjects where id = object_id('ibd_image') and objectproperty(id, 'isusertable') = 1)
drop table ibd_image
go
if exists (select * from sysobjects where id = object_id('ibd_region') and objectproperty(id, 'isusertable') = 1)
drop table ibd_region
go
if exists (select * from sysobjects where id = object_id('ibd_category') and objectproperty(id, 'isusertable') = 1)
drop table ibd_category
go


/*****基础分类表*****/
create table ibd_category(
[id]               varchar(36) constraint ix_ibd_category primary key,
[parent_id]        varchar(36),                                                                                                            --父分类id
[tenant_id]        varchar(36) foreign key references ucb_tenant(id),                                                                      --租户ID
[module_id]        varchar(36) not null,                                                                                                   --模块注册id
[index]            int default 0 not null,                                                                                                 --序号
[code]             varchar(32),                                                                                                            --编码
[name]             nvarchar(64) not null,                                                                                                  --名称
[alias]            nvarchar(16),                                                                                                           --别名/简称
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
create nonclustered index ix_ibd_category_parent_id on ibd_category(parent_id)
go

/*****行政区划表*****/
create table ibd_region(
[id]               varchar(36) constraint ix_ibd_region primary key,
[parent_id]        varchar(36),                                                                                                            --父级ID
[grade]            int not null,                                                                                                           --级别
[code]             varchar(16),                                                                                                            --编码
[name]             nvarchar(64) not null,                                                                                                  --名称
[alias]            nvarchar(64),                                                                                                           --别名/简称
)
create nonclustered index ix_ibd_region_parent_id on ibd_region(parent_id)
go

/*****电子影像数据表*****/

/*****电子影像表*****/
create table ibd_image(
[id]               varchar(36) constraint ix_ibd_image primary key,
[tenant_id]        varchar(36) foreign key references ucb_tenant(id) not null,                                                             --租户ID
[category_id]      varchar(36) foreign key references ibd_category(id) on delete cascade,                                                  --分类id
[image_type]       int default 0 not null,                                                                                                 --类型：0、附件；1、收据；2、发票；3、付款单；4、报销单；5、入库单；6、出库单
[code]             varchar(32),                                                                                                            --编码，可索引
[name]             nvarchar(64) not null,                                                                                                  --名称
[expand]           varchar(8),                                                                                                             --扩展名
[secrec]           varchar(36),                                                                                                            --涉密等级，字典
[pages]            int,                                                                                                                    --页数
[size]             bigint,                                                                                                                 --文件字节数
[file_path]        nvarchar(max),                                                                                                          --存放路径
[image]            image not null,                                                                                                         --电子影像内容
[remark]           nvarchar(max),                                                                                                          --描述
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
create nonclustered index ix_ibd_image_code on ibd_image(code)
go


/*****模块选项配置表*****/
create table ibd_param(
[id]               varchar(36) constraint ix_ibd_param primary key,
[tenant_id]        varchar(36) foreign key references ucb_tenant(id) not null,                                                             --租户ID
[module_id]        varchar(36) not null,                                                                                                   --模块注册id
[param_id]         varchar(36) not null,                                                                                                   --选项id
[name]             nvarchar(64) not null,                                                                                                  --选项名称
[value]            nvarchar(max),                                                                                                          --选项参数值
[org_id]           varchar(36) foreign key references uco_organization(id),                                                                --生效机构id
[user_id]          varchar(36) foreign key references ucb_user(id),                                                                        --生效用户id
[remark]           nvarchar(max),                                                                                                          --描述
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
create nonclustered index ix_ibd_param_module_id on ibd_param(module_id)
go


/*****报表数据表*****/

/*****分期规则表*****/
create table ibr_rule(
[id]               varchar(36) constraint ix_ibr_rule primary key,
[tenant_id]        varchar(36) foreign key references ucb_tenant(id),                                                                      --租户ID
[cycle_type]       int not null,                                                                                                           --报表周期类型：1、年；2、月；3、周；4、日
[name]             nvarchar(64) not null,                                                                                                  --规则名称
[cycle]            int,                                                                                                                    --周期数
[start_time]       datetime,                                                                                                               --会计分期起始时间
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****模板表*****/
create table ibr_templates(
[id]               varchar(36) constraint ix_ibr_templates primary key,
[tenant_id]        varchar(36) foreign key references ucb_tenant(id),                                                                      --租户ID
[category_id]      varchar(36) foreign key references ibd_category(id) on delete cascade,                                                  --报表分类id,
[name]             nvarchar(64) not null,                                                                                                  --模板名称
[content]          text not null,                                                                                                          --模板内容
[remark]           nvarchar(max),                                                                                                          --描述
[is_builtin]       bit default 0 not null,                                                                                                 --是否预置：0、自定；1、预置
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****报表定义表*****/
create table ibr_definition(
[id]               varchar(36) constraint ix_ibr_definition primary key,
[tenant_id]        varchar(36) foreign key references ucb_tenant(id) not null,                                                             --租户ID
[category_id]      varchar(36) foreign key references ibd_category(id) on delete cascade not null,                                         --报表分类id,
[template_id]      varchar(36) foreign key references ibr_templates(id) not null,                                                          --报表模板id
[name]             nvarchar(64) not null,                                                                                                  --报表名称
[mode]             int default 0 not null,                                                                                                 --统计模式：0、时段；1、时点；2、当前
[delay]            int default 2 not null,                                                                                                 --延时小时数（正数延后，负数提前）
[report_type]      int default 0 not null,                                                                                                 --报表类型：0、组织机构；1、个人私有
[data_source]      int default 0 not null,                                                                                                 --数据源：0、系统；1、模板
[remark]           nvarchar(max),                                                                                                          --描述
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****报表分期表*****/
create table ibr_period(
[id]               varchar(36) constraint ix_ibr_period primary key,
[report_id]        varchar(36) foreign key references ibr_definition(id) on delete cascade not null,                                       --报表定义id
[rule_id]          varchar(36) foreign key references ibr_rule(id) not null                                                               --分期规则id
)
go

/*****会计主体表*****/
create table ibr_entity(
[id]               varchar(36) constraint ix_ibr_entity primary key,
[report_id]        varchar(36) foreign key references ibr_definition(id) on delete cascade not null,                                       --报表定义id
[org_id]           varchar(36)  not null,                                                                                                  --组织机构（会计主体）id
[name]             nvarchar(32) not null                                                                                                   --会计主体名称
)
go

/*****报送人员表*****/
create table ibr_member(
[id]               varchar(36) constraint ix_ibr_member primary key,
[entity_id]        varchar(36) foreign key references ibr_entity(id) on delete cascade not null,                                           --会计主体id 
[role_id]          varchar(36) not null,                                                                                                   --角色id
[name]             nvarchar(64) not null                                                                                                   --角色名称
)
go

/*****计划任务表*****/
create table ibr_schedular(
[id]               varchar(36) constraint ix_ibr_schedular primary key,
[report_id]        varchar(36) foreign key references ibr_definition(id) on delete cascade not null,                                       --报表定义id
[rule_id]          varchar(36) foreign key references ibr_rule(id) not null,                                                              --分期规则id
[build_time]       datetime,                                                                                                               --报表计划生成时间
)
go

/*****报表实例表*****/
create table ibr_instances(
[id]               varchar(36) constraint ix_ibr_instances primary key,
[report_id]        varchar(36) foreign key references ibr_definition(id) on delete cascade not null,                                       --报表定义id
[name]             nvarchar(64) not null,                                                                                                  --报表名称
[content]          image not null,                                                                                                         --实例内容
[creator_dept_id]  varchar(36),                                                                                                            --创建部门id
[creator]          nvarchar(32),                                                                                                           --创建人姓名
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****实例报送人员表*****/
create table ibr_instanc_user(
[id]               varchar(36) constraint ix_ibr_instanc_user primary key,
[instance_id]      varchar(36) foreign key references ibr_instances(id) on delete cascade not null,                                        --实例表id
[user_id]          varchar(36) foreign key references ucb_user(id) not null                                                                --用户id
)
go

