USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'RoleMemberUser') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW RoleMemberUser
GO


/*****视图：查询所有角色的成员用户*****/

CREATE VIEW RoleMemberUser
AS

select U.ID,
       M.RoleId,
       U.Name,
       U.LoginName,
       U.Description,
       U.Validity
from SYS_Role_Member M
join Sys_User U on U.ID = M.MemberId
where M.Type = 1
union
select U.ID,
       M.RoleId,
       U.Name,
       U.LoginName,
       U.Description,
       U.Validity
from SYS_Role_Member M
join Sys_UserGroupMember G on G.GroupId = M.MemberId
join Sys_User U on U.ID = G.UserId
where M.Type = 2
union
select U.ID,
       M.RoleId,
       U.Name,
       U.LoginName,
       U.Description,
       U.Validity
from SYS_Role_Member M
join Sys_OrgMember O on O.OrgId = M.MemberId
join Sys_User U on U.ID = O.UserId
where M.Type = 3

GO