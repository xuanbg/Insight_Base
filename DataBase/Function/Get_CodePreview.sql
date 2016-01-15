USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_CodePreview') AND OBJECTPROPERTY(id, N'ISSCALARFUNCTION') = 1)
DROP FUNCTION Get_CodePreview
GO


/*****标量值函数：获取编码字符串示例*****/

CREATE FUNCTION Get_CodePreview (
@SchemeId              UNIQUEIDENTIFIER,    --编码方案ID
@DeptId                UNIQUEIDENTIFIER,    --部门ID
@UserId                UNIQUEIDENTIFIER,    --用户ID
@CodeFormat            NVARCHAR(64),
@SerialFormat          NVARCHAR(16)
)

RETURNS NVARCHAR(64) AS
BEGIN

DECLARE @UserCode      NVARCHAR(16),
        @Datetime      DATETIME,
        @Serial        VARCHAR(8),
        @Number        INT,
        @Dig           INT,
        @Count         INT

set @Datetime = getdate()
set @UserCode = ''


/*****日期字段*****/

if (charindex ('yyyy', @CodeFormat) > 0)
  set @CodeFormat = replace(@CodeFormat, 'yyyy', datename(yy, @Datetime))

if (charindex ('yy', @CodeFormat) > 0)
  set @CodeFormat = replace(@CodeFormat, 'yy', right(datename(yy, @Datetime),2))

if (charindex ('mm', @CodeFormat) > 0)
  set @CodeFormat = replace(@CodeFormat, 'mm', right('0' + convert(varchar,datepart(mm, @Datetime)),2))

if (charindex ('dd', @CodeFormat) > 0)
  set @CodeFormat = replace(@CodeFormat, 'dd', right('0' + datename(dd, @Datetime), 2))

/*****自定义字段*****/

if (charindex ('@', @CodeFormat) > 0)
  set @CodeFormat = replace(@CodeFormat, '@', '(X)')

/*****用户/部门/编码机构字段*****/

if (charindex ('$0', @CodeFormat) > 0)
  begin
  select @UserCode = isnull(Code, '') from MasterData where ID = @UserId
  set @CodeFormat = replace(@CodeFormat, '$0', @UserCode)
  end

if (charindex ('$1', @CodeFormat) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = @DeptId
  set @CodeFormat = replace(@CodeFormat, '$1', @UserCode)
  end

if (charindex ('$2', @CodeFormat) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = dbo.Get_SupOrg(@DeptId, 1)
  set @CodeFormat = replace(@CodeFormat, '$2', @UserCode)
  end

if (charindex ('$3', @CodeFormat) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = dbo.Get_SupOrg(@DeptId, 0)
  set @CodeFormat = replace(@CodeFormat, '$3', @UserCode)
  end

/*****计算流水码分组标识*****/

if (charindex ('yyyy', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, 'yyyy', datename(yy, @Datetime))

if (charindex ('yy', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, 'yy', right(datename(yy, @Datetime),2))

if (charindex ('mm', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, 'mm', right('0' + convert(varchar,datepart(mm, @Datetime)),2))

if (charindex ('dd', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, 'dd', right('0' + datename(dd, @Datetime), 2))
  
if (charindex ('$0', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, '$0', @UserCode)

if (charindex ('$1', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, '$1', @UserCode)

if (charindex ('$2', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, '$2', @UserCode)

if (charindex ('$3', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, '$3', @UserCode)

if (charindex ('@', @SerialFormat) > 0)
  set @SerialFormat = replace(@SerialFormat, '@', '(X)')

/*****流水码字段*****/

if (charindex ('#', @CodeFormat) > 0)
  begin
  set @Dig = cast(substring(@CodeFormat, charindex ('#', @CodeFormat) + 1, 1) as int)
  if @Dig > 0
    begin
    select @Number = isnull(max(SerialNumber), -1) + 1
    from SYS_Code_Record
    where SchemeId = @SchemeId
      and RelationChar = @SerialFormat
    if (@Number % power(10, @Dig) = 0)
      set @Number = @Number + 1
    set @Serial = right(replicate('0', @Dig) + cast(@Number as varchar), @Dig)
    end
  else
    select @Serial = min(AllotNumber)
    from SYS_Code_Allot
    where SchemeId = @SchemeId
      and OwnerId = @UserId
      and BusinessId is null

  set @CodeFormat = replace(@CodeFormat, substring(@CodeFormat, charindex ('#', @CodeFormat), 2), @Serial)
  end

RETURN @CodeFormat
END

GO
