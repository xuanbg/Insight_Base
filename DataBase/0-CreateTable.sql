USE Insight_Base
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_VerifyImage') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_VerifyImage
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs_Rules') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs_Rules
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Code_Allot') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Code_Allot
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Code_Record') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Code_Record
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Allot_Record') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Allot_Record
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Code_Scheme') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Code_Scheme
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Role_Data') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Role_Data
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Role_Action') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Role_Action
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Role_Member') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Role_Member
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Role') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Role
GO


IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Data') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Data
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_ModuleParam') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_ModuleParam
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_ModuleAction') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_ModuleAction
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Module') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Module
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_ModuleGroup') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_ModuleGroup
GO


IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_OrgMember') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_OrgMember
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Organization') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Organization
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_UserGroupMember') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_UserGroupMember
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_UserGroup') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_UserGroup
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_User') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_User
GO


/*****组织机构、用户和用户组*****/

/*****用户表*****/

CREATE TABLE SYS_User(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_User PRIMARY KEY,                                                                    --此ID与主数据ID相同
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Name]             NVARCHAR(16) NOT NULL,                                                                                                  --用户名
[LoginName]        NVARCHAR(32) NOT NULL,                                                                                                  --登录名
[Password]         VARCHAR(32) DEFAULT 'E10ADC3949BA59ABBE56E057F20F883E' NOT NULL,                                                        --登录密码，保存密码的MD5值，初始密码123456
[PayPassword]      VARCHAR(32),                                                                                                            --支付密码，保存密码的MD5值
[Mobile]           VARCHAR(16),                                                                                                            --手机号
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[Type]             INT DEFAULT 0 NOT NULL,                                                                                                 --用户类型：-1、外部用户；1、内部用户
[BuiltIn]          BIT DEFAULT 0 NOT NULL,                                                                                                 --是否预置：0、自定；1、预置
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[CreatorUserId]    UNIQUEIDENTIFIER DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,                                               --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****用户组表*****/

CREATE TABLE SYS_UserGroup(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_UserGroup PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Name]             NVARCHAR(64) NOT NULL,                                                                                                  --用户组名称
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[BuiltIn]          BIT DEFAULT 0 NOT NULL,                                                                                                 --是否预置：0、自定；1、预置
[Visible]          BIT DEFAULT 1 NOT NULL,                                                                                                 --是否可见：0、不可见；1、可见
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****用户组成员表*****/

