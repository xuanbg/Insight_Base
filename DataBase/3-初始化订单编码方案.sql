INSERT SYS_Code_Scheme (Name, CodeFormat, SerialFormat, Description, CreatorDeptId, CreatorUserId) 
select '订单编码方案', '@#5yymmdd', '@yymmdd', '订单专用编码方案，不要修改！不要删除！', NULL, N'00000000-0000-0000-0000-000000000000'
