USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleActionPermit') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleActionPermit
GO


/*****视图：查询所有角色的操作权限*****/

CREATE VIEW RoleActionPermit
AS

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
select P.RoleId,
       G.[Index],
       G.ID as ActionId,
       null as ParentId,
       0 as Type,
       G.Name as Action,
       cast(null as bit) as Permit
from Sys_ModuleGroup G
join PermModuleGroup P on P.ModuleGroupId = G.ID
union all
select P.RoleId,
       M.[Index] + 100 as [Index],
       M.ID as ActionId,
       M.ModuleGroupId as ParentId,
       1 as Type,
       M.ApplicationName as Action,
       cast(null as bit) as Permit
from Sys_Module M
join PermModule P on P.ModuleId = M.ID
where M.ModuleGroupId is not null
union all
select M.RoleId,
       A.[Index] + 200 as [Index],
       A.ID as ActionId,
       case when A.EntryId is not null then A.EntryId else A.ModuleId end as ParentId,
       case when min(isnull(P.Action, 2)) = 2 then 2 else min(P.Action) + 3 end as Type,
       A.Alias as Action,
       cast(min(P.Action) as bit) as Permit
from SYS_ModuleAction A
join PermModule M on M.ModuleId = A.ModuleId
left join SYS_RolePerm_Action P on P.ActionId = A.ID
  and P.RoleId = M.RoleId
group by A.ID, A.EntryId, A.ModuleId, M.RoleId, A.[Index], A.Alias)

select newid() as ID, * from List

GO