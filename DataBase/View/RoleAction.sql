USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleAction') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleAction
GO


/*****视图：查询所有角色的成员用户*****/

CREATE VIEW RoleAction
AS

with
Modules as (
  select distinct M.ID, M.ModuleGroupId as ParentId, R.RoleId, null as ActionId, null as Action, null as Permit, M.[Index], 1 as NodeType, M.ApplicationName as Name
  from SYS_Module M
  join SYS_ModuleAction A on A.ModuleId = M.ID
  join SYS_Role_Action R on R.ActionId = A.ID),
Groups as (
  select distinct G.ID, null as ParentId, M.RoleId, null as ActionId, null as Action, null as Permit, G.[Index], 0 as NodeType, G.Name
  from SYS_ModuleGroup G
  join Modules M on M.ParentId = G.ID),
Actions as (
  select case when P.ID is null then newid() else P.ID end as ID, ModuleId as ParentId, M.RoleId, A.ID as ActionId, P.Action, null as Permit, A.[Index], 2 as NodeType, A.Alias as Name
  from SYS_ModuleAction A
  join Modules M on M.ID = A.ModuleId
  left join SYS_Role_Action P on P.ActionId = A.ID)

select * from Groups union all
select * from Modules union all
select * from Actions

GO