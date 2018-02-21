use insight_base
go


/*****初始化角色：所有用户*****/
delete ucr_role where name = '所有用户'
insert ucr_role (id, name, remark, is_builtin) 
select '9d47dc61-3b45-4f32-90cc-64e4d8f634bd', '所有用户', '内置角色，角色成员为全部用户成员', 1;

-- 初始化角色成员
insert ucr_role_member(id, member_type, role_id, member_id)
select lower(newid()), 2, '9d47dc61-3b45-4f32-90cc-64e4d8f634bd', id from ucg_group where name = 'allusers'

-- 设置功能权限
insert ucr_role_function (id, role_id, function_id, permit)
select lower(newid()), '9d47dc61-3b45-4f32-90cc-64e4d8f634bd', a.id, 1
from ucs_function a
where a.navigator_id in('ced5a90c-092e-4b38-b21d-433dfd96bfdb', '05c1b3b4-1536-4de7-864a-b98c474f438b')
  and a.id not in('c6d59daf-18c8-4a05-aa22-ba27cbc4595b', '076890fa-483a-4ebc-9168-d94367741fe9', 'edbc058a-bdc2-4108-b690-1c2e9e65ad97')

-- 设置数据权限
--insert sys_role_data (roleid, moduleid, mode, modeid, permission, creatoruserid)
--select @roleid, '5c801552-1905-452b-ae7f-e57227be70b8', 0, id, 0 from sys_data where type = 0 union all
--select @roleid, '5c801552-1905-452b-ae7f-e57227be70b8', 0, id, 1 from sys_data where type = 1
go


/*****初始化角色：系统管理员*****/
delete ucr_role where name = '系统管理员'
insert ucr_role (id, name, remark, is_builtin) 
select '1faf0cf9-c28e-427e-bbd0-107607c01306', '系统管理员', '内置角色，角色成员为系统管理员组成员', 1;

-- 初始化角色成员
insert ucr_role_member(id, member_type, role_id, member_id)
select lower(newid()), 2, '1faf0cf9-c28e-427e-bbd0-107607c01306', id from ucg_group where name = 'administers';

-- 设置功能权限
insert ucr_role_function (id, role_id, function_id, permit)
select lower(newid()), '1faf0cf9-c28e-427e-bbd0-107607c01306', a.id, 1
from ucs_function a
join ucs_navigator m on m.id = a.navigator_id
  and (a.id = 'edbc058a-bdc2-4108-b690-1c2e9e65ad97'
    or m.id in('5c801552-1905-452b-ae7f-e57227be70b8', '6c0c486f-e039-4c53-9f36-9fe262fb0d3c')
    or m.parent_id = 'bdf57601-024f-41bc-9ef6-c5f2c334df79')
  
-- 设置数据权限
declare @id0 varchar(36)
declare @id1 varchar(36)
declare @id5 varchar(36)
select @id0 = id from ucr_config where data_type = 0
select @id1 = id from ucr_config where data_type = 1
select @id5 = id from ucr_config where data_type = 4
insert ucr_role_data (id, role_id, module_id, mode, mode_id, permit)
select lower(newid()), '1faf0cf9-c28e-427e-bbd0-107607c01306', m.id, 0, @id0, 1
from ucs_navigator m
where m.alias is not null
and (m.id in('5c801552-1905-452b-ae7f-e57227be70b8', '6c0c486f-e039-4c53-9f36-9fe262fb0d3c')
    or m.parent_id = 'bdf57601-024f-41bc-9ef6-c5f2c334df79')
union all
select lower(newid()), '1faf0cf9-c28e-427e-bbd0-107607c01306', m.id, 0, @id1, 1
from ucs_navigator m
where m.alias is not null
and (m.id in('5c801552-1905-452b-ae7f-e57227be70b8', '6c0c486f-e039-4c53-9f36-9fe262fb0d3c')
    or m.parent_id = 'bdf57601-024f-41bc-9ef6-c5f2c334df79')
union all
select lower(newid()), '1faf0cf9-c28e-427e-bbd0-107607c01306', m.id, 0, @id5, 1
from ucs_navigator m
where m.alias is not null
and (m.id in('5c801552-1905-452b-ae7f-e57227be70b8', '6c0c486f-e039-4c53-9f36-9fe262fb0d3c')
    or m.parent_id = 'bdf57601-024f-41bc-9ef6-c5f2c334df79')
go