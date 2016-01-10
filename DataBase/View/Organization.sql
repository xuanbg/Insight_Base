USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Organization') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW Organization
GO


/*****视图：全部组织机构*****/

CREATE VIEW Organization
AS

select ID,
       SN,
       ParentId,
	   NodeType,
	   [Index],
	   Name as 名称,
	   FullName as 全称,
	   Alias as 简称,
	   Code as 编码
from Sys_Organization O
where Validity = 1
  and not exists (select MergerOrgId from Sys_OrgMerger M where M.MergerOrgId = O.ParentId)
union 
select O.ID,
       O.SN,
       OM.OrgId,
	   O.NodeType,
	   O.[Index],
	   O.Name as 名称,
	   O.FullName as 全称,
	   O.Alias as 简称,
	   O.Code as 编码
from Sys_Organization O
  join Sys_OrgMerger OM on OM.MergerOrgId = O.ParentId

GO