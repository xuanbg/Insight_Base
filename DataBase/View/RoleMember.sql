USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleMember') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleMember
GO


/*****视图：查询所有角色的成员*****/

CREATE VIEW RoleMember
AS

with List as (
select RoleId,
       1 as [Index],
       cast('10000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       null as ParentId,
       0 as NodeType,
       '用户' as Member
from Sys_Role_User
union
select RoleId,
       2 as [Index],
       cast('20000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       null as ParentId,
       0 as NodeType,
       '用户组' as Member
from Sys_Role_UserGroup
union
select RoleId,
       3 as [Index],
       cast('30000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       null as ParentId,
       0 as NodeType,
       '职位' as Member
from Sys_Role_Title)

select newid() as ID, * from List
union
select RU.ID,
       RU.RoleId,
       U.SN as [Index],
       U.ID as MemberId,
       cast('10000000-0000-0000-0000-000000000000' as uniqueidentifier) as ParentId,
       1 as NodeType,
       U.Name + '(' + U.LoginName + ')' as Member
from Sys_Role_User RU
join Sys_User U on U.ID = RU.UserId
union
select RG.ID,
       RG.RoleId,
       G.SN as [Index],
       G.ID as MemberId,
       cast('20000000-0000-0000-0000-000000000000' as uniqueidentifier) as ParentId,
       2 as NodeType,
       G.Name as Member
from Sys_Role_UserGroup RG
join Sys_UserGroup G on G.ID = RG.GroupId
union
select RP.ID,
       RP.RoleId,
       P.SN as [Index],
	   P.ID as MemberId,
       cast('30000000-0000-0000-0000-000000000000' as uniqueidentifier) as ParentId,
       3 as NodeType,
       case when P.FullName is null then P.Name else P.FullName end as Member
from Sys_Role_Title RP
join Sys_Organization P on P.ID = RP.OrgId

GO