USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_RoleAction') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_RoleAction
GO


/*****表值函数：获取所有模块和功能操作列表*****/

CREATE FUNCTION Get_RoleAction (@RoleId UNIQUEIDENTIFIER)

RETURNS TABLE AS

RETURN

with
Groups as (
  select ID, null as ParentId, [Index], 0 as Type, Name, null as Permit, null as Description
  from SYS_ModuleGroup),
Modules as (
  select M.ID, G.ID as ParentId, isnull(G.[Index], 10) * 10 + M.[Index] as [Index],
  1 as Type, M.ApplicationName as Name, case when A.ModuleId is not null then 1 end as Permit, null as Description
  from SYS_Module M
  join Groups G on G.ID = M.ModuleGroupId
  left join(
  select distinct A.ModuleId
  from SYS_ModuleAction A
  join SYS_RolePerm_Action P on P.ActionId = A.ID
    and P.RoleId = @RoleId
  ) A on A.ModuleId = M.ID),
Actions as (
  select A.ID, M.ID as ParentId, M.[Index] * 20 + A.[Index] as [Index], 2 as Type, A.Alias as Name,
  P.Action as Permit, case when P.Action = 0 then '拒绝' when P.Action = 1 then '允许' end as Description
  from SYS_ModuleAction A
  join Modules M on M.ID = A.ModuleId
  left join SYS_RolePerm_Action P on P.ActionId = A.ID)


select *, Permit as state from Actions
union all
select distinct M.*, null as state
from Modules M
join Actions A on A.ParentId = M.ID
union all
select distinct G.*, null as state
from Groups G
join Modules M on M.ParentId = G.ID
join Actions A on A.ParentId = M.ID

GO