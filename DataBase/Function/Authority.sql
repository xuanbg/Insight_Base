USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Authority') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Authority
GO


/*****表值函数：获取操作的鉴权结果*****/

CREATE FUNCTION Authority (
@UserId                UNIQUEIDENTIFIER,    --用户ID
@DeptId                UNIQUEIDENTIFIER,    --部门ID
@ActionId              UNIQUEIDENTIFIER     --操作ID
)

RETURNS TABLE AS

RETURN
select A.ActionId
from Sys_RolePerm_Action A
join Get_PermRole(@UserId, @DeptId) R on R.RoleId = A.RoleId
  and A.ActionId = @ActionId
group by A.ActionId
having min(A.Action) > 0

GO