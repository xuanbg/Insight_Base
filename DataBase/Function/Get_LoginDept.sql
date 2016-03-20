USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_LoginDept') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_LoginDept
GO


/*****表值函数：获取登录用户的可登录部门*****/

CREATE FUNCTION Get_LoginDept(
@LoginName                 VARCHAR(32)      --用户登录名
)

RETURNS TABLE AS

RETURN

--正常职位成员用户
select D.ID, D.FullName as Name
from Sys_User U
join Sys_OrgMember M on M.UserId = U.ID
join Sys_Organization P on P.ID = M.OrgId
  and P.Validity = 1
join Sys_Organization D on D.ID = P.ParentId
where U.LoginName = @loginName

union --合并职位成员用户
select D.ID, D.FullName as Name
from Sys_User U
join Sys_OrgMember M on M.UserId = U.ID
join Sys_OrgMerger OM on OM.MergerOrgId = M.OrgId
join Sys_Organization P on P.ID = OM.OrgId
join Sys_Organization D on D.ID = P.ParentId
where U.LoginName = @loginName

GO