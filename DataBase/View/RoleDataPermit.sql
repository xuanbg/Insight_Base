USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleDataPermit') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleDataPermit
GO


/*****视图：查询所有角色的数据权限*****/

CREATE VIEW RoleDataPermit
AS

with 
List as (
select distinct
       D.RoleId,
       G.[Index],
       G.ID as DataId,
       null as ParentId,
       0 as Type,
       G.Name as Model,
       cast(null as bit) as Permit,
	   null as Description
from SYS_ModuleGroup G
join SYS_Module M on M.ModuleGroupId = G.ID
join SYS_RolePerm_Data D on D.ModuleId = M.ID
union
select distinct
       A.RoleId,
       G.[Index],
       G.ID as DataId,
       null as ParentId,
       0 as Type,
       G.Name as Model,
       cast(null as bit) as Permit,
	   null as Description
from SYS_ModuleGroup G
join SYS_Module M on M.ModuleGroupId = G.ID
join SYS_RolePerm_DataAbs A on A.ModuleId = M.ID

union
select distinct
       D.RoleId,
       M.[Index] + 100 as [Index],
       M.ID as DataId,
       case when M.ModuleGroupId is null then M.ParentId else M.ModuleGroupId end as ParentId,
       1 as Type,
       M.Name + '数据' as Model,
       cast(null as bit) as Permit,
	   null as Description
from SYS_Module M
join SYS_RolePerm_Data D on D.ModuleId = M.ID
union
select distinct
       A.RoleId,
       M.[Index] + 100 as [Index],
       M.ID as DataId,
       case when M.ModuleGroupId is null then M.ParentId else M.ModuleGroupId end as ParentId,
       1 as Type,
       M.Name + '数据' as Model,
       cast(null as bit) as Permit,
	   null as Description
from SYS_Module M
join SYS_RolePerm_DataAbs A on A.ModuleId = M.ID

union
select D.RoleId,
       D.Mode + 201 as [Index],
       D.ID as DataId,
       D.ModuleId as ParentId,
       D.Mode + 2 as Type,
       case D.Mode when 0 then '无归属' when 1 then '仅本人' when 2 then '仅本部门' when 3 then '本部门所有' when 4 then '本机构所有' when 5 then '本根域所有' end as Model,
       cast(D.Permission as bit) as Permit,
	   case D.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_Data D
union
select A.RoleId,
       300 as [Index],
       A.ID as DataId,
       A.ModuleId as ParentId,
       4 as Type,
       O.FullName as Model,
       cast(A.Permission as bit) as Permit,
	   case A.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_DataAbs A
join SYS_Organization O on O.ID = A.OrgId
where A.OrgId is not null
union
select A.RoleId,
       301 as [Index],
       A.ID as DataId,
       A.ModuleId as ParentId,
       3 as Type,
       U.Name + '(' + U.LoginName + ')' as Model,
       cast(A.Permission as bit) as Permit,
	   case A.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_DataAbs A
join SYS_User U on U.ID = A.UserId
where A.UserId is not null
)

select newid() as ID, * from List

GO