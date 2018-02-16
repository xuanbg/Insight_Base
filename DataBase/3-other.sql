
if exists (select * from ucbobjects where id = object_id(N'ucb_verify_image') and objectproperty(id, N'isusertable') = 1)
drop table ucb_verify_image
go

if exists (select * from ucbobjects where id = object_id(N'ucb_code_allot') and objectproperty(id, N'isusertable') = 1)
drop table ucb_code_allot
go
if exists (select * from ucbobjects where id = object_id(N'ucb_code_record') and objectproperty(id, N'isusertable') = 1)
drop table ucb_code_record
go
if exists (select * from ucbobjects where id = object_id(N'ucb_allot_record') and objectproperty(id, N'isusertable') = 1)
drop table ucb_allot_record
go
if exists (select * from ucbobjects where id = object_id(N'ucb_code_scheme') and objectproperty(id, N'isusertable') = 1)
drop table ucb_code_scheme
go

/*****编码方案*****/

/*****编码方案表*****/
create table ucb_code_scheme(
[id]               varchar(36) constraint ix_ucb_code_scheme primary key,
[name]             nvarchar(64) not null,                                                                                                  --名称
[code_format]      nvarchar(64) not null,                                                                                                  --编码格式
[serial_format]    nvarchar(16),                                                                                                           --流水码关联字符串格式
[remark]           nvarchar(max),                                                                                                          --描述
[is_invalid]       bit default 0 not null,                                                                                                 --是否失效：0、有效；1、失效
[creator_dept_id]  varchar(36) foreign key references uco_organization(id),                                                                --创建部门id
[creator_id]       varchar(36) foreign key references ucb_user(id) default '00000000-0000-0000-0000-000000000000' not null,                --创建人id
[created_time]     datetime default getdate() not null                                                                                     --创建时间
)
go

/*****编码分配记录表*****/

create table ucb_allot_record(
[id]               varchar(36) constraint ix_ucb_allot_record primary key,
[sn]               bigint identity(1,1),                                                                                                   --自增序列
[schemeid]         varchar(36) foreign key references ucb_code_scheme(id) on delete cascade not null,                                 --编码方案id
[moduleid]         varchar(36) foreign key references ucs_navigator(id) on delete cascade not null,                                      --模块注册id
[ownerid]          varchar(36) foreign key references ucb_user(id) not null,                                                          --用户id
[startnumber]      varchar(8),                                                                                                             --编码区段起始值
[endnumber]        varchar(8),                                                                                                             --编码区段结束值
[creatordeptid]    varchar(36) foreign key references uco_organization(id),                                                           --创建部门id
[creator_id]    varchar(36) foreign key references ucb_user(id) not null,                                                          --创建人id
[created_time]       datetime default getdate() not null                                                                                     --创建时间
)

/*****编码流水记录表*****/

create table ucb_code_record(
[id]               varchar(36) constraint ix_ucb_code_record primary key,
[sn]               bigint identity(1,1),                                                                                                   --自增序列
[schemeid]         varchar(36) foreign key references ucb_code_scheme(id) on delete cascade not null,                                 --编码方案id
[relationchar]     nvarchar(16),                                                                                                           --关联字符串
[serialnumber]     int not null,                                                                                                           --流水号
[businessid]       varchar(36),                                                                                                       --业务记录id
[created_time]       datetime default getdate() not null                                                                                     --创建时间
)
create nonclustered index ix_ucb_code_record_serial on ucb_code_record(schemeid, relationchar) include (serialnumber)
create nonclustered index ix_ucb_code_record_businessid on ucb_code_record(businessid)
go

/*****编码分配记录表*****/

create table ucb_code_allot(
[id]               varchar(36) constraint ix_ucb_code_allot primary key,
[sn]               bigint identity(1,1),                                                                                                   --自增序列
[schemeid]         varchar(36) foreign key references ucb_code_scheme(id) on delete cascade not null,                                 --编码方案id
[moduleid]         varchar(36) foreign key references ucs_navigator(id) on delete cascade not null,                                      --模块注册id
[ownerid]          varchar(36) foreign key references ucb_user(id) not null,                                                          --用户id
[allotnumber]      varchar(8) not null,                                                                                                    --分配流水号
[businessid]       varchar(36),                                                                                                       --业务记录id
[updatetime]       datetime,                                                                                                               --使用时间
[creatordeptid]    varchar(36) foreign key references uco_organization(id),                                                           --创建部门id
[creator_id]    varchar(36) foreign key references ucb_user(id) not null,                                                          --创建人id
[created_time]       datetime default getdate() not null                                                                                     --创建时间
)
create nonclustered index ix_ucb_code_allot_serial on ucb_code_allot(schemeid, moduleid, ownerid) include (allotnumber)
create nonclustered index ix_ucb_code_allot_businessid on ucb_code_allot(businessid)
go

/*****验证图形表*****/

create table ucb_verifyimage(
[id]               varchar(36) constraint ix_ucb_verifyimage primary key,
[sn]               bigint identity(1,1),                                                                                                   --自增序列
[type]             int not null,                                                                                                           --图形类型：0、遮罩图；1、背景图；
[name]             nvarchar(16) not null,                                                                                                  --图形名称
[path]             varchar(256) not null,                                                                                                  --图片本地路径
[created_time]       datetime default getdate() not null                                                                                     --创建时间
)
go

