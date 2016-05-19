USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleMember') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleMember
GO


/*****视图：查询所有角色的成员*****/

CREATE VIEW RoleMember
AS

select distinct cast('00000000-0000-0000-0000-000000000001' as uniqueidentifier) as ID,
       null as ParentId,
       RoleId,
	   cast('00000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       1 as [Index],
       1 as NodeType,
       '用户' as Name
from SYS_Role_Member
where Type = 1
union
select distinct M.ID,
       cast('00000000-0000-0000-0000-000000000001' as uniqueidentifier) as ParentId,
       M.RoleId,
       U.ID as MemberId,
       U.SN as [Index],
       1 as NodeType,
       U.Name
from SYS_Role_Member M
join Sys_User U on U.ID = M.MemberId
union
select distinct cast('00000000-0000-0000-0000-000000000002' as uniqueidentifier) as ID,
       null as ParentId,
       RoleId,
	   cast('00000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       2 as [Index],
       2 as NodeType,
       '用户组' as Name
from SYS_Role_Member
where Type = 2
union
select distinct M.ID,
       cast('00000000-0000-0000-0000-000000000002' as uniqueidentifier) as ParentId,
       M.RoleId,
       G.ID as MemberId,
       G.SN as [Index],
       2 as NodeType,
       G.Name
from SYS_Role_Member M
join Sys_UserGroup G on G.ID = M.MemberId
union
select distinct cast('00000000-0000-0000-0000-000000000003' as uniqueidentifier) as ID,
       null as ParentId,
       RoleId,
	   cast('00000000-0000-0000-0000-000000000000' as uniqueidentifier) as MemberId,
       3 as [Index],
       3 as NodeType,
       '职位' as Name
from SYS_Role_Member
where Type = 3
union
select M.ID,
       cast('00000000-0000-0000-0000-000000000003' as uniqueidentifier) as ParentId,
       M.RoleId,
       P.ID as MemberId,
       P.SN as [Index],
       3 as NodeType,
       P.FullName as Name
from SYS_Role_Member M
join Sys_Organization P on P.ID = M.MemberId

GO