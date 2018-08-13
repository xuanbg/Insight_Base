use insight_base
go

if exists (select * from sysobjects where id = object_id(N'ucv_user_role') and objectproperty(id, N'isview') = 1)
drop view ucv_user_role
go

/*****视图：查询用户拥有的所有角色*****/

create view ucv_user_role as
select
    lower(newid()) as id, m.role_id, r.tenant_id, m.member_id as user_id, null as dept_id
from ucr_role_member m
    join ucr_role r on r.id = m.role_id
where
    m.member_type = 1 union
select
    lower(newid()) as id, m.role_id, r.tenant_id, g.user_id, null as dept_id
from ucr_role_member m
    join ucr_role r on r.id = m.role_id
    join ucg_group_member g on g.group_id = m.member_id
where
    m.member_type = 2 union
select
    lower(newid()) as id, m.role_id, r.tenant_id, o.user_id, d.parent_id as dept_id
from ucr_role_member m
    join ucr_role r on r.id = m.role_id
    join uco_org_member o on o.org_id = m.member_id
    join uco_org d on d.id = o.org_id
where
    m.member_type = 3

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
	join uco_org o on o.id = m.member_id 
	join uco_org_member r on r.org_id = o.id
	join ucb_user u on u.id = r.user_id
where
	m.member_type = 3

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
	join uco_org o on o.id = m.member_id 
where
	m.member_type = 3

go