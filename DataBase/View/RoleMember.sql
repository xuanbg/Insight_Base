USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleMember') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleMember
GO


/*****视图：查询所有角色的成员*****/

CREATE VIEW RoleMember
AS

select M.ID,
       null as ParentId,
       M.RoleId,
       U.ID as MemberId,
       U.SN as [Index],
       1 as NodeType,
       U.Name
from SYS_Role_Member M
join Sys_User U on U.ID = M.MemberId
union
select M.ID,
       null as ParentId,
       M.RoleId,
       G.ID as MemberId,
       G.SN as [Index],
       2 as NodeType,
       G.Name
from SYS_Role_Member M
join Sys_UserGroup G on G.ID = M.MemberId
union
select M.ID,
       null as ParentId,
       M.RoleId,
       P.ID as MemberId,
       P.SN as [Index],
       3 as NodeType,
       P.FullName as Name
from SYS_Role_Member M
join Sys_Organization P on P.ID = M.MemberId

GO