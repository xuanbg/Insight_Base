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
[Version]          VARCHAR(8),                                                                                         --服务版本号
[NameSpace]        VARCHAR(128) NOT NULL,                                                                              --服务命名空间
[Interface]        VARCHAR(64) NOT NULL,                                                                               --接口名称
[Service]          VARCHAR(64) NOT NULL,                                                                               --服务类名称
[ServiceFile]      NVARCHAR(MAX)  NOT NULL,                                                                            --文件路径
)
GO

insert SYS_Interface (Port, Path, Version, NameSpace, Interface, Service, ServiceFile)
select '6200', 'security', 'v1.0', 'Insight.Base.Services', 'ISecurity', 'Security', 'Services\Security.dll' union all
select '6200', 'moduleapi', 'v1.0', 'Insight.Base.Services', 'IModules', 'Modules', 'Services\Modules.dll' union all
select '6200', 'orgapi', 'v1.0', 'Insight.Base.Services', 'IOrganizations', 'Organizations', 'Services\Organizations.dll' union all
select '6200', 'userapi', 'v1.0', 'Insight.Base.Services', 'IUsers', 'Users', 'Services\Users.dll' union all
select '6200', 'groupapi', 'v1.0', 'Insight.Base.Services', 'IUserGroups', 'UserGroups', 'Services\UserGroups.dll' union all
select '6200', 'roleapi', 'v1.0', 'Insight.Base.Services', 'IRoles', 'Roles', 'Services\Roles.dll' union all
select '6200', 'codeapi', 'v1.0', 'Insight.Base.Services', 'ICodes', 'Codes', 'Services\Codes.dll' union all
select '6200', 'logapi', 'v1.0', 'Insight.Base.Services', 'ILogs', 'Logs', 'Services\Logs.dll'
