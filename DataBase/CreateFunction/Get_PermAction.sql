IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_PermAction') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_PermAction
GO


/*****表值函数：获取当前登录用户有效操作权限*****/

CREATE FUNCTION Get_PermAction(
@ModuleId              UNIQUEIDENTIFIER,     --模块ID
@UserId                UNIQUEIDENTIFIER,    --当前登录用户ID
@OrgId                 UNIQUEIDENTIFIER     --当前登录部门ID
)

RETURNS TABLE AS

RETURN

select A.ActionId
from Sys_RolePerm_Action A
join Get_PermRole(@UserId, @OrgId) R on R.RoleId = A.RoleId
join Sys_ModuleAction M on M.ID = A.ActionId
  and M.ModuleId = @ModuleId
group by A.ActionId
having min(A.Action) > 0

GO
