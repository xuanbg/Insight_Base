use insight_base
go

if exists (select * from sysobjects where id = object_id(N'ucv_role_member') and objectproperty(id, N'isview') = 1)
drop view ucv_role_member
go

/*****视图：查询角色成员*****/

create view ucv_role_member as
select
    m.id, m.role_id, m.member_type, m.member_id, u.name
from
	ucr_role_member m
	join ucb_user u on u.id = m.member_id 
where
	m.member_type = 1 union
select
    m.id, m.role_id, m.member_type, m.member_id, g.name
from
	ucr_role_member m
	join ucg_group g on g.id = m.member_id 
where
	m.member_type = 2 union
select
    m.id, m.role_id, m.member_type, m.member_id, o.alias as name
from
	ucr_role_member m
	join uco_organization o on o.id = m.member_id 
where
	m.member_type = 3

go