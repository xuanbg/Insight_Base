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
select G.ID as ActionId,
       null as ParentID,
       P.RoleId,
       G.[Index],
       G.Name as 功能,
       0 as Action
from Sys_ModuleGroup G
join PermModuleGroup P on P.ModuleGroupId = G.ID
union all
select M.ID as ActionId,
       M.ModuleGroupId as ParentID,
       P.RoleId,
       M.[Index] + 100 as [Index],
       M.ApplicationName as 功能,
       1 as Action
from Sys_Module M
join PermModule P on P.ModuleId = M.ID
where M.ModuleGroupId is not null
union all
select A.ID as ActionId,
       case when A.EntryId is not null then A.EntryId else A.ModuleId end as ParentId,
       M.RoleId,
       A.[Index] + 200 as [Index],
       A.Alias as 功能,
       case when min(isnull(P.Action, 2)) = 2 then 2 else min(P.Action) + 3 end as Action
from SYS_ModuleAction A
join PermModule M on M.ModuleId = A.ModuleId
left join SYS_RolePerm_Action P on P.ActionId = A.ID
  and P.RoleId = M.RoleId
group by A.ID, A.EntryId, A.ModuleId, M.RoleId, A.[Index], A.Alias)

select newid() as ID, * from List

GO