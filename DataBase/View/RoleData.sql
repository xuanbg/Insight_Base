USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleData') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleData
GO


/*****视图：查询所有角色的数据权限*****/

CREATE VIEW RoleData
AS

with
Modules as (
  select distinct M.ID, M.ModuleGroupId as ParentId, R.RoleId, 0 as Mode, '00000000-0000-0000-0000-000000000000' as ModeId, null as Permission, null as Permit, M.[Index], 1 as NodeType, M.ApplicationName as Name
  from SYS_Module M
  join SYS_ModuleAction A on A.ModuleId = M.ID
  join SYS_Role_Action R on R.ActionId = A.ID),
Groups as (
  select distinct G.ID, null as ParentId, M.RoleId, 0 as Mode, '00000000-0000-0000-0000-000000000000' as ModeId, null as Permission, null as Permit, G.[Index], 0 as NodeType, G.Name
  from SYS_ModuleGroup G
  join Modules M on M.ParentId = G.ID),
Actions as (
  select case when P.ID is null then newid() else P.ID end as ID, ModuleId as ParentId, M.RoleId, P.Mode, P.ModeId, P.Permission, null as Permit, D.[Type] as [Index], D.Type + 2 as NodeType, D.Alias as Name
  from SYS_Role_Data P
  join Modules M on M.ID = P.ModuleId
  join SYS_Data D on D.ID = P.ModeId)

select * from Groups union all
select * from Modules union all
select * from Actions

GO