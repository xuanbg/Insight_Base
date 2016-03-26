USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_RoleActionPermit') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_RoleActionPermit
GO


/*****视图：查询所有角色的操作权限*****/

CREATE FUNCTION Get_RoleActionPermit (@RoleId UNIQUEIDENTIFIER)
RETURNS TABLE AS

RETURN

with 
PermAction as (
  select distinct P.RoleId, P.ActionId
  from Sys_ModuleAction A
  join SYS_RolePerm_Action P on P.ActionId = A.ID),
PermModule as (
  select distinct P.RoleId, A.ModuleId
  from SYS_ModuleAction A
  join PermAction P on P.ActionId = A.ID),
PermModuleGroup as (
  select distinct P.RoleId, M.ModuleGroupId
  from Sys_Module M
  join PermModule P on P.ModuleId = M.ID
  where M.ModuleGroupId is not null),

List as (
select G.ID,
       null as ParentId,
       G.[Index],
       0 as Type,
       G.Name as Action,
       null as Permit
from Sys_ModuleGroup G
join PermModuleGroup P on P.ModuleGroupId = G.ID
  and P.RoleId = @RoleId
union all
select M.ID,
       M.ModuleGroupId as ParentId,
       M.[Index] + 100 as [Index],
       1 as Type,
       M.ApplicationName as Action,
       null as Permit
from Sys_Module M
join PermModule P on P.ModuleId = M.ID
  and P.RoleId = @RoleId
where M.ModuleGroupId is not null
union all
select A.ID,
       case when A.EntryId is not null then A.EntryId else A.ModuleId end as ParentId,
       A.[Index] + 200 as [Index],
       case when min(isnull(P.Action, 2)) = 2 then 2 else min(P.Action) + 3 end as Type,
       A.Alias as Action,
       min(P.Action) as Permit
from SYS_ModuleAction A
join PermModule M on M.ModuleId = A.ModuleId
  and M.RoleId = @RoleId
left join SYS_RolePerm_Action P on P.ActionId = A.ID
  and P.RoleId = M.RoleId
group by A.ID, A.EntryId, A.ModuleId, M.RoleId, A.[Index], A.Alias)

select * from List

GO