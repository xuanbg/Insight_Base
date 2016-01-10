USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleUser') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleUser
GO


/*****视图：查询所有角色的成员用户*****/

CREATE VIEW RoleUser
AS

with List as (
select RU.RoleId,
       U.ID as UserId,
       U.Name as 用户名,
       U.LoginName as 登录名,
       U.Description as 描述,
       case when U.Validity = 1 then '正常' else '封禁' end as 状态
from Sys_Role_User RU
join Sys_User U on U.ID = RU.UserId
union
select RG.RoleId,
       U.ID as UserId,
       U.Name as 用户名,
       U.LoginName as 登录名,
       U.Description as 描述,
       case when U.Validity = 1 then '正常' else '封禁' end as 状态
from Sys_Role_UserGroup RG
join Sys_UserGroupMember GM on GM.GroupId = RG.GroupId
join Sys_User U on U.ID = GM.UserId
union
select RP.RoleId,
       U.ID as UserId,
       U.Name as 用户名,
       U.LoginName as 登录名,
       U.Description as 描述,
       case when U.Validity = 1 then '正常' else '封禁' end as 状态
from Sys_Role_Title RP
join Sys_OrgMember OM on OM.OrgId = RP.OrgId
join Sys_User U on U.ID = OM.UserId)

select newid() as ID, * from List

GO