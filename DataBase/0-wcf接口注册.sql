IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Interface') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Interface
GO
/*****模块表*****/

CREATE TABLE SYS_Interface(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Interface PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                               --自增序列
[Binding]          VARCHAR(32) NOT NULL,                                                                               --绑定类型
[Port]             VARCHAR(32) NOT NULL,                                                                               --服务端口号
[Name]             VARCHAR(32) NOT NULL,                                                                               --模块名称
[Class]            VARCHAR(64) NOT NULL,                                                                               --实现类命名空间
[Interface]        VARCHAR(64) NOT NULL,                                                                               --接口类命名空间
[Location]         NVARCHAR(MAX),                                                                                      --文件相对路径
)
GO

insert SYS_Interface (Binding, Port, Name, Class, Interface, Location)
select 'HTTP', '80', 'Interface', 'Insight.WS.Service.Interface', 'Insight.WS.Service.IInterface', 'Services'
