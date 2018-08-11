use insight_base
go

if exists (select * from sysobjects where id = object_id(N'ucv_role_user') and objectproperty(id, N'isview') = 1)
drop view ucv_role_user
go

/*****视图：查询角色的所有成员用户*****/

create view ucv_role_user as
select
    lower(newid()) as id, m.role_id, u.name, u.account, u.remark, u.is_invalid, u.created_time
from
	ucr_role_member m
	join ucb_user u on u.id = m.member_id 
where
	m.member_type = 1 union
select
    lower(newid()) as id, m.role_id, u.name, u.account, u.remark, u.is_invalid, u.created_time
from
	ucr_role_member m
	join ucg_group g on g.id = m.member_id 
	join ucg_group_member r on r.group_id = g.id
	join ucb_user u on u.id = r.user_id
where
	m.member_type = 2 union
select
    lower(newid()) as id, m.role_id, u.name, u.account, u.remark, u.is_invalid, u.created_time
from
	ucr_role_member m
	join uco_organization o on o.id = m.member_id 
	join uco_org_member r on r.org_id = o.id
	join ucb_user u on u.id = r.user_id
where
	m.member_type = 3

go