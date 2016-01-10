USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'TitleMember') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW TitleMember
GO


/*****视图：查询所有组织节点的成员用户*****/

CREATE VIEW TitleMember
AS

--正常职位下用户成员
select PM.ID,
       U.Name as 用户名,
       U.LoginName as 登录名,
       case when U.Validity = 1 then '正常' else '封禁' end as 状态,
       PM.OrgId as TitleId
from Sys_User U
join Sys_OrgMember PM on PM.UserId = U.ID
join Sys_Organization O on O.ID = PM.OrgId
  and O.Validity = 1

union --被合并职位下用户成员
select PM.ID,
       U.Name as 用户名,
       U.LoginName as 登录名,
       case when U.Validity = 1 then '正常' else '封禁' end as 状态,
       OM.OrgId as TitleId
from Sys_User U
join Sys_OrgMember PM on PM.UserId = U.ID
join Sys_OrgMerger OM on OM.MergerOrgId = PM.OrgId

GO