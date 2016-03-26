USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_RoleDataPermit') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_RoleDataPermit
GO


/*****视图：查询所有角色的数据权限*****/

CREATE FUNCTION Get_RoleDataPermit (@RoleId UNIQUEIDENTIFIER)

RETURNS TABLE AS

RETURN

select distinct
       G.ID,
       null as ParentId,
       G.[Index],
       0 as Type,
       G.Name as Model,
       null as Permit,
	   null as Description
from SYS_ModuleGroup G
join SYS_Module M on M.ModuleGroupId = G.ID
join SYS_RolePerm_Data D on D.ModuleId = M.ID
  and D.RoleId = @RoleId
union
select distinct
       G.ID,
       null as ParentId,
       G.[Index],
       0 as Type,
       G.Name as Model,
       null as Permit,
	   null as Description
from SYS_ModuleGroup G
join SYS_Module M on M.ModuleGroupId = G.ID
join SYS_RolePerm_DataAbs A on A.ModuleId = M.ID
  and A.RoleId = @RoleId
union
select distinct
       M.ID,
       case when M.ModuleGroupId is null then M.ParentId else M.ModuleGroupId end as ParentId,
       M.[Index] + 100 as [Index],
       1 as Type,
       M.Name + '数据' as Model,
       null as Permit,
	   null as Description
from SYS_Module M
join SYS_RolePerm_Data D on D.ModuleId = M.ID
  and D.RoleId = @RoleId
union
select distinct
       M.ID,
       case when M.ModuleGroupId is null then M.ParentId else M.ModuleGroupId end as ParentId,
       M.[Index] + 100 as [Index],
       1 as Type,
       M.Name + '数据' as Model,
       null as Permit,
	   null as Description
from SYS_Module M
join SYS_RolePerm_DataAbs A on A.ModuleId = M.ID
  and A.RoleId = @RoleId
union
select D.ID,
       D.ModuleId as ParentId,
       D.Mode + 201 as [Index],
       D.Mode + 2 as Type,
       case D.Mode when 0 then '无归属' when 1 then '仅本人' when 2 then '仅本部门' when 3 then '本部门所有' when 4 then '本机构所有' when 5 then '本根域所有' end as Model,
       D.Permission as Permit,
	   case D.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_Data D
where D.RoleId = @RoleId
union
select A.ID,
       A.ModuleId as ParentId,
       300 as [Index],
       4 as Type,
       O.FullName as Model,
       A.Permission as Permit,
	   case A.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_DataAbs A
join SYS_Organization O on O.ID = A.OrgId
where A.OrgId is not null
  and A.RoleId = @RoleId
union
select A.ID,
       A.ModuleId as ParentId,
       301 as [Index],
       3 as Type,
       U.Name + '(' + U.LoginName + ')' as Model,
       A.Permission as Permit,
	   case A.Permission when 0 then '只读' when 1 then '读写' else null end as Description
from SYS_RolePerm_DataAbs A
join SYS_User U on U.ID = A.UserId
where A.UserId is not null
  and A.RoleId = @RoleId


GO