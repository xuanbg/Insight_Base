USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_RoleModulePermit') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_RoleModulePermit
GO


/*****视图：查询所有角色的模块权限*****/

CREATE FUNCTION Get_RoleModulePermit (@RoleId UNIQUEIDENTIFIER)
RETURNS TABLE AS

RETURN

with PermModule as (
select M.ID,
       M.ModuleGroupId as ParentId,
       M.[Index] + 100 as [Index],
       case when max(isnull(P.Action, -1)) = -1 then 1 else 4 end as Type,
       M.ApplicationName as Module
from Sys_Role R
join Sys_Module M on M.ID = M.ID
  and M.ModuleGroupId is not null
join SYS_ModuleAction A on A.ModuleId = M.ID
left join SYS_RolePerm_Action P on P.ActionId = A.ID
  and P.RoleId = R.ID
where R.ID = @RoleId
group by M.ID, M.ModuleGroupId, R.ID, M.[Index], M.ApplicationName),
List as (
select distinct
       G.ID,
       null as ParentId,
       G.[Index],
       0 as Type,
       G.Name as Module
from Sys_Role R
join Sys_ModuleGroup G on G.ID = G.ID
join PermModule P on P.ParentID = G.ID
where R.ID = @RoleId
union all
select * from PermModule)

select * from List

GO