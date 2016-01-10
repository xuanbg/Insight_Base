USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_PermRole') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_PermRole
GO


/*****表值函数：获取当前登录用户有效角色*****/

CREATE FUNCTION Get_PermRole(
@UserId                UNIQUEIDENTIFIER,     --当前登录用户ID
@OrgId                 UNIQUEIDENTIFIER      --当前登录部门ID
)

RETURNS TABLE AS

RETURN
with Roles as (
  select R.RoleId                              --获取当前用户作为成员的角色ID
  from Sys_Role_User R
  where R.UserId = @UserId
  union all
  select R.RoleId                              --获取当前用户所在用户组作为成员的角色ID
  from Sys_Role_UserGroup R
  join Sys_UserGroupMember G on G.GroupId = R.GroupId
    and G.UserId = @UserId
  union all
  select R.RoleId                              --获取当前用户的职位作为成员的角色ID
  from Sys_Role_Title R
  join Sys_OrgMember P on P.OrgId = R.OrgId
    and P.UserId = @UserId
  join Sys_Organization O on O.ID = R.OrgId
    and O.ParentId = @OrgId
  union all
  select R.RoleId                              --获取当前用户的职位作为成员的角色ID（职位对应部门被合并）
  from Sys_Role_Title R
  join Sys_OrgMember P on P.OrgId = R.OrgId
    and P.UserId = @UserId
  join Sys_Organization O on O.ID = R.OrgId
  join Sys_OrgMerger OM on OM.MergerOrgId = O.ParentId
    and OM.OrgId = @OrgId)

select distinct RS.RoleId from Roles RS
join Sys_Role R on R.ID = RS.RoleId
  and R.Validity = 1

GO
