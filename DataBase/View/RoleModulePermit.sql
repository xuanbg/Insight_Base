USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleModulePermit') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleModulePermit
GO


/*****视图：查询所有角色的模块权限*****/

CREATE VIEW RoleModulePermit
AS

with PermModule as (
select R.ID as RoleId,
       M.[Index] + 100 as [Index],
       M.ID as ModuleId,
       M.ModuleGroupId as ParentId,
       case when max(isnull(P.Action, -1)) = -1 then 1 else 4 end as Type,
       M.ApplicationName as Module
from Sys_Role R
join Sys_Module M on M.ID = M.ID
join SYS_ModuleAction A on A.ModuleId = M.ID
left join SYS_RolePerm_Action P on P.ActionId = A.ID
  and P.RoleId = R.ID
where M.ModuleGroupId is not null
group by M.ID, M.ModuleGroupId, R.ID, M.[Index], M.ApplicationName),
List as (
select distinct
       R.ID as RoleId,
       G.[Index],
       G.ID as ModuleId,
       null as ParentId,
       0 as Type,
       G.Name as Module
from Sys_Role R
join Sys_ModuleGroup G on G.ID = G.ID
join PermModule P on P.ParentID = G.ID
union all
select * from PermModule)

select newid() as ID, * from List

GO