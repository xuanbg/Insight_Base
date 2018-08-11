IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'Get_CnAmount') AND OBJECTPROPERTY(id, N'ISSCALARFUNCTION') = 1)
DROP FUNCTION Get_CnAmount
GO


/*****标量值函数：金额阿拉伯数字转换为中文*****/

CREATE FUNCTION Get_CnAmount(
@Amount                DECIMAL(18, 2), --金额
@Type                  INT             --0到元补整；1到角补整
)

RETURNS NVARCHAR(36) AS
BEGIN

DECLARE @Digital       NVARCHAR(10)
DECLARE @Position      NVARCHAR(18)
DECLARE @Inputstr      VARCHAR(18)
DECLARE @L             INT             --金额乘以100后的字符串长度
DECLARE @I             INT             --循环变量
DECLARE @Zero          INT             --连续零计数器
DECLARE @Posvalue      INT             --从金额中取出一位的值
DECLARE @Chdig         NVARCHAR(1)     --数字大写
DECLARE @Chpos         NVARCHAR(1)     --数字位大写
DECLARE @CnAmount      NVARCHAR(36)

SET     @Digital = '零壹贰叁肆伍陆柒捌玖'
SET     @Position = '万仟佰拾亿仟佰拾万仟佰拾元角分'
SET     @CnAmount = ''

if abs(@Amount) < 1
  set @Inputstr = cast((abs(@Amount) * 100) as int)
else
  set @Inputstr = replace(abs(@Amount), '.', '')
set @L = len(@Inputstr)
set @Position = right(@Position, @L)
set @I = 1
while @I <= @L
  begin
  set @Posvalue = cast(substring(@Inputstr, @I, 1) as int) --取第 i 位数字
  set @Chdig = substring(@Digital, @Posvalue + 1, 1)       --取第 i 位数字的大写数字：零壹贰叁肆伍陆柒捌玖
  set @Chpos = substring(@Position, @I, 1)                 --取第 i 位数字的大写位名：万仟佰拾亿仟佰拾万仟佰拾元角分
  if @Posvalue > 0
    begin
    if (@Zero > 0) and ((@L - @I + 3) % 4 != 0)            --补回除亿、万、元位外的零
      set @CnAmount = @CnAmount + '零'
    set @Zero = 0
    end
  if @Posvalue = 0
    begin
    set @Chdig = ''
    if ((@L - @I + 2) % 4 != 0)                            --去零对应位大写，保留亿、万、元位
      set @Chpos = ''
    if (@Zero > 2) and (@I = @L - 6)                       --亿万相连时只写亿
      set @Chpos = ''
    if (@Zero + @Type > 0) and (@I = @L)                   --补整（第二参数输入1时到角补整）
      set @Chpos = '整'
    set @Zero = @Zero + 1
    end
  set @CnAmount = @CnAmount + @Chdig + @Chpos
  set @I = @I + 1
  end
if @Amount < 0
  set @CnAmount = '(负)' + @CnAmount
if @Amount = 0
  set @CnAmount = '零元整'

RETURN @CnAmount
END

GO