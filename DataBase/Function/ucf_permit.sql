if exists (select * from sysobjects where id = object_id(N'ucf_permit') and objectproperty(id, N'istablefunction') = 1)
drop function ucf_permit
go

/*****表值函数：获取当前登录用户有效操作权限*****/

create function ucf_permit(
@userId     varchar(36),    --当前登录用户id
@deptId     varchar(36)     --当前登录部门id
)
returns table as return select distinct
f.id,
f.navigator_id,
f.alias,
f.routes 
from
	ucs_function f
	join ucr_role_function p on p.function_id = f.id
	join ucv_user_role r on r.role_id = p.role_id 
where
	r.user_id = @userId 
	and (r.dept_id = @deptId or r.dept_id is null) 
group by
	f.id,
	f.navigator_id,
	f.alias,
	f.routes 
having
	min (p.permit) > 0 
go