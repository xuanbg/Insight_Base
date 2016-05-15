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
  select distinct M.ID, M.ModuleGroupId as ParentId, R.RoleId, 0 as Mode, '00000000-0000-0000-0000-000000000000' as ModeId, null as Permission, null as Permit, M.[Index], 1 as NodeType, M.ApplicationName as Name, null as Description
  from SYS_Module M
  join SYS_Role_Data R on R.ModuleId = M.ID),
Groups as (
  select distinct G.ID, null as ParentId, M.RoleId, 0 as Mode, '00000000-0000-0000-0000-000000000000' as ModeId, null as Permission, null as Permit, G.[Index], 0 as NodeType, G.Name, null as Description
  from SYS_ModuleGroup G
  join Modules M on M.ParentId = G.ID),
Actions as (
  select distinct P.ID, ModuleId as ParentId, P.RoleId, P.Mode, P.ModeId, P.Permission, null as Permit, D.[Type] as [Index], D.Type + 2 as NodeType, D.Alias as Name,
  case P.Permission when 0 then '只读' when 1 then '读写' end as Description
  from SYS_Role_Data P
  join SYS_Data D on D.ID = P.ModeId
  where P.Mode = 0
  union
  select distinct P.ID, ModuleId as ParentId, P.RoleId, P.Mode, P.ModeId, P.Permission, null as Permit, 6 as [Index], 3 as NodeType, U.Name,
  case P.Permission when 0 then '只读' when 1 then '读写' end as Description
  from SYS_Role_Data P
  join SYS_User U on U.ID = P.ModeId
  where P.Mode = 1
  union
  select distinct P.ID, ModuleId as ParentId, P.RoleId, P.Mode, P.ModeId, P.Permission, null as Permit, 7 as [Index], 4 as NodeType, O.FullName as Name,
  case P.Permission when 0 then '只读' when 1 then '读写' end as Description
  from SYS_Role_Data P
  join SYS_Organization O on O.ID = P.ModeId
  where P.Mode = 2)

select * from Groups union all
select * from Modules union all
select A.* from Actions A
join Modules M on M.ID = A.ParentId
  and M.RoleId = A.RoleId

GO