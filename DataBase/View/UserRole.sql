use insight_base
go

if exists (select * from sysobjects where id = object_id(N'ucv_user_role') and objectproperty(id, N'isview') = 1)
drop view ucv_user_role
go


/*****视图：查询所有角色的操作权限*****/

create view ucv_user_role
as

select role_id, member_id as user_id, null as dept_id
from ucr_role_member
where member_type = 1
union
select role_id, m.user_id, null as dept_id
from ucr_role_member r
join ucg_group_member m on m.group_id = r.member_id
where r.member_type = 2
union
select role_id, m.user_id, o.parent_id as dept_id
from ucr_role_member r
join uco_org_member m on m.org_id = r.member_id
join uco_organization o on o.id = m.org_id
where r.member_type = 3

go