IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_PermData') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_PermData
GO


/*****表值函数：获取当前登录用户数据访问范围和权限*****/

CREATE FUNCTION Get_PermData(
@ModuleId              UNIQUEIDENTIFIER,     --模块ID
@UserId                UNIQUEIDENTIFIER,     --当前登录用户ID
@OrgId                 UNIQUEIDENTIFIER      --当前登录部门ID
)

RETURNS @PermScope     TABLE(
OrgId                  UNIQUEIDENTIFIER,
UserId                 UNIQUEIDENTIFIER,
Permission             INT
) AS

BEGIN

DECLARE @Mode          INT,
        @Permission    INT,
        @NodeId        UNIQUEIDENTIFIER

DECLARE @Table         TABLE(
        OrgId          UNIQUEIDENTIFIER,
        UserId         UNIQUEIDENTIFIER,
        Permission     INT
)

--自定义数据权限
insert into @Table

select A.OrgId, A.UserId, max(A.Permission)
from SYS_RolePerm_DataAbs A
join Get_PermRole(@UserId, @OrgId) R on R.RoleId = A.RoleId
where A.ModuleId = @ModuleId
group by A.OrgId, A.UserId

union all
select M.MergerOrgId, null, max(A.Permission)
from SYS_RolePerm_DataAbs A
join Sys_OrgMerger M on M.OrgId = A.OrgId
join Get_PermRole(@UserId, @OrgId) R on R.RoleId = A.RoleId
where A.ModuleId = @ModuleId
group by M.MergerOrgId

begin

DECLARE import CURSOR LOCAL STATIC READ_ONLY FORWARD_ONLY FOR

select D.Mode, D.Permission
from SYS_RolePerm_Data D
join Get_PermRole(@UserId, @OrgId) R on R.RoleId = D.RoleId
where D.ModuleId = @ModuleId

OPEN import
FETCH NEXT FROM import INTO @Mode, @Permission

WHILE (@@FETCH_STATUS=0)
BEGIN


if @Mode = 0                          --授权访问范围为无归属时，返回null和权限代码
  insert into @Table
  select '00000000-0000-0000-0000-000000000000', null, @Permission

if @Mode = 1                           --授权访问范围为仅本人时，返回null和权限代码
  insert into @Table
  select null, @UserId, @Permission

else if @Mode = 2                      --授权访问范围为仅本部门时，返回本部门（当前登录部门）ID、合并到该部门的部门ID和权限代码
  insert into @Table
  select @OrgId, null, @Permission
  union
  select MergerOrgId, null, @Permission
  from Sys_OrgMerger
  where OrgId = @OrgId

else if @Mode between 3 and 5
  begin
  if @Mode = 3
    select @NodeId = @OrgId
  if @Mode = 4
    select @NodeId = dbo.Get_SupOrg(@OrgId, 1)    --获取上级机构ID
  if @Mode = 5
    select @NodeId = dbo.Get_SupOrg(@OrgId, 0)    --获取根机构ID

  insert into @Table                   --授权访问范围为本部门及下属、本机构及下属、全部时，返回本部门（当前登录部门）和相应机构/部门ID、合并到上述部门的部门ID和权限代码
  select ID, null, @Permission from Get_SubOrg(@NodeId)
  end

FETCH NEXT FROM import INTO @Mode, @Permission
END

CLOSE import
DEALLOCATE import

end

insert @PermScope
select OrgId, UserId, max(Permission)
from @Table
where Permission >= 0
group by OrgId, UserId

RETURN
END
GO