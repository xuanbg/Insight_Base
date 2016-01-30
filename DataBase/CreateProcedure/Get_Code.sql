USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'GetCode') AND OBJECTPROPERTY(id, N'ISPROCEDURE') = 1)
DROP PROCEDURE GetCode
GO


/*****标量值函数：获取编码字符串*****/

CREATE PROCEDURE GetCode (
@SchemeId              UNIQUEIDENTIFIER,           --编码方案ID
@DeptId                UNIQUEIDENTIFIER,           --部门ID
@UserId                UNIQUEIDENTIFIER,           --用户ID
@BusinessId            UNIQUEIDENTIFIER,           --业务记录ID
@ModuleId              UNIQUEIDENTIFIER,           --业务模块ID
@Char                  NVARCHAR(8)                 --自定义字符串
)
AS
BEGIN

SET NOCOUNT ON

DECLARE @SerialFormat  NVARCHAR(16),
        @UserCode      NVARCHAR(16),
        @Datetime      DATETIME,
        @Serial        VARCHAR(8),
        @Number        INT,
        @Dig           INT,
        @Count         INT,
		@Code          VARCHAR(64)

select @Code = CodeFormat, @SerialFormat = isnull(SerialFormat, '') from SYS_Code_Scheme where ID = @SchemeId
set @Datetime = getdate()
set @UserCode = ''

/*****格式化日期字段*****/

if (charindex ('yyyy', @Code) > 0)
  set @Code = replace(@Code, 'yyyy', datename(yy, @Datetime))

if (charindex ('yy', @Code) > 0)
  set @Code = replace(@Code, 'yy', right(datename(yy, @Datetime),2))

if (charindex ('mm', @Code) > 0)
  set @Code = replace(@Code, 'mm', right('0' + convert(varchar,datepart(mm, @Datetime)),2))

if (charindex ('dd', @Code) > 0)
  set @Code = replace(@Code, 'dd', right('0' + datename(dd, @Datetime), 2))
  
/*****格式化自定义字段*****/

if (charindex ('@', @Code) > 0)
  set @Code = replace(@Code, '@', isnull(@Char, ''))

/*****格式化部门/编码机构字段*****/

if (charindex ('$1', @Code) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = @DeptId
  set @Code = replace(@Code, '$1', @UserCode)
  end

if (charindex ('$2', @Code) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = dbo.Get_SupOrg(@DeptId, 1)
  set @Code = replace(@Code, '$2', @UserCode)
  end

if (charindex ('$3', @Code) > 0)
  begin
  select @UserCode = isnull(Code, '') from SYS_Organization where ID = dbo.Get_SupOrg(@DeptId, 0)
  set @Code = replace(@Code, '$3', @UserCode)
  end

/*****计算流水码重置规则*****/

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
  set @SerialFormat = replace(@SerialFormat, '@', isnull(@Char, ''))

/*****格式化流水码字段*****/

if (charindex ('#', @Code) > 0)
  begin
  set @Dig = cast(substring(@Code, charindex ('#', @Code) + 1, 1) as int)
  if (@Dig > 0)
    begin
    select @Count = count(1) from SYS_Code_Record where BusinessId = @BusinessId
    if (@Count > 0)
      select @Serial = right(replicate('0', @Dig) + cast(SerialNumber as varchar), @Dig)
      from SYS_Code_Record
      where BusinessId = @BusinessId
    else
      begin
      select @Number = isnull(max(SerialNumber), -1) + 1
      from SYS_Code_Record
      where SchemeId = @SchemeId
        and RelationChar = @SerialFormat
      if (@Number % power(10, @Dig) = 0)
        set @Number = @Number + 1
      set @Serial = right(replicate('0', @Dig) + cast(@Number as varchar), @Dig)
      end
    end
  else
    begin
    select @Count = count(1) from SYS_Code_Allot where BusinessId = @BusinessId
    if (@Count > 0)
      select @Serial = AllotNumber
      from SYS_Code_Allot
      where BusinessId = @BusinessId
    else
      select @Serial = min(AllotNumber)
      from SYS_Code_Allot
      where SchemeId = @SchemeId
        and ModuleId = @ModuleId
        and OwnerId = @UserId
        and BusinessId is null
    end
  set @Code = replace(@Code, substring(@Code, charindex ('#', @Code), 2), @Serial)
  end


/*****使用业务记录ID注册流水码*****/

if (@Count = 0)
  begin
  if (@Dig > 0)
    insert SYS_Code_Record(SchemeId, RelationChar, SerialNumber, BusinessId)
    select @SchemeId, @SerialFormat, @Number, @BusinessId
  else
    update SYS_Code_Allot set BusinessId = @BusinessId, UpdateTime = getdate()
    where SchemeId = @SchemeId
      and OwnerId = @UserCode
      and AllotNumber = @Serial
  end

select @Code

END

GO
