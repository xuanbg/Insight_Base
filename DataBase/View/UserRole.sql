USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'UserRole') AND OBJECTPROPERTY(id, N'ISVIEW') = 1)
DROP VIEW UserRole
GO


/*****视图：查询所有角色的操作权限*****/

CREATE VIEW UserRole
AS

select newid() as ID, RoleId, MemberId as UserId, null as DiptId
from SYS_Role_Member
where Type = 1
union
select newid() as ID, RoleId, M.UserId, null as DiptId
from SYS_Role_Member R
join SYS_UserGroupMember M on M.GroupId = R.MemberId
where R.Type = 2
union
select newid() as ID, RoleId, M.UserId, O.ParentId as DiptId
from SYS_Role_Member R
join SYS_OrgMember M on M.OrgId = R.MemberId
join SYS_Organization O on O.ID = M.OrgId
where R.Type = 3

GO