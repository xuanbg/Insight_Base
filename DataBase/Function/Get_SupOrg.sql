USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_SupOrg') AND OBJECTPROPERTY(id, N'ISSCALARFUNCTION') = 1)
DROP FUNCTION Get_SupOrg
GO


/*****标量值函数：获取上级或根机构ID*****/

CREATE FUNCTION Get_SupOrg (
@DeptId                UNIQUEIDENTIFIER,    --部门ID
@Type                  INT                  --机构类型：0、根机构；1、上级机构
)

RETURNS UNIQUEIDENTIFIER AS
BEGIN

DECLARE @NodeType     INT
DECLARE @ParentId     UNIQUEIDENTIFIER
SET @NodeType = 0

while @NodeType != 1
  begin
  select @NodeType = NodeType * @Type, @ParentId = ParentId from Sys_Organization where ID = @DeptId
  if @ParentId is null
    set @NodeType = 1
  if @NodeType != 1
    set @DeptId = @ParentId
  end

RETURN @DeptId
END
GO