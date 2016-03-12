use Insight_Base
go

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Interface') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Interface
GO
/*****模块表*****/

CREATE TABLE SYS_Interface(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Interface PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                               --自增序列
[Port]             VARCHAR(8),                                                                                         --服务端口号
[Path]             VARCHAR(16),                                                                                        --服务路径
[NameSpace]        VARCHAR(128) NOT NULL,                                                                              --服务命名空间
[Interface]        VARCHAR(64) NOT NULL,                                                                               --接口名称
[Service]          VARCHAR(64) NOT NULL,                                                                               --服务类名称
[ServiceFile]      NVARCHAR(MAX)  NOT NULL,                                                                            --文件路径
)
GO

insert SYS_Interface (Port, Path, NameSpace, Interface, Service, ServiceFile)
select NULL, NULL, 'Insight.WS.Base', 'IVerify', 'Verify', 'Services\Verify.dll' union all
select NULL, 'modules', 'Insight.WS.Base', 'IModule', 'Modules', 'Services\Modules.dll' union all
select NULL, 'orgs', 'Insight.WS.Base', 'IOrganizations', 'Organizations', 'Services\Organizations.dll' union all
select NULL, 'users', 'Insight.WS.Base', 'IUsers', 'Users', 'Services\Users.dll' union all
select NULL, 'roles', 'Insight.WS.Base', 'IRoles', 'Roles', 'Services\Roles.dll' union all
select NULL, 'codes', 'Insight.WS.Base', 'ICodes', 'Codes', 'Services\Codes.dll'