CREATE TABLE SYS_UserGroupMember(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_UserGroupMember PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[GroupId]          UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_UserGroup(ID) ON DELETE CASCADE NOT NULL,                                   --用户组ID
[UserId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) ON DELETE CASCADE NOT NULL,                                        --用户ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO


/*****组织机构表*****/

CREATE TABLE SYS_Organization(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Organization PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[ParentId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID),                                                           --父节点ID
[NodeType]         INT NOT NULL,                                                                                                           --节点类型：1、机构；2、部门；3、岗位
[Index]            INT NOT NULL,                                                                                                           --序号
[Code]             VARCHAR(32),                                                                                                            --编码
[Name]             NVARCHAR(32) NOT NULL,                                                                                                  --名称
[Alias]            NVARCHAR(16),                                                                                                           --别名/简称
[FullName]         NVARCHAR(32),                                                                                                           --全称
[PositionId]       UNIQUEIDENTIFIER,                                                                                                       --职能ID，字典
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****职位成员表*****/

CREATE TABLE SYS_OrgMember(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_OrgMember PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[OrgId]            UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID) ON DELETE CASCADE NOT NULL,                                --组织机构（职位）ID
[UserId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) ON DELETE CASCADE NOT NULL,                                        --用户ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO


/*****模块注册数据*****/

/*****模块组表*****/

CREATE TABLE SYS_ModuleGroup(
[ID]               UNIQUEIDENTIFIER PRIMARY KEY NONCLUSTERED,
[SN]               BIGINT CONSTRAINT IX_SYS_ModuleGroup UNIQUE CLUSTERED IDENTITY(1,1),                                                    --自增序列
[Index]            INT,                                                                                                                    --序号
[Name]             NVARCHAR(64) NOT NULL,                                                                                                  --名称
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[IconUrl]          VARCHAR(128),                                                                                                           --图标URL
[Icon]             IMAGE,                                                                                                                  --图标二进制数据
)
GO

/*****模块表*****/

CREATE TABLE SYS_Module(
[ID]               UNIQUEIDENTIFIER PRIMARY KEY NONCLUSTERED,
[SN]               BIGINT CONSTRAINT IX_SYS_Module UNIQUE CLUSTERED IDENTITY(1,1),                                                         --自增序列
[ModuleGroupId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_ModuleGroup(ID) NOT NULL,                                                   --模块组ID
[Type]             INT NOT NULL,                                                                                                           --模块类型：0、系统模块；1、业务模块；2、个人模块
[Index]            INT,                                                                                                                    --序号
[Name]             NVARCHAR(16),                                                                                                           --模块名称
[ProgramName]      VARCHAR(32) NOT NULL,                                                                                                   --程序集名称
[NameSpace]        VARCHAR(128) NOT NULL,                                                                                                  --模块命名空间
[ApplicationName]  NVARCHAR(16),                                                                                                           --应用名称
[Location]         NVARCHAR(MAX),                                                                                                          --文件路径
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[RegisterTime]     DATETIME DEFAULT GETDATE(),                                                                                             --模块注册时间
[Default]          BIT DEFAULT 0 NOT NULL,                                                                                                 --是否默认启动：0、否；1、是
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[IconUrl]          VARCHAR(128),                                                                                                           --图标URL
[Icon]             IMAGE,                                                                                                                  --图标
)
GO

/*****模块功能表*****/

CREATE TABLE SYS_ModuleAction(
[ID]               UNIQUEIDENTIFIER PRIMARY KEY NONCLUSTERED,
[SN]               BIGINT CONSTRAINT IX_SYS_ModuleAction UNIQUE CLUSTERED IDENTITY(1,1),                                                   --自增序列
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --模块注册ID
[Index]            INT,                                                                                                                    --序号
[BeginGroup]       BIT NOT NULL,                                                                                                           --是否开始分组：0、否；1、是
[ShowText]         BIT NOT NULL,                                                                                                           --是否显示文字：0、不显示；1、显示
[Name]             VARCHAR(64) NOT NULL,                                                                                                   --名称
[Alias]            NVARCHAR(16) NOT NULL,                                                                                                  --别名/简称
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[IconUrl]          VARCHAR(128),                                                                                                           --图标URL
[Icon]             IMAGE,                                                                                                                  --图标
)
GO

/*****模块选项配置表*****/

CREATE TABLE SYS_ModuleParam(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_ModuleParam PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --模块注册ID
[ParamId]          UNIQUEIDENTIFIER NOT NULL,                                                                                              --选项ID
[Name]             NVARCHAR(64) NOT NULL,                                                                                                  --选项名称
[Value]            NVARCHAR(MAX),                                                                                                          --选项参数值
[OrgId]            UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID),                                                           --生效机构ID
[UserId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID),                                                                   --生效用户ID
[Description]      NVARCHAR(MAX)                                                                                                           --描述
)
GO

/*****数据配置表*****/

CREATE TABLE SYS_Data(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Data PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Type]             INT NOT NULL,                                                                                                           --类型：0、无归属；1、仅本人；2、仅本部门；3、部门所有；4、机构所有；5、根域所有；
[Alias]            NVARCHAR(16) NOT NULL,                                                                                                  --别名/简称
)
GO



/*****角色权限数据表*****/

/*****角色表*****/

CREATE TABLE SYS_Role(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Role PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Name]             NVARCHAR(64) NOT NULL,                                                                                                  --名称
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[BuiltIn]          BIT DEFAULT 0 NOT NULL,                                                                                                 --是否预置：0、自定；1、预置
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO


/*****角色成员表*****/

CREATE TABLE SYS_Role_Member(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Role_Member PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Type]             INT NOT NULL,                                                                                                           --类型：1、用户；2、用户组；3、岗位；
[RoleId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Role(ID) ON DELETE CASCADE NOT NULL,                                        --角色ID
[MemberId]         UNIQUEIDENTIFIER NOT NULL,                                                                                              --成员ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO



/*****角色操作权限表*****/

CREATE TABLE SYS_Role_Action(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_RolePerm_Action PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[RoleId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Role(ID) ON DELETE CASCADE NOT NULL,                                        --角色ID
[ActionId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_ModuleAction(ID) ON DELETE CASCADE NOT NULL,                                --模块功能注册ID
[Action]           INT DEFAULT 0 NOT NULL,                                                                                                 --操作行为：0、拒绝；1、允许
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****角色数据权限表*****/

CREATE TABLE SYS_Role_Data(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_RolePerm_Data_Rel PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[RoleId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Role(ID) ON DELETE CASCADE NOT NULL,                                        --角色ID
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --业务模块ID
[Mode]             INT DEFAULT 0 NOT NULL,                                                                                                 --数据授权模式：0、相对模式；1、用户模式；2、部门模式；
[ModeId]           UNIQUEIDENTIFIER NOT NULL,                                                                                              --模式ID或部门/用户ID（绝对模式）
[Permission]       INT DEFAULT 0 NOT NULL,                                                                                                 --权限：0、只读；1、读写
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO


/*****编码方案*****/

/*****编码方案表*****/

CREATE TABLE SYS_Code_Scheme(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Code_Scheme PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Name]             NVARCHAR(64) NOT NULL,                                                                                                  --名称
[CodeFormat]       NVARCHAR(64) NOT NULL,                                                                                                  --编码格式
[SerialFormat]     NVARCHAR(16),                                                                                                           --流水码关联字符串格式
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
[CreatorDeptId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID),                                                           --创建部门ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) DEFAULT '00000000-0000-0000-0000-000000000000' NOT NULL,           --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****编码分配记录表*****/

CREATE TABLE SYS_Allot_Record(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Allot_Record PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[SchemeId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Code_Scheme(ID) ON DELETE CASCADE NOT NULL,                                 --编码方案ID
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --模块注册ID
[OwnerId]          UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) NOT NULL,                                                          --用户ID
[StartNumber]      VARCHAR(8),                                                                                                             --编码区段起始值
[EndNumber]        VARCHAR(8),                                                                                                             --编码区段结束值
[CreatorDeptId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID),                                                           --创建部门ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) NOT NULL,                                                          --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)

/*****编码流水记录表*****/

CREATE TABLE SYS_Code_Record(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Code_Record PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[SchemeId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Code_Scheme(ID) ON DELETE CASCADE NOT NULL,                                 --编码方案ID
[RelationChar]     NVARCHAR(16),                                                                                                           --关联字符串
[SerialNumber]     INT NOT NULL,                                                                                                           --流水号
[BusinessId]       UNIQUEIDENTIFIER,                                                                                                       --业务记录ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
CREATE NONCLUSTERED INDEX IX_SYS_Code_Record_Serial ON SYS_Code_Record(SchemeId, RelationChar) INCLUDE (SerialNumber)
CREATE NONCLUSTERED INDEX IX_SYS_Code_Record_BusinessId ON SYS_Code_Record(BusinessId)
GO

/*****编码分配记录表*****/

CREATE TABLE SYS_Code_Allot(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Code_Allot PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[SchemeId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Code_Scheme(ID) ON DELETE CASCADE NOT NULL,                                 --编码方案ID
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --模块注册ID
[OwnerId]          UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) NOT NULL,                                                          --用户ID
[AllotNumber]      VARCHAR(8) NOT NULL,                                                                                                    --分配流水号
[BusinessId]       UNIQUEIDENTIFIER,                                                                                                       --业务记录ID
[UpdateTime]       DATETIME,                                                                                                               --使用时间
[CreatorDeptId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Organization(ID),                                                           --创建部门ID
[CreatorUserId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_User(ID) NOT NULL,                                                          --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
CREATE NONCLUSTERED INDEX IX_SYS_Code_Allot_Serial ON SYS_Code_Allot(SchemeId, ModuleId, OwnerId) INCLUDE (AllotNumber)
CREATE NONCLUSTERED INDEX IX_SYS_Code_Allot_BusinessId ON SYS_Code_Allot(BusinessId)
GO

/*****日志数据表*****/

/*****日志规则表*****/

CREATE TABLE SYS_Logs_Rules(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs_Rules PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Code]             VARCHAR(6) NOT NULL,                                                                                                    --操作代码
[ToDataBase]       BIT DEFAULT 0 NOT NULL,                                                                                                 --是否写到数据库：0、否；1、是
[Level]            INT DEFAULT 0 NOT NULL,                                                                                                 --日志等级：0、Emergency；1、Alert；2、Critical；3、Error；4、Warning；5、Notice；6、Informational；7、Debug
[Source]           NVARCHAR(16),                                                                                                           --事件来源
[Action]           NVARCHAR(16),                                                                                                           --操作名称
[Message]          NVARCHAR(128),                                                                                                          --日志默认内容
[CreatorUserId]    UNIQUEIDENTIFIER NOT NULL,                                                                                              --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****日志表*****/

CREATE TABLE SYS_Logs(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Level]            INT NOT NULL,                                                                                                           --日志等级：0、Emergency；1、Alert；2、Critical；3、Error；4、Warning；5、Notice；6、Informational；7、Debug
[Code]             VARCHAR(6),                                                                                                             --操作代码
[Source]           NVARCHAR(16) NOT NULL,                                                                                                  --事件来源
[Action]           NVARCHAR(16) NOT NULL,                                                                                                  --操作名称
[Message]          NVARCHAR(MAX),                                                                                                          --日志内容
[Key]              VARCHAR(64),                                                                                                            --操作名称
[SourceUserId]     UNIQUEIDENTIFIER,                                                                                                       --来源用户ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****验证图形表*****/

CREATE TABLE SYS_VerifyImage(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_VerifyImage PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Type]             INT NOT NULL,                                                                                                           --图形类型：0、遮罩图；1、背景图；
[Name]             NVARCHAR(16) NOT NULL,                                                                                                  --图形名称
[Path]             VARCHAR(256) NOT NULL,                                                                                                  --图片本地路径
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****初始化用户：系统管理员，密码：admin*****/

insert SYS_User (ID, Name, LoginName, Password, Type, BuiltIn)
select '00000000-0000-0000-0000-000000000000', '系统管理员', 'Admin', '908720E5EED8090CF950329A1B35182B', 1, 1
GO

/*****初始化用户组：所有用户和系统管理员组*****/

insert SYS_UserGroup (Name, Description, BuiltIn, Visible)
select 'AllUsers', '所有用户', 1, 0 union all
select 'Administers', '系统管理员组', 1, 1
GO

/*****初始化组成员：系统管理员*****/

insert SYS_UserGroupMember (GroupId, UserId)
select ID, '00000000-0000-0000-0000-000000000000' from SYS_UserGroup
GO


/*****初始化数据权限定义*****/
INSERT SYS_Data (Type, Alias)
select 0, '无归属' union all
select 1, '本人' union all
select 2, '本部门' union all
select 3, '部门所有' union all
select 4, '机构所有' union all
select 5, '根域所有'
GO

/*****初始化日志规则*****/
INSERT SYS_Logs_Rules (Code, Level, Source, Action, Message, CreatorUserId)
select '200101', 2, '系统平台', 'SqlQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200102', 2, '系统平台', 'SqlNonQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200103', 2, '系统平台', 'SqlScalar', null, '00000000-0000-0000-0000-000000000000' union all
select '200104', 2, '系统平台', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all
select '200105', 2, '系统平台', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all

select '300601', 3, '日志服务', '新增规则', '插入数据失败', '00000000-0000-0000-0000-000000000000' union all
select '300602', 3, '日志服务', '删除规则', '删除数据失败', '00000000-0000-0000-0000-000000000000' union all
select '300603', 3, '日志服务', '编辑规则', '更新数据失败', '00000000-0000-0000-0000-000000000000' union all

select '500101', 5, '系统平台', '接口验证', null, '00000000-0000-0000-0000-000000000000' union all

select '600601', 6, '日志服务', '新增规则', null, '00000000-0000-0000-0000-000000000000' union all
select '600602', 6, '日志服务', '删除规则', null, '00000000-0000-0000-0000-000000000000' union all
select '600603', 6, '日志服务', '编辑规则', null, '00000000-0000-0000-0000-000000000000'
GO

/*****触发器：自动添加用户组【所有用户】成员*****/

CREATE TRIGGER SYS_User_Insert ON SYS_User AFTER INSERT AS

BEGIN
SET NOCOUNT ON

DECLARE @GroupId UNIQUEIDENTIFIER
select @GroupId = ID from SYS_UserGroup where Name = 'AllUsers'

INSERT SYS_UserGroupMember (GroupId, UserId, CreatorUserId)
select @GroupId, TI.ID, TI.CreatorUserId 
from Inserted TI 
where TI.Type > 0

END
GO

/*****触发器：自动删除角色成员用户*****/

CREATE TRIGGER SYS_User_Delete ON SYS_User AFTER DELETE AS

BEGIN
SET NOCOUNT ON

DELETE M
from SYS_Role_Member M
join deleted D on D.ID = M.MemberId
where M.Type = 1

END
GO

/*****触发器：自动删除角色成员用户组*****/

CREATE TRIGGER SYS_UserGroup_Delete ON SYS_UserGroup AFTER DELETE AS

BEGIN
SET NOCOUNT ON

DELETE M
from SYS_Role_Member M
join deleted D on D.ID = M.MemberId
where M.Type = 2

END
GO

/*****触发器：自动删除角色成员职位*****/

CREATE TRIGGER SYS_Organization_Delete ON SYS_Organization AFTER DELETE AS

BEGIN
SET NOCOUNT ON

DELETE M
from SYS_Role_Member M
join deleted D on D.ID = M.MemberId
and D.NodeType = 3
where M.Type = 3

END
GO
