USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_PermModule') AND OBJECTPROPERTY(id, N'ISTABLEFUNCTION') = 1)
DROP FUNCTION Get_PermModule
GO


/*****表值函数：获取当前登录用户允许访问模块*****/

CREATE FUNCTION Get_PermModule(
@UserId                UNIQUEIDENTIFIER,    --当前登录用户ID
@OrgId                 UNIQUEIDENTIFIER     --当前登录部门ID
)

RETURNS @PermModule    TABLE(
ModuleId               UNIQUEIDENTIFIER
) AS

BEGIN

declare @ModuleId      uniqueidentifier
declare Module cursor local static read_only forward_only for

select ID from SYS_Module

open Module
fetch next from Module into @ModuleId

while (@@fetch_status=0)
begin
  
  insert @PermModule
  select distinct A.ModuleId
  from SYS_ModuleAction A
  join Get_PermAction(@ModuleId, @UserId, @OrgId) P on P.ActionId = A.ID

  fetch next from Module into @ModuleId

end
close Module
deallocate Module

RETURN
END

GO
