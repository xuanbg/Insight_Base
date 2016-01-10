USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_RoleData') AND OBJECTPROPERTY(id, N'ISPROCEDURE') = 1)
DROP PROCEDURE Get_RoleData
GO


/*****存储过程：获取所有模块和功能操作列表*****/

CREATE PROCEDURE Get_RoleData (@RoleId UNIQUEIDENTIFIER)

AS
BEGIN

SET NOCOUNT ON;

with
Mode as (
  select 0 as Mode, '无归属' as Name union all
  select 1 as Mode, '仅本人' as Name union all
  select 2 as Mode, '仅本部门' as Name union all
  select 3 as Mode, '本部门所有' as Name union all
  select 4 as Mode, '本机构所有' as Name union all
  select 5 as Mode, '本根域所有' as Name),
Groups as (
  select ID, null as ParentId, [Index], 0 as Action, Name, null as CheckState, null as Info
  from SYS_ModuleGroup),
Modules as (
  select M.ID, case when M.ModuleGroupId is null then M.ParentId else M.ModuleGroupId end as ParentId, isnull(G.[Index], 10) * 10 + M.[Index] as [Index],
  3 as Action, M.Name + '数据' as Name
  from SYS_Module M
  left join Groups G on G.ID = M.ModuleGroupId
  where M.Type = 1),
Actions as(
select case when D.ID is null then newid() else D.ID end as ID, M.ID as ParentId, M.[Index] * 10 + O.Mode as [Index], O.Mode + 4 as Action, O.Name,
case when D.Permission = 0 then 'Indeterminate' when D.Permission = 1 then 'Checked' end as CheckState,
case when D.Permission = 0 then '只读' when D.Permission = 1 then '读写' end as Info
from Modules M
join Mode O on O.Mode = O.Mode
left join SYS_RolePerm_Data D on D.ModuleId = M.ID
  and D.RoleId = @RoleId
  and D.Mode = O.Mode)

select *, CheckState as Original from Actions
union all
select distinct M.*, case when P.ID is not null then 'Checked' end as CheckState, null as Info, case when P.ID is not null then 'Checked' end as Original
from Modules M
left join SYS_RolePerm_Data P on P.ModuleId = M.ID
  and P.RoleId = @RoleId
union all
select distinct G.*, G.CheckState as Original
from Groups G
join Modules M on M.ParentId = G.ID
order by [Index]

END

GO