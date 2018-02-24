use insight_base
go

/*****初始化角色：系统管理员*****/
delete ucr_role where name = '系统管理员'
insert ucr_role (id, tenant_id, name, remark, is_builtin) 
select '1faf0cf9-c28e-427e-bbd0-107607c01306', '00000000-0000-0000-0000-000000000000', '系统管理员', '内置角色，角色成员为系统管理员组成员', 1;

-- 初始化角色成员
insert ucr_role_member(id, member_type, role_id, member_id)
select lower(newid()), 1, '1faf0cf9-c28e-427e-bbd0-107607c01306', '00000000-0000-0000-0000-000000000000';

-- 设置功能权限
insert ucr_role_function (id, role_id, function_id, permit)
select lower(newid()), '1faf0cf9-c28e-427e-bbd0-107607c01306', a.id, 1
from ucs_function a
go