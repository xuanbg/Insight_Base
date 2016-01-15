USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_FormatDate') AND OBJECTPROPERTY(id, N'ISSCALARFUNCTION') = 1)
DROP FUNCTION Get_FormatDate
GO


/*****标量值函数：获取任意格式的当前时间*****/

CREATE FUNCTION Get_FormatDate (
@Code                 NVARCHAR(16)        --编码字段
)

RETURNS NVARCHAR(16) AS
BEGIN

DECLARE @Datetime DATETIME
set @Datetime = getdate()

if (charindex ('yyyy', @Code) > 0)
  set @Code = replace(@Code, 'yyyy', datename(yy, @Datetime))

if (charindex ('yy', @Code) > 0)
  set @Code = replace(@Code, 'yy', right(datename(yy, @Datetime),2))

if (charindex ('mm', @Code) > 0)
  set @Code = replace(@Code, 'mm', right('0' + convert(varchar,datepart(mm, @Datetime)),2))

if (charindex ('dd', @Code) > 0)
  set @Code = replace(@Code, 'dd', right('0' + datename(dd, @Datetime), 2))

if (charindex ('month', @Code) > 0)
  set @Code = replace(@Code, 'month', datename(mm, @Datetime))

if (charindex ('mon', @Code collate sql_latin1_general_cp1_cs_as) > 0)
  set @Code = replace(@Code, 'mon', left(upper(datename(mm, @Datetime)),3))

if (charindex ('mon', @Code) > 0)
  set @Code = replace(@Code, 'mon', left(datename(mm, @Datetime),3))

if (charindex ('m', @Code) > 0)
  set @Code = replace(@Code, 'm', convert(varchar, datepart(mm, @Datetime)))

if (charindex ('d', @Code) > 0)
  set @Code = replace(@Code, 'd', datename(dd, @Datetime))

RETURN @Code

END

GO