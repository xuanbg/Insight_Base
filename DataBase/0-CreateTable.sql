USE Insight_Base
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
[OpenId]           VARCHAR(32),                                                                                                            --OpenId
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
[Icon]             IMAGE,                                                                                                                  --图标
)
GO

/*****模块表*****/

CREATE TABLE SYS_Module(
[ID]               UNIQUEIDENTIFIER PRIMARY KEY NONCLUSTERED,
[SN]               BIGINT CONSTRAINT IX_SYS_Module UNIQUE CLUSTERED IDENTITY(1,1),                                                         --自增序列
[ModuleGroupId]    UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_ModuleGroup(ID),                                                            --模块组ID
[Type]             INT NOT NULL,                                                                                                           --模块类型：0、系统模块；1、业务模块；2、个人模块
[Index]            INT,                                                                                                                    --序号
[Name]             NVARCHAR(16),                                                                                                           --模块名称
[ProgramName]      VARCHAR(32) NOT NULL,                                                                                                   --程序集名称
[MainFrom]         VARCHAR(128) NOT NULL,                                                                                                  --主窗体名称
[ApplicationName]  NVARCHAR(16),                                                                                                           --应用名称
[Location]         NVARCHAR(MAX),                                                                                                          --文件路径
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[RegisterTime]     DATETIME DEFAULT GETDATE(),                                                                                             --模块注册时间
[Default]          BIT DEFAULT 0 NOT NULL,                                                                                                 --是否默认启动：0、否；1、是
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
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
[EntryId]          UNIQUEIDENTIFIER,                                                                                                       --入口功能ID
[Description]      NVARCHAR(MAX),                                                                                                          --描述
[Validity]         BIT DEFAULT 1 NOT NULL,                                                                                                 --是否有效：0、无效；1、有效
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
[Mode]             INT DEFAULT 0 NOT NULL,                                                                                                 --数据授权模式：0、相对模式；1、用户模式；2、部门模式；
[RoleId]           UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Role(ID) ON DELETE CASCADE NOT NULL,                                        --角色ID
[ModuleId]         UNIQUEIDENTIFIER FOREIGN KEY REFERENCES SYS_Module(ID) ON DELETE CASCADE NOT NULL,                                      --业务模块ID
[ModeId]           UNIQUEIDENTIFIER NOT NULL,                                                                                              --模式ID
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


/*****初始化用户：系统管理员，密码：admin*****/

insert SYS_User (ID, Name, LoginName, Type, BuiltIn)
select '00000000-0000-0000-0000-000000000000', '系统管理员', 'Admin', 1, 1
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


/*****初始化模块数据：模块组*****/
INSERT SYS_ModuleGroup
select N'0876516C-C71D-4541-9887-9845EED80EC2', 1, N'个人中心', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000001974455874536F6674776172650041646F626520496D616765526561647971C9653C000005164944415478DABC57696C5455143E6FF6AD7BE7B5B55496BAB0586AAB01D3085221DAD2824692D21419D9D49000B6B616220A8A4E91A5822289B1A9517FB4B5920046A00CA555208D21412D2D6DD96C854E99E92C9D193AF3663FDE3B3C09514A643AC34DBEDCFB5EEEB9DFD9EE39B900911DE2E06729163A8FF1FD9F2188B0024A06838967DF4E984BCFA633039848FF474B01A1670FBBCBF871F24CB266A8A52E96BD923333B181AC637266C435B8E212AEF01E608C5AF54CBA9FCA454A0191AED773984D90763A3F7FA88E9278DD7E4E34630A6BDF16775C34299DF5BA7D1CFDEFDC9352C7C60A3B75BDDEC354EE9F0398F1C69C20F9DAF6D4BD1985B925D0FB277276F7B0FCD9AC14B8400C4F6781EBBC6A948B1916544AE6FA556BF3C3EF0F97131933812F120A50F918828978E0894E78722A035672361300F078005C647690199150DA91593B984DF6FE45709300C7A300CD6E034D385F1ADBE7F50438C6C5A52B7227B1C070B0A2BE80045700DF147E07C091703B03E0B28C0E133DF41204B958C64C256BABE02D636AB8392026C24FEF3FC3693C0EB750392D2347316746885CF3D53CD06EAB848A752B60654B19F1840DC0CF8142C2B04A06723C4114EE3FEDD250F97B5DCFFF137B7ABD3209661BB6287FC71F26E0ABAF57E390C9813E3F6267770FB6759CC315155AC47DF1887BD568D8CEFE41F7F372545E2C0A53019A40B6F31B13B21E7F4CD128999298A6692A829D35EF4252BC122E5EEA03A3C904DEA01818120AE0482EB80052FC4CB6EF13F58F7D8EE0D2AC1A4B3F3923187E0E7CA11E6204BE648B5C7EEE9D73AB9FAAD9B6099262E470E9F225D01B0CE00D08E1E0D136A84FAB01F0CAC129129B3867D02C0394A8584926066FE58028EC1C58679A45AF60F6ECDCB39B373D03A9094AE8B948C98DE021E48742E45A920372721310944990ACFAC85A4264685DF0520F86CE19470846F3E6CE3F5BBAA41866E5664163533398CC56F0054570E808219F482C77496154243143BC1861C0CDE8B726BD49E4A8EB070946E839C2702BE073F9CF3B162F5A04CB97954263E3F7D0DE7E0AEC4E0E7EEBB90A75E95A187429BA63BD02D6EE876BAAADD6D74AE7AB6227278B96BC385D7AA0FE578E966737CD81703C20C9CFCFF714172D8265654BA1A1A109BABA2E8042A902DDC95F604BACB6DB15509832DEB35571329155164409911998B6C3BA597BD2F5CA236AA1E6CE521C0E39EEDABD07F5434364FE14976B56E2EA356B31FF8585B4B215E19789B8E325D54AB29EBEF365D52ADC9742FFB304B10469046A026938CD2864F9C2A2C550C65B7EFE7C1748A53218BC3E00EDBAA3B405EBFD3EE1C8C6C3A3A7E8BAFAD0E8297F00EC7CD9BDC9F7000781FF7E9BD198E4FAC17E3876AC652E9F5CA4F0838AC0CE67793C411C9F70B6B19A49A4C887794BC5FC2DF1F1EB3BBFEF5B81FB21F7869354827B93CF8B2AF9BD1490E4E5E5790A0A8BA34A3E56082415E5E59E92D232484D65A1B9F940E89ECB649127BFABE51BD6AF47CB880DE9387EA2154B4ACA70D5EA37B0B0A08026D81C8209741F446184C8CDD611F491867EA1B70F7F3E7D06EBBEFE16172C58103572D16DF20D1B3C5B3EF810626362E0E2E5CB7083B4542411EABFD207ADADADD1757B5555255A6C76F40782D8DDD38BBAB6766C39D1865595955177BBE8D6AB85018BD90206C30D62B911820184132D47A0B6B636AA9603FF42917774741C140B056B26673E0A3EBF1F74C77E7A20E4B743C0BB784E7575355244DBED77AB0312BE65A6F3DFFA0761F9BF0B91E48E57ACF34190D3F1B7000300E5237BBC9BE87FA80000000049454E44AE426082 union all
select N'D8559A8F-9E56-4B6A-BFE6-536633257A4E', 2, N'资金结算', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000001974455874536F6674776172650041646F626520496D616765526561647971C9653C00000A704944415478DA9457095454D719FEDF9BE5CDB0CC0CCBC0C888605C0648008D1244036E18110C293469624F63EC62A249D5F4344DD21A6371495A6B8C89893D4DAB31C626ED89D160C80001C505110541361504061C189C056601667FEFF5DEE71B33A5C493DEC3CFCCDCFBEEFDBFFB2FDFFF3F02BE671CD99AA988574A5E1390ECB324CBAA5960EFAD11040934437CE9F4B2EF16BC7ABE0E4DB1415BC5B9BB2A3D2C436B4FBF99FF04FAED87FB0C629239B2626FCE5A7998E463D5CCF910A7C90471F4D4BB2BECDD1D8C63186CA65EE8AA2F05AB75ECE9FC572F1C472B0C56BEB8A4DCB376651A1CFBB60D6ADECC93A1B9D1FB01104E54FECD9EEC75312AD5A1B4DCE7402026019C3A80C15AA498E441B0408AA32152150F8AA83870396E3E89664BF1CAE2920A4FC1A349201109C0E3A321E78FE50E7E930B58A6DC71BBFD17D73E7EDDFEBD16F8DD9AE498E2C509C68CC21740E0EF05B037A37B1170BDC30266B30B84E860658C1454AA30E8ED1E813EBDFD7CD1B6CBEBD1D63BD92555F6A7962743599B0936672780951D45AE028811C981616818B238A0A1CB08AD3AE39EDA5D3FFA03DA43639D8220FD82DF3EAD797156EAC32B22233D48791B022E80AB4D6628BB6038B076F7E56D87BFD11D65FD70534843AC6E70ECEC9A1D57DE40FB467276D5589ECD4B87AAAE61100B48681C1805A3C3073A9317BACD1EB86172819892C0E2E45808A7848BDC894B9DFADA2F2E61D3600B1015FB966908609F150BD817B3F28B1594BB11394308ED2D26B8DA6E39B5EEAD7A4E11EFE750246148DC48CC7C904D5BB6A3A2559D340B189AE1DC04247734FACA723F69868198701114CC9441E9A54E387DFC48ECAD8ABF9BF053D4C5832BDC34EDE7A22CBB701580E3063A80846B570C6077B8D059040E7DF447002910A02512B25EA88CE0030C5B3106C9F4DC9DE5E713D31EE414F6B5B4A13B88800A91702042E40A90C8653047150A3EEB3094D5B51FA97DFBA9F5380843140A31A4AC5A07E04320AC4DDC0104CBC09C829F22202C1F29E89F5008178F1D82961EFBEBBC0F0362C24F546F5B958341A464CEE780761CDFF3BC482A1386AAA64F9B92B6744B98334A6A944F87C428050825A16BD0964D1880C8E6F0424BE927200D11C1EC64E5BDD8BC5A7A14053F0B770D40804028E4D636ECBD726A427E7B834110BB2BCF7B4647EA0CCDD59823C6B0DBAE9F78A762E9F6B2F2F17195D41F198DCFA4D0BC844B43858C82945C647A3FF2DF48131F1A2CCCCB2F4416E0F120B3DBFAF570BDE12ADE8253C9332185EF81A8DABA3283924563808348C6F12531104BE7953F8D4C55978C4F8D436EE5D29AE4008C8DFBA0A34A0B7E04203D438D74339CCFBBEBABC189D6906E8EFD6C3637E8865CC77865CC24BC120031EA7158306D39794BF990587C6E7FA7C96885EAF273F84E63011E8858939BB0ECD13465B1265E56909919270F9353DC69ED6DA3D0DAA13FFFB7AF7B0E291594441126260F95F59C454B03BC69EF4B722FBD5F78920162357755963EF5E196B25FA3AF53F82CB222D1610B383FAFEEAF43D2F3C9D62C29B246519842C2596146521CD8476C396B9627546E7CA7E133FE86E327DFCA79481622CC110848F1924DD56F074865C290FA5958BDE7A577B91FAF7EF832F227FC0A4937CFC0DC59046F059C4AF22D4F25ADFAD9CAC44FE72F50F3B352703969D0DD1A02F39DBBCC16269372C12A8F0805BDCE0CC72A74533E38D17527F8E61BF63FF12F3125FEB1521E0B2969326EF27AAB03C69C76B08FDB5DC879FB9D36CF3B474B2A8783A918074ADCC9DDD94713D4E139A9735520C4B58040D848011713FF45E088687ADB8DD0DF3B7266D9CBA7F3783F8B5E78AFF0F6C2F485AA85A9D9201587C015FD09EEF1CC8427915E06ECA35638DF7C163A7A3BE08397BEA482A918070DFBEF33B7EB92E2C349D243CFC70118162244407009FC2EED6DC81ADD1D26181A7258771C695FD77767DC820F58BFAFF044F6C3D9F3B25217801E51F9B9B60A68BEA6874E64A0DAD61AD09B7A20228282050F2D820B4D17A1A1FCC6BBC2090070CA0CFCE640D37BE8F3F3BFBE92B171E66D472E2522A3F0222E4634AA721E3F3352DB663E5C72B8ED381F9038D2A5A4907C3C5D930A2D866A6430163A6F0E414FF3D0FE8AC3F55AAC20776D466E4FFFC096C4844B9480E4D25034B11CE3836C3CCFDB36EE6DD8853E0F60BAC6467F7CA13AEEEBBA41ACD0C5D78661240EDEFCF27049380C3B7B119332A8E844813232063AC6FBAEA1B51B386DAB8F36E891943DF37AEE66AFDBDF83CD49DC278D04BC62091FB5014A62F80876F36414E0839817DF2F3616E4CD03B7C78DB84300BE513954D7D5D87D2E7AD3C75BBFF99C8F33391219BFD71C00205CBE535B4A10827C9AA6CB6AB6E7174DD24A0532869DD0820586F2E7BB577F3153A35A9CFA503C22353FC42992C1326287B6CE763098CDE0F37907191FBBFFA3574A0FF201E50B0461E803CBD71ECE5D9882AECACE26A6CE396D68D0DE9E44097B3F8B5D3BD37579FA5CF5A23B66472C45A19304E31021934152A206E624A540823A5E2612091E4BCC8CDD9A9C99F0CF96B3DD96400C4870C9F4D02C646A54D0D6A52A41732B7953DFB5D00EED57281D0B4EBFB1920A9A0F1E382E0C877EFFF573F35726659916243E131E1592AD54F6438C321CA6C7AB412C0A81390F6A204221830B0D8D27D1F3E9C27B8D2ECB7ABA8CE3D4238FC441BA66DA12C1F6B25BE0736E2505C21B677716DF42945AB0F1F187C14F6B3DE7B6E74F06C2CFD3ABAFB1F2A615493DF6F5A2A2B405C90B1337391CEE38CD6C15383C1698AA5603D94824E1CC090060DC5683D6E9882EBAD0370A4BE7CF0265B46C5A8FDEF2298D3C95FBE71AA49801A79781829C34204BB49E9AFF05416ED857B45E2816AC6BAAEA5C5D57DA86B3447AF164ABC16E1EEB8F59AFF88CE4DA79061CAEE17B2D3CDFEA82BBFDF85FF638AD56308ED1704E370654B802B233926049563214A24EB728270510B743837E0C9665A7C3B21D5A1CC5E220001211253C383775F623197929A6E7F73EF14B3E4D0DA98B67CE95A09E9040651E83F0FA3C5C75C5960F58C06DD7DFD48FE85A0E0829D1A6A93366807E94817EBB878B3B82F6710F391109854AC5A0B3F9E0D11CD4F5ECAAF254BFB1226009298EA3E9894A504685434767DFBECD077FB2CFEBF543BC3A1AD2521E001FEB064C40BD7D26703ADC35D8F2812CC0B9CC189AAABA2266CC8B72FB88544CBD21A15208090B41F982FA41D46289106F89B150145CAD6B84D3DBF2D28288286CDE639A67FA068C7229F2AC66961A66CD9802B3674E4141280796A41137A0FAA13342778F093E7AE5AB62CC03C1B580CBCBC1C6F2E6A1961A6D98FAC144BB6D3CD6A41F145A0CA81A0E0C42C2AC1948B9185A2FD6C399ED0539E8F921BE31C57B458D15372BBC2E5F074D11C9FD03A68821A30DA6A864C8860CD45FEE86AE5B06E8ED32D5FEE3B553B81FD463F0C424EC27C55D1A92689EB184DCFBDECEF2F2ECBC15704EFB2D529E8F95EBF8EEC71B544D71A3118905DD3672C3FEE28AD5ABE6728B65E5CD7070F3972B7067845F64782AF74EAC0534DFE9B8F9BE8FE22958EE1BB35EAAADACCEFA1EE5C0BBC1C6B761C30CC346BB463D17B595D716714136E6AD0BDAE70A302D013F6C84E3970FDE3A7726517EBF3DD1FC6F7CF3DB135F567F280021AF9CE4D17BFF8F3D9240A605DF3C30FE23C000440D617EF639534A0000000049454E44AE426082 union all
select N'3FA63F65-04A9-47E7-B27B-2C6CC5EBB9A9', 3, N'客户关系管理', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000001974455874536F6674776172650041646F626520496D616765526561647971C9653C000006A54944415478DAAC57796C145518FFCDEEEC6EBBBB2DB4DD6E4B3916DAA485B64041B9022D20898A484C2068D4C4BF880231C67804D4E05F5E09C668D098104D8C89268620523934FE414B3C820A4269811EB6DDA3E75E9D7667773BBBB3E3F76666CBF6C2AEF2929737F3BD37DFF77BBFEF786F384551C0711CB26C86AD47F08CC2E1250558A9E8424EEB3738051F34BF872FE935F56F8AB86C01D4EE45415115DA5C0B0B166C5A5B8B9A4A1752290D82C1C0A1ADC38DDFAEB6C2DD171E08B6A3A6F514C2F70CC0F25D2870AE46E8E1863A6CA8AB855F6C87107743562475DEC899312FC785625B152E5F6BC50F97AE21D08AA29B8D08CDA693CF827653F12AB43C54BF06B52BCAD03A700A713936413D6B8C8791D83086465B69CD0324E0F023F7571B1AE1A229295B068CDB5EC7DBE4C4C3163339931E1C85F978F2B12DB83D7C1E8C752EC3383240B06E34002B9C8FE2E4B95FD1DD113AF8CB473831534CCCC68069EB1BF0559697381FD9BE11F3F2EC50C8A22C2BE8095CC5B8A419E0A618CE6C2919E8F25FC48695EBE0F136BD40A22FA8C7E702C058FF2ADEAD5C56EADCB7B3017DC20D7847FB4144C16E5E8032FB2A446302513D089538922B7A37F34648495993531F4B8AA82C499046AC2049EE5C01E4182D7879C7A67568F19D454C8A4E4C08A280C098074BF3772038F6BD6A88371850E658867CDB7C9505667B540C6320D08BA49CA275EE3453FC5C8330D76232404CF6623412BD1364BA16910085780FE424F98937C055BA0AC9948821A14BCD0696093673315C252BD1DD7F1D63B160DA3DDC9C0098ADE065423E18EA4232A1514CAC82521C9C415B13106877046041E15244E221728747B3C03157C4301A15281DCB50685F82FE900712C54CC311FCC9C938D47C0CE7324326330BB88D07516E9A8FAEAD1BAA51BFAE9AE41675515296E0EE6F4367DFCD09BF533CA27CD132AA053DAA4C05C869B1C0F326943BD6A328CF058391A3770EED5D6E9C6BBA8CD111E91855C9D7488B3C09C0CABD701454C17FF0E95DC8CFE3E11BBB420115D152C26843A9F53EC429FCDB3D7FA0A6623DF2AD8560DF3266C55818DDFE2B080943B0984C585BBE0711A90FA1783BA5AB049E82CA61AD84DDB4109F7E751AA12169CFCF1FE23BB69534007EEB617C5BBFB176774DB50DEE91CB6ADE4F4DB1CAC207916BCE4370BC1363E36EDDAD0AF22C2E382CCBD13BDC02ABB90851C58370DC3BD9E9F4E2B02D415228C7C90B4DC34DEFA8C5299E8E815C8EC7EEBAEA7274061A29A826470C2B3AF3724A48C8A33D741EE3C9984AB7EE0D8CC55B10E03BE12AD806313E0A7FC84BE7C2F43A31247850E5AC60CFCE745AA601A8CE0EC73A2125A65715C64671611D7A43CD886718CF541E4DC4D09D68C622DB6624F54235531B163A313FDFCABE3065668181290D8C7A26227F2A80A814A4DD89D38C4F02918C226A0E43261DCA0C00D8B7C1311FA5F70C69C82623A2386110535C1016BD48256749E60C106C1D03909AB290538F6B6220A89E08C1A9005289082E7A3DD85E56A60150325860CF23D1A1E9057FDA16A90AC687D4B3C2C4EB253A7D51E1B4A3A8A707F0DFC25BF49650A9D73F1D6F3989377D3EA291E8512843D96ED59ED0C6A43EB2DDCDD6159A8F0840DB4D2014D60E242663FA464648DE467237BE6E3D8DB3643396C9404CF0A1FBD619EC23A4274B4BD8D1AB0512DB41829007FB8192527A9FC24EA60B9998EA0D0283F84988C065B1A1D246A196905420A1C1EB78FFD6399C61B1C8363DB512B24C28CD2BC5F2D58FE3684E013633DF335F5A0C4658A90E9B1C092C2ED5012893A9672A3C83240E18F00A553E89900EF42B78E213ECD42F236CC77EDDB838AD12EAAEA2AB078AA81307C8D719327D7308172A962FC6B1C830047E1C8B68D69673C77894547B2944AC710E479DB944B784BFFB92D87F1CF5B482B84344F7794CDFB972B71B11AF17098B1E2376EA0B3F7F1E97EEAF5A8C8B74A4370EFA21EAD1C30E283BED658BD58CA79C39F005E3E8F44978EE381A68BA47DFB1F47F2EA58C1556B9969D20106B2A17A891D9E3B7A1A2621C2DBF0F22D766C0DA9A02F80251747863383047E3995970B726E98A7A9EFD180D573A06884319D5EBB7C16E50103702D5D585F085E2E8F064677CAE002681384020AEDD0EA3D8590C4996B16A4501242189DBBD22D85C36C6B3BD96A7419839BD3AB12CB1E79A2086B5F3815A5F36C6B36120134484D34BDB44EEA49FB56897B251C8E33F3416F93CCFC364325275E5A86071C8FEF732BB5F33F3672F6A952B7D58C9B25EE739ED90495FC9D26DFF876A1A4BF70A804D2F4E6C34E9A37906F7887AC161E3903EDE131730A561DDC7735D9F98CBC27F041800B84C0B61515980570000000049454E44AE426082 union all
select N'8D38A297-E75B-4315-B34B-2BE7BC82AD2E', 4, N'供应链管理', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000000467414D410000B18F0BFC6105000000206348524D00007A25000080830000F9FF000080E9000075300000EA6000003A980000176F925FC54600000006624B474400FF00FF00FFA0BDA793000000097048597300000B1300000B1301009A9C18000008E44944415458C39D97796C5CD51587BFFBDE9BDDE319CFD89EB11D67716CCC96B8843D44101228D0B422402084D00515B56A690BED5F45884A111415415BD422014504B594B63469106D69424B12925859C86612307108B1B33878DF66E6CD5BEFED1F9E54696C43E0494757EFDEA7FBFBDE39E7BE739E504AF1052F31C5FCE7DAD0F83C0FAFF8F57BF381AF20B85108911282460021F858298680FF08C59B7F7C70EE8E730511E7E281BB7FD9B652089E989616D3ABCB82C4833A95F1BDCC9A31805492D17C196D6D319CE085F49B1A7D39FF388A475F7DA8E595CF02F95480BB9EDAD72834B1F6A25AB3A526DECFC54D23B87E9A23BD17D0943A4855F54EA4F478637B919AECFD4C2F3FCA879DB3711D9D014BE3E890F79E21C45D7F78B0E5A3A940B4A9C4EF7C72CF2294DAB5645E77CBF5734F110AD5A1DC51B2C976A637C4683766501D3F49A6FC04FD89C7D9373C8D17D60F725254306204A888BACC6F1C6AF194DCB1E2E9BD374EA53329C0B25FEC5E046AFD3DD77F948AAA4DEC3E1923515B492875014ECC626331459798CB63EF1CE7B59365F88904B23C4562EE7DF4FB95649A3CBAED728AB9F759D87828A574F1AFBB9FDEBBF89C00EE786257A39272CD8AEB3E0ACEAA69A39B3B71AA2FC1CB543027B39ADAB21BF85A739044A541B0611BFB7A576384757CDDC0330244CAA3C42BD7D338EF084E792515C93616361FD3A5526B963FB9BBE96CBD09A74049F5EA57AF3A959A59B31F543FB7B6FC8968F0113A423102BA24C5AF984F1F07A2B7411AA231702C901E48573096136C7E3BC1B23BD6B278DA217223492AC772782CAED8DC36EDCFC065537AE0B6C7B62F9FDB90BB626EE31E1CAF07C318221E394A32FA1D1604FE8DC0012C60030BCB40D74BA6412000C110108021E75E32914388F0087D814FF002DD9C57D7CAECEA914B6F7F7CC7CA490196AE6AD59594AB5A667F4CC1EEC19383840379828685AED908F13C2001F8E1A616621C4593E3E2BA0686068601A918FCE5DB3762E2F083474EB2E1EF826DEF9EA42FDF4D3A740CE9FB8F2E5DD5AA4F0050525ED790359BD3C9C3149D415C46915A1E5D58082CC0046CD69D9AC5D5577E9F9F6D6F00059A024D8C6FA433EE919D763F7BB66558B4E4294EF4AF644BEB42FEB9CE2459D1497DA5D9ACA4BC6E420E48296FAEAFEEC77486B1BC31943429024504E1D25777F5FB1FB22FF716992AB87026287FDC0425080D9261383F54CBF9D77632A29E2611BD954D1B17900E97D3B62F47CDCC5EBA7AEA6F0636FD1F8092728135D28F69E5B0651E4245C63C455CFA04F0F050B40FB4535D35FED69ECDB8B204E982F04128B8E5F23788314A0E89A714179DFF5732C935E4071204D40C86FBEB50B26EC1040F28291BF366235B37BDC9DC2B1D44C0A56740D276608C484CE778DF55F8C924B609011FB4C078EC7D05F8E0FB306F3AB4F07372085C25F015F88EC0B3414A0F256C34954749D93819404237D2F40CDF84B5FD05AE5AECB2676B842D87BF4BA422CBE22B1B710B602B90FE78C2F90110625C5CB9E09B07705038085CA5E13802D714E08F27AF4092C958282913139250FA12DF93786A16DBF7DDC2892E8B54A6919BAFC9F2A5D997625B9299154729D715B6097611BC22B81648073C17843F828D87AD5C1CE9E2580ACF55201586005D1744630EB20474F62918F2A544132192E96B79EEB9197C78D02753F1376A131BF8E04898CD3B46E8383A88390656018A79B0F3609BE059B06E570D3BBB06B1518C8EF9140B207C815006866610D403483F8C927274B21074F8BECC6A28A4E79399FE13F6771CA6756F1B4B6EE9A42E359F51A71EE504B0F22EBE6B10080A74633C0C4A41A6BC8955AFFC147FE421FA479A999618E6C7F709743D88D07502469091D10A9494ED93016C326DEFBA4840600885F42156369764D5D5EC6A3BC87D2B5F269BF5B0BC0BD9B2FF5BD87E1CE919188681A68DC7574A97394DF752282CA1598295EFC1B3EF21A807D1F50861A38C8EDE144ACA1D137340CAD77B876D123183481074E9E3DA36AE6D511E6F265BFB06D1B2F5D4D73CC3DD37DD4F43D57EEC6211DB2AE0DA16AE6DE1BB0E9EEB12D0832825091871F6EEA942880041234A3894E470571C29E58609001B9F5D7A7068CCEAD0748D443440D8F0D1A4875B2CD230733DA6E9639A927CC147137B68C8BC4463D56E6CD3C42EE671EC22AE5DC4776C7CD741F92E4AF91891DB31B41891608AEE53B338DC253A363EBB74CBA4C54849F9F060CEA33A19221135086A2ECA73B146F7932FB814F22EF9824B2EE7904AFF83F997DECB8A1BBE87E60E8D43582508DB42792E3A3ED974915020493454CDCEB62C4ACA87A7AC869B9F5FF6FAB1DE7CAB8B4E361DA622AA13D61CF6EECC901B16E40B92425E92CF4BF2399F7CCEA7FBD87E068772D885024E318F6717F11D0BE13B0484CB89EE34D1601D3BF65ECCBE76AD75F3F3CB5E9F12A0D40FAC6C3F36D61F8B46A8A98C902A33282F5FC486D742E447350A79453EE793CB4972A33E07F667712D0FA758C02916F11D13216D829A4B79582712BF9C4387E7B0EEADF27E25D537CED69B00B0E5C5E5C79594B71FE81C7512F12833B265D456A6299A5FA7BF3B866385314D83421EFAFB2447BA2EC7B30A48C744F38AE8D22162F824A306D96418E118BCBCB6DA5152DEB9E5C5E59D9F0900B06DF53DAD4AA9C5073A477B02A1104DF5099A9BAFA1323103435521DC14F9C124BFFB6D3D9E7F0951CD21AABBC4838A744CA72619A6BE32C250DF306F6FEDE8514A7D79EB4B2BB64CA635695B2E840010972D7B6656309AFCFDF46C6241A622C4BC2BD6E2CA117C55E0C0BB30603E801108120A0588468294C58224E3616CD3A6F3F820873B7B5B1D73E49B7BD63ED4A9A6E8FFA702108C17DB08903EEFDA07EEA8A89DF3A36C3632B3B9314F3C30446F6F1DE5A9267443C7F57C0C436364CCE2D8C921FAFA06BB467ADA9FED78E7376B8041A008A8C920CE05A012A801D295B3AE6EAC6E58704D34593747685AC408C6AA013CA7D0A7A42C16474F7DD0FBF1B66D039D3B8E94843F0106BE08002580001003E2250B03A5D613FD8C1C92800FB880CD78E79A2B5901703F6F084E038892905112354A765AFCF41FB23A03C22B995B1AFDD2FAA40C9F06F0BFDBD2785A509C317726C0E9F1B4C9B3D6984CEBBF920F82117848D9070000002574455874646174653A63726561746500323031332D30342D30335431373A31383A30352B30383A3030260834CD0000002574455874646174653A6D6F6469667900323031312D31312D30325432333A33313A35302B30383A30308EF99E190000004D74455874736F66747761726500496D6167654D616769636B20362E382E382D3720513136207838365F363420323031342D30322D323820687474703A2F2F7777772E696D6167656D616769636B2E6F726759A45F7F00000018744558745468756D623A3A446F63756D656E743A3A50616765730031A7FFBB2F00000017744558745468756D623A3A496D6167653A3A48656967687400333228F4F8F400000016744558745468756D623A3A496D6167653A3A5769647468003332D05B387900000019744558745468756D623A3A4D696D657479706500696D6167652F706E673FB2564E00000017744558745468756D623A3A4D54696D65003133323032343739313039E808ED00000013744558745468756D623A3A53697A6500352E30354B4242B7F0549000000060744558745468756D623A3A5552490066696C653A2F2F2F686F6D652F6674702F313532302F6561737969636F6E2E636E2F6561737969636F6E2E636E2F63646E2D696D672E6561737969636F6E2E636E2F706E672F353638312F3536383139342E706E675AA37B080000000049454E44AE426082 union all
select N'002F6084-7E76-4A71-9C78-4DECB297247A', 5, N'人力资源', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000000473424954080808087C08648800000525494441545885ED974F6C545514C67FF7BD99D7615A0A9452FABFC828509A18AA4050CBA2896094286143D2000969DC1137B8204A74634280215068085194854A62DCA89840241513A8D14A4A0742B448ADE3D0526881168619DA99F7CE71011DE95F34DAB8D0939CDC9777EF39DF77BE7BEECD7BF02F9B99CAE47A8B1964F943A83307D12C2C49A2728D9CD425634803F8A60CFC1EE5A4834FA15995185F09862CD0BB183746C214AB0E7D6B0C892921A0B798413A588D15780E02D5A87F3E56961F1D4A42FA670CD3B96B5C183C35350A38FEC720AB12024B302595D84F3A68AE057D59486B35480AB4576F3B111F406363E322C7713E139105B66D5B7F17FFF8A976CA4B2E535E720527B0049BE7307609E25EC24BA749DEBDFC926D0DF4CCCCE9FEDA07E038CEE76BD7AE5D3077EEDC7FA629BDAFC0B5406E823D07AC32B097821D075F3E3EEB5AE0D2E5E2BA990BBA1B7D00A954EA8987C1C57339F6FE9B9C6DFA188065CF6FE495577760D93E54155505C83C8F764B6D2CF5636900637A8176D09B2031909BF89D9924EEF967610D0D0CF7C008D94F1EDD41C7B9E3AC7EF1655038FBC3714E1ECDE1854D6F8D0B389A8C680168116831B6B403434036683F2ADD18DF12EEC4070DD9DC1877BF4F1F3BC4D265CF32DBEE27CF778BA5CB9EE1F4B143880822426B6B2BB1580C55251289108944327322423C3197C31F9CA63532842B8F23EE4DC4FD19CF4BE24A1558555CB93A1F63F0263C058AE27A1E8AA2CA83F1BEF7F4F4D0D2D2422C16233F3F9FF6F67656AE5C494B4B0B2B56AC60F5EAD508CB693B7F9E78DC6570F02E458579FCD2798D97D6ACC3F1AD2179EFD3B1D20FDBF48A4534379FE2969747BF379B33674E316BFE924C854D4D4D949595110804F03C8F75EBD6D1D7D7474D4D0DD7AF5F07E0BBEFDBF8255AC46FDDD59C6D7B8C731716D1115DC1A0BB064C41066B8C025F357D44DBF5761E0F4EE3CB2F3E413C2195339DBE5884B3AD2779BAFA7976EFDE8DAAB26AD5AA8C2AB5B5B523FAE0E0C183E3F688888EC01B414044387CE40D165696E34A2F6E9EE0BACAFC8A45CCEA4F72E8BDD779EF601B9D9D9D0C0E0E66123F0C90D942551CC7A1BCBC7C4CC34E48209D4ED1DF7F03438878FC36F22020955272738AE8B97601D74D336FDEBC479E84C97C42029665535A1222FAEB6F2CACACC6EFF703E0BA697EBA74815933F331C6A2B3B393A1A1A131E09355FEA7088808DBB71DE59D9D757CF3F569FE586B282A2C67FBB60F51552A2A2A1E7D17884C3837210155A5AC7421EF36B63E32F164923F6A6E84EAA315D8B97327AA4A434303BB76ED42443263381C261C0EA3AA84C36144843D7BF66448EDDDBB3733EEDBB70F11A1A1A181FDFBF7A3AA1C38700011995C81E12A8717FE5579C75368B8B8E1F9497B6074E287034737DC7849C78B1D4DF24F295057578788904824D8B06103C964928D1B3702D0DBDB4B7D7D3D7D7D7D6CDEBC998181014484FAFA7A1289049B366DC2B66DE2F138EBD7AF273B3B7B4CE5132A304CE4E2C58B1863482412F8FD7E8A8B8B8946A3048341727373292828A0A3A38369D3A6D1D5D58565592C5EBC986834CAD5AB57090683844221BABBBBF13C8FDADADA490988AA5AC618B66EDD8A885053533346BE50283442CAE5CB97A3AA54555565DE1516168E880B854219B02D5BB660DBF670A19A21E0F7FB7FECEAEAAA2A2D2D35C64CE9973A2242341A55CBB2DA815C1F106C6E6E7E2D9D4E1F01E631C5FF0A80BAAEDB75E2C489B781D906B081BC073E03C806B2A6083CC5FD0293C01DA0E7E16A6DC0E1FEB6385344C0035C20FD80CCD8ABF17FFBCFD9EF4FD7ECB9F43FB91F0000000049454E44AE426082 union all
select N'32414B30-E4B1-4570-9066-43B8DF894793', 9, N'主数据管理', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF40000001974455874536F6674776172650041646F626520496D616765526561647971C9653C000006B34944415478DAAC975B6C145518C7FF67667676DBBDB6B0B45C1A012B145A020F80C85D632354EB830F26FA84BCA80924E2832612316A8248144D341A5FF0F2200F184C2411B98802060A6D2DC8C5729102A540DBDDEE6EF7BEB333E377CECE6E974B776BEAB45F66E772CEF79DEFFC7FDF39C370F7C13EF9EA9BC58AAA3E2B4B72AB09F818637518C3619A662F03C2BAA1EFCD66323F6D7A79DD297EBB5C3B669DE5CFBFDEB55552E437DC4E37AAAAAA505D5D054551E09F3891775EBA13C6301808209BCD62682884502884682C0A43D7B76F78E985B7E815BD5400CA673BBFDFE3F17A5BE736CC81CBE5846118C269DEF15802C89FB9499284743A8D3367CF623812D9BB71FD8BCFD1E3EC83DA2A6415B222B736CD6914F98AC5E2651D964DAB1508EFF364C7C956EE832C3A5A000E5DD7E1F3BAD077BB7F6444BC93FFE8D8CCA5AB3080A9936BC0FBE63E4A05C078CBDA9A0922F57DB7FA91CE64C0242B8031045270CC4F8609BBAA62EA941AD1A76AB3156BED810188C7A73A4E635ED31C4CAEF123994A23148E209DCA201A8F91B8F49201288A0CB7D305BB434595CF8B0A871DB1445CF499D1B4D26DAD9C239948A1ADFD4F785C2EEAC407AFD703B7AB12D3A74F81C8A8C9FFCD7B14CC60FD23164F40230AEE0C0C204CC147A331EB392B1F00230F9294536F8A463D30184020188622CB9065458C9023C9AF15C5467D4A94150D593D4B739C85A6E9E29CA5F9360C7ECFA076B2089A9511B492C72C2FBC7C20B27CB7F17BDC720A47EE3D53A2B6FC3977225919922C8499485B39A2243223180CEC1F0C060BF830C6C658BF4A6318191E463030B89FFB28154056624C3789803BFD03944EAD30DA821505369A15BFCFAF63D1283244135DEBA315A1C214B83DBE96C757ADC0D13F8EE3E2E52B02232E4487A3121E8F4BE8C04DE29408A99C835CEA254A6F3C96465A4B43275242E11012890459126EB70B2B572C45381A6D29A701C5A40CF55CBB81279F5845E8A531101844904418890CE3466F488849E8820C7C9434053A652C57B20DF1BCA2D22EC899F1D034F8FD13E1B03BD07DE52A92196D84B65102907218C5D1D175061EB71BD534FAC6C65AD848F94E6705F23A32F33C8E40982B54F4231E4B0A0CC334EFBD7D771089C6A9671B7456919FEA1214140927994CE1767A80B230741786B26C2BC290591872FC3451A8F218F2D2ABF3E06C4EB0EEDD68EAFD0D87DE59DA6F30853B49670DF3D396778F6ECEAF90561DC8D77FDC27A811CB216AD16A61C8725A90CC82264C3E3DCC0EE9C0464CF225D1D05A0F87B7524C5D2691B177B7F7BD79F0BD55EB9BB71C994AAE350BC3C11C86D23831649248BB797E17FCDE141634CF874AE28D0452E83C7819174EF4A27EC1544C9AE6F3FFB879D936BE0F111892AA296F84E19D0763C8C6842113B882A6CAB87A107397CFA52E29C192822B5D37B16147DBBA573F3EBEEE62E74DCC7DAC1E2EBBF23A5F25C514783C9E96D5AB09C363A363C8AF47021AC150D308455A783886C3B4978824744CD382B057D8A19B3621555A20D17669A883FB3274130AB93575519B2A2C0C4DC2F03A61B872540CAF9163A518433D8721FF63790C29C8BA9975186AA34C502699EA14029FBD70364E7D219DE313377BD16C5085A2A0044D92C030BF131A378634D470CA44860661C668359CE81775C23DC187456B165BEA9591BC7A5EC8E53E0AC687A14EF5965649A9122677E4702275FD6FD86AEBC06425B7D099B45A066FE1C2E93EF4F4C7BFE53372CF6A380E0C19D7861DDAEF1F60DA237530151512E920FDCF59D2A52ADE8F0DC771F97A1AFDE16CE8952FBB3E22D7C9C28EC8A054D1B7C01810BC174326D0A30947F2F0FBA831BB306F5923F44C0C8ACB4199F4E2AF233D2200CA8C76F9766CCF6B3BCFEDA0867C039AE20198C3E1D0D14B97BA57CE9AD54004D845107C43F1200CF3D78641D7FC370F4076207A600B2668ED685A3E0B462645ED197EFEEE021455C233DBDAD7929F0C2F3C6421CB7998574399178363870F9D787856437522996C0A0E05914A2645C77CC7E3717B61B32964AAD860F233DF1DF11189A223576068FFDBF0A58E63DEA3947A43C7BEDDDDE8E90EC246CED76C6D5F493EAE91DD241B200B583BE46CE1C384CC4B564B366175F353F31B1A9B96FA6B6A97D094386DAAEACFABDF44310412955C153307F7A2A9FA061A164EA1AC10AA3609BFEEBD85A7B777AEB546DB6739CE94FC34B3F6EE4EB24ABE45B0CE76EBD968AB99ED974DF5FB9A9F6FA0953027E2CE6383543F34B46CEB6CB01CC747735EBC4EEBD68B09EB9E6A5929E7FCA00F483BF6FD709D1293D303AF2C2D1F9E5E62390F615C9BBBF24715D90CEB9C3F62E5D2FE7F06A05AD3662BBAA7954B7BF1F1AF0003002A41279E6D5AD2B00000000049454E44AE426082 union all
select N'BDF57601-024F-41BC-9EF6-C5F2C334DF79', 10, N'系统设置', NULL, 0x89504E470D0A1A0A0000000D4948445200000020000000200806000000737A7AF400000035744558745469746C6500546563686E6F6C6F67793B53657474696E67733B4F7074696F6E733B437573746F6D697A3B50726F706572746965730FB5035C0000086A49444154785EA55707509557167E10B1AF1A35666349D9D58C99D5CCEAAE63A2D8161224C24250411050101404441E20451E55AA0F1011F419E105FB121B0A968048513AD2A4166902224A918EC2B7E7FEF33F86D1C944867FE69BFBEEFDEFBDE7BBE73FE73BF70944FE52818BDFAF04A9E0B06FA4E0B04FA4801E3982BCB9D0E713550D0305D697417193FA44230BD13CEA7FC4E6B0315BB77081D0354C20149D10D8884205B207C09F82191DE90C0F83336EE71EBED2C14312E5218EEAB3770B97D0D804DEA08250142A11F945F6D9B81C3FBB4FE8B34A4662787858C060ED1C3236024EDE6704FCE29193DBBB9F3C9596FD04038383B81E978A034EC1421A9F62E520165E8D4DC6C0C020D2B28A60E9181CC193931BA2F54343C3024B87A0B11170F03C3D629CC7043A5D48D2A37CF4F40FA09F489C94DE808D4B28C223AF6370F02D7AFA0691909C03735B7FE6EF49CC3BB2F5E67647C74640766A7E93096C434B07B1F78DDBA9E82543ADAFBAE8C46F4027C4E09B217474F5E275771FAEDC7A803D07BC98BFA713261326CABC315602F2A6D6DE4B0E38071F35B70BB032B70D703BEC7DBAA7A6BE199D5D7D78DEDAC9A1B9B503CD2F3AD04460FD9617ED70F33BFDC6D0CC35C068BF47949185A7546BE7817FC9488C85C0044B87C013BF3FC8C6A56B0984783C7FD186EEDE01CE50634B3B1A9FB7E31987360E0DCD6DA87BD68AE6965788BA7C1B892939B84FD0DFEB12C57B43FE83096CDAAC3D53E872BCB3BBBB177DFD83E8A3006B7BDDC30C33438457A86F7E89DAA697A869E480B2A7CD2828AB477155035EBC226FBC7C4D6D07CC84BE3DDFAD57FB8CC5C4071358BA6CD5E47D367E51D76293C8E5BDA86B7A853A66E819432BB20B2A60E724868E812D848E623CA4ECC82DA9456E712D728A6A3854D3BC9391D7B0DDD0FE04CB96317980673BC5C04CE4795C128D176D5DA86E6841757D0BAAEA5AE0203A869FB5F7FBD09C4FB7EA5A899DDD4238E39985D5C828A84615CD73F5390DF5EDE6229A3393A030D61890E75369B6B195D7407B47172A6A9FA3ACA61965D54D30B5707F3B77DEFC45F47ECE578BFFB178EF012F8E58DAE34AA4E55672DED23172643BCD274C63071A7310B2853B8C1DC5C74F47739FA1840C175534129E417CFC1C761ADB7BD19C0586A60EFE81A1E7514B9F2735BB0229D9657852DE00375F0954344CDC68CEC784896322F0CDF2D53374F7385DBA1293480233805A3A517E593DA10EF9250C4F111E71158EEE27202141AAAC69426955231E64942231A30489E925A8696845D82FD1F849CB5C3A6BF6A733655EF82002CBFEB976CEEEFD1E1D14C5946E6D9C7B0B4AEB29B86A919E578DF4FC2A945637E239D3014ACB92CA262465952121AD9808142323BF12A9B9E5785251876D06763D7FFB7AC582D15AF02198A2B5D3E6A4F4422C82C22F2130EC221EB28DF3AAC84029EEA71773884F2B42FC238642DC7B58C8F5EFA5E6C13DE00C824E5D4698F43A94D5F79C2703337802F21F4A4261FD8F3ACBD5B52D8E6CD6343DA4A269E26F64E1DE9D9856486E2EC13D3278970CDE492DC09D9402DC26DC4D2D44ECFD6CE89B8AFAD7FFA817BA49D550A2A4667C71E5F7AADFB103C9AAE7E8923D82771F7E8202BFF02F84794A5B7605F8874421B3A00AB7930B90422E4EA7DF0F324B71F3412EE71167AF70AC53D60D61C149984398C567C124B700E92E9F90CBF03E7619AEFE91BBF8FD0923C4E4DC03A204DCA3A92BE40BD2089119CA6A466289F41AF24BEBC86809AC1D03A1BAD5826BE31F169117F2E11772168ACABAC798F1754A6A733D83CE257988CF5D34B3F6FA96DA9B2515F528A1B810F9496F19EE7558EEE27BE6BCC82F226D83B2E6ECD1A92A50D7B1167476F77224F817D394D48CC2CE9C8BA1202CC791A028909BC3697CC9BA1F742581611770233E0B01949EEB7ED03BC952CF5D7CF67E554D230A9E3C4568C48D212230D4DFCF2AE96BB8F94B87822557DE64D35EC544C8C1F35432AF3BF21C812DDB2C05EDAF7B046D9D3D23A2B4F2FB2DAB376E3694AA695B766D54DD7593C6FECA0CB176838AC14D552DF3EEB5CABA51CB56FE670DFB6C22DF88E882E26A74500D69EFEC467D632B776F686EE9A08CAAA5B23EC095F0B8840CE89A385F1F2DD70255ADFD02152D7381CACF66A3BD30993073C117DF7C3EE79385F3585F162753A7CF9AF3D9C2255FF284A631E819DBAE0C3E7565A8ADA39B956C563159CB54926904B5ADA8A0026679E8E8D09A4D1A8A8CF41FA5EAE87898302A70E4DF199FC8BF9B64ED1CB8DBCE3D2CCEE9C82FC3CFA86A3650314B487E0C4BDBA3B0B2152329AD00E5643C39E309AC1C8387F65A7BDFD33771349EBFE8EFD3655EF80312EFE0DD71BE881DF294E071612527E13DBD03ECB424DDAE58B361BBBEA2928E81D9C100D413A9D4AC0A24503AC7C56762EF415F300F73870130568C4EDFA942511872F2CBB90B4A556D33CAAB9BB1CFC61FCA3F199AA8A8EF3675703B854A2A6EF752483B92F270E6523C8C2DBDC0A7EDB8094C52DFBEDF5253EF60BC85BD78383DB78C4BDDF8943C387948E0EA27452209591ED5948B31A9ECBE00552DB314CA2A217F97FC683C04E4F87898B166A3FA5A478F136FABA98E2493582592505121E3AA6A76510DE292F271ED6E36F44C0E0F2D5EFA6F155A33974F45B9F110905D66A6EB9BBAFC76353605855445EF24E7E3424C0A6EDE7F8C98DF73F06B7422A26F67E27F7159F00A3A8BB54A3A31ECEE41501837013E0EA6E99B1E4E8F4BC8C231C96FD036B483DE1EE7E1AB77B37039361D5BB6590C6BEFB21F763F2A4540783456AFDF9A29FBFEE32520278B83455F2E5DB8DDC0364D5179C7ADAFBE5EB1E5BFDA960F232EDE4190E42A53CB475F2CFE566395A2E60D42CEC773172C66C1CBD68E97C07BC2C58BD3BC4DAA3B0F6AE81E84C60E6BAC58BDD98657D2B9FCFBA9A345689C04DEFB67A5C09399C188F098C98DBD2F6A827110F853F1E2D59261B4D1F7A5F7FF2C19F2185910BA830000000049454E44AE426082
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
