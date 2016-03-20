USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'OrgInfo') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW OrgInfo
GO


/*****视图：全部组织机构*****/

CREATE VIEW OrgInfo
AS

select ID,
       SN,
       ParentId,
	   NodeType,
	   [Index],
	   Name,
	   FullName,
	   Alias,
	   Code
from Sys_Organization O
where Validity = 1
  and not exists (select MergerOrgId from Sys_OrgMerger M where M.MergerOrgId = O.ParentId)
union 
select O.ID,
       O.SN,
       OM.OrgId,
	   O.NodeType,
	   O.[Index],
	   O.Name,
	   O.FullName,
	   O.Alias,
	   O.Code
from Sys_Organization O
  join Sys_OrgMerger OM on OM.MergerOrgId = O.ParentId

GO