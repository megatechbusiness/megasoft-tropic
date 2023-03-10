USE [Megasoft]
GO
/****** Object:  StoredProcedure [dbo].[sp_BlendMonitorLevels]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_BlendMonitorLevels]
(@GUID UNIQUEIDENTIFIER)
AS
--DECLARE @GUID UNIQUEIDENTIFIER = '4daaa41d-1794-40c6-826c-9b2662005835';

WITH Data AS
(
SELECT Tank, TankType, StartLevel AS BlendLevel, 0 AS RowNum, Product
FROM mtTankLevelStaging 
WHERE GUID = @GUID
AND TankType = 'Blend'
UNION ALL
SELECT Tank, TankType, EndLevel AS FeederLevel, ROW_NUMBER() OVER (ORDER BY Tank ) AS RowNum, Product
FROM mtTankLevelStaging 
WHERE GUID = @GUID
AND TankType = 'Feeder'
)
SELECT Tank, TankType,Product,
(SELECT SUM(B.BlendLevel)
                       FROM Data B
                       WHERE B.RowNum <= A.RowNum) EndLevel
FROM Data A
ORDER BY TankType 
GO
/****** Object:  StoredProcedure [dbo].[sp_BlendSheetReport]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_BlendSheetReport]
(@GUID UNIQUEIDENTIFIER)
AS

--DECLARE @GUID UNIQUEIDENTIFIER = '4daaa41d-1794-40c6-826c-9b2662005835';
WITH Feeder AS
(
SELECT GUID, Tank, TankType, FeederStart = StartLevel, FeederEnd = EndLevel
,ISNULL(Product,'') AS FeederProduct, ISNULL(AluminiumSilicon,'') AS AluminiumSilicon,
KinematicViscosity, DensitySpecification, FromTemperature
FROM mtTankLevelStaging 
where GUID = @GUID
AND TankType = 'Feeder'
)
,Blend AS
(
SELECT GUID, Tank, TankType, BlendStart = StartLevel, BlendEnd = EndLevel
,ISNULL(Product,'') AS BlendProduct, BlendNo
FROM mtTankLevelStaging 
where GUID = @GUID
AND TankType = 'Blend'
)
SELECT F.Tank, B.Tank AS BlendTank, FeederStart, FeederEnd, B.BlendStart, BlendEnd , FeederVol = BlendEnd - BlendStart,
VolPerc = (FeederEnd / (BlendEnd - BlendStart)) * 100,
FeederProduct, BlendProduct, BlendNo, AluminiumSilicon,KinematicViscosity,DensitySpecification, FromTemperature
FROM Feeder F
LEFT JOIN Blend B 
ON F.GUID = B.GUID 

GO
/****** Object:  StoredProcedure [dbo].[sp_GetRoleAccess]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_GetRoleAccess]

--sp_GetRoleAccess 'ADMIN'
(@Role VARCHAR(50))
AS
SELECT _PF.Role, _PF.Program,
ProgramFunction = _PF.ProgramFunction,
HasAccess=CASE WHEN MRF.Role IS NULL THEN 'false' ELSE 'true' END 
FROM 
(
	SELECT Role=@Role,*
	FROM dbo.mtProgramFunction MPF WITH(NOLOCK)
)_PF
LEFT JOIN dbo.mtRoleFunction MRF WITH(NOLOCK)
ON _PF.Role=MRF.Role AND _PF.ProgramFunction=MRF.ProgramFunction
ORDER BY _PF.Program


GO
/****** Object:  StoredProcedure [dbo].[sp_GetUserCompanies]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[sp_GetUserCompanies]

--[sp_GetUserCompanies] 'JAROD'

(@Username VARCHAR(50))
AS
SELECT _A.Username, _A.Company, _A.DatabaseName,
HasAccess=CASE WHEN MUC.Username IS NULL THEN 'false' ELSE 'true' END 
FROM 
(
	SELECT Username=@Username,*
	FROM dbo.mtSysproAdmin MPF WITH(NOLOCK)
)_A
LEFT JOIN dbo.mtUserCompany MUC WITH(NOLOCK)
ON _A.Username=MUC.Username AND _A.Company=MUC.Company
ORDER BY _A.Company


GO
/****** Object:  StoredProcedure [dbo].[sp_GetUserRole]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[sp_GetUserRole]

--sp_GetUserRole 'JAROD'

(@Username VARCHAR(50))
AS
SELECT _R.Username, _R.Role, _R.Description,
HasAccess=CASE WHEN MUR.Username IS NULL THEN 'false' ELSE 'true' END 
FROM 
(
	SELECT Username=@Username,*
	FROM dbo.mtRole MPF WITH(NOLOCK)
)_R
LEFT JOIN dbo.mtUserRole MUR WITH(NOLOCK)
ON _R.Username=MUR.Username AND _R.Role=MUR.Role
ORDER BY _R.Role


GO
/****** Object:  StoredProcedure [dbo].[sp_TankLevelsReport]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[sp_TankLevelsReport]
(@GUID UNIQUEIDENTIFIER)
AS
SELECT Tank,FieldType='BEFORE', FieldName = 'DATE', FieldValue = CONVERT(VARCHAR(10),FromDate,101)
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'DATE', FieldValue = CONVERT(VARCHAR(10),ToDate,101)
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'TIME', FieldValue = CONVERT(VARCHAR(8),FromDate,108)
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'TIME', FieldValue = CONVERT(VARCHAR(8),ToDate,108)
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'm³', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),StartLevel))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'm³', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),EndLevel))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'TEMPERATURE', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),FromTemperature))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'TEMPERATURE', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),ToTemperature))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'STD DENSITY', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),StandardDensity))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'STD DENSITY', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),StandardDensity))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'DENSITY - TT', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),FromDensity))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'DENSITY - TT', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,3),ToDensity))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'VCF', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,5),FromVCF))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'VCF', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,5),ToVCF))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'LITRES - 20°C', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,0),FromLitresStandardTemp))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'LITRES - 20°C', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,0),ToLitresStandardTemp))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='BEFORE', FieldName = 'LITRES - TT', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,0),FromLitresTankTemp))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
UNION ALL
SELECT Tank,FieldType='AFTER', FieldName = 'LITRES - TT', FieldValue = CONVERT(VARCHAR(20),CONVERT(DECIMAL(18,0),ToLitresTankTemp))
FROM mtTankLevelStaging WITH(NOLOCK)
WHERE GUID = @GUID
GO
/****** Object:  StoredProcedure [dbo].[sp_TankMovementsReport]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[sp_TankMovementsReport]
(@GUID UNIQUEIDENTIFIER)
AS
SELECT * 
FROM mtTankMovementStaging WITH(NOLOCK)
WHERE GUID = @GUID
GO
/****** Object:  Table [dbo].[mtProgramFunction]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtProgramFunction](
	[Program] [varchar](50) NOT NULL,
	[ProgramFunction] [varchar](100) NOT NULL,
 CONSTRAINT [PK_mtProgramFunction] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[ProgramFunction] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtReportMaster]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtReportMaster](
	[Program] [varchar](50) NOT NULL,
	[Report] [varchar](50) NOT NULL,
	[ReportPath] [varchar](200) NULL,
 CONSTRAINT [PK_mtReportMaster] PRIMARY KEY CLUSTERED 
(
	[Program] ASC,
	[Report] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtRole]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtRole](
	[Role] [varchar](50) NOT NULL,
	[Description] [varchar](50) NULL,
 CONSTRAINT [PK_mtRole_1] PRIMARY KEY CLUSTERED 
(
	[Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtRoleFunction]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtRoleFunction](
	[Role] [varchar](50) NOT NULL,
	[ProgramFunction] [varchar](100) NOT NULL,
 CONSTRAINT [PK_mtRoleFunction_1] PRIMARY KEY CLUSTERED 
(
	[Role] ASC,
	[ProgramFunction] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtSysproAdmin]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtSysproAdmin](
	[Company] [varchar](4) NOT NULL,
	[DatabaseName] [varchar](50) NULL,
	[CompanyPassword] [varchar](50) NULL,
	[UseRoles] [bit] NOT NULL,
 CONSTRAINT [PK_mtSysproAdmin] PRIMARY KEY CLUSTERED 
(
	[Company] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtSystemSettings]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtSystemSettings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UseRoles] [varchar](1) NULL,
	[SmtpHost] [varchar](100) NULL,
	[SmtpPort] [int] NULL,
	[FromAddress] [varchar](100) NULL,
	[ReportExportPath] [varchar](200) NULL,
 CONSTRAINT [PK_mtSystemSettings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtTankLevelStaging]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtTankLevelStaging](
	[GUID] [uniqueidentifier] NOT NULL,
	[EntryNo] [int] IDENTITY(1,1) NOT NULL,
	[Tank] [varchar](50) NOT NULL,
	[Description] [varchar](100) NULL,
	[Tagname] [varchar](50) NULL,
	[TankType] [varchar](20) NOT NULL,
	[FromDate] [datetime] NULL,
	[ToDate] [datetime] NULL,
	[StartLevel] [decimal](18, 6) NULL,
	[EndLevel] [decimal](18, 6) NULL,
	[StartVolumePerc] [decimal](18, 2) NULL,
	[EndVolumePerc] [decimal](18, 2) NULL,
	[TankCapacity] [decimal](18, 6) NULL,
	[FromTemperature] [decimal](18, 6) NOT NULL,
	[ToTemperature] [decimal](18, 6) NULL,
	[StandardDensity] [decimal](18, 6) NULL,
	[FromDensity] [decimal](18, 6) NULL,
	[ToDensity] [decimal](18, 6) NULL,
	[FromVCF] [decimal](18, 6) NULL,
	[ToVCF] [decimal](18, 6) NULL,
	[FromLitresTankTemp] [decimal](18, 6) NULL,
	[ToLitresTankTemp] [decimal](18, 6) NULL,
	[FromLitresStandardTemp] [decimal](18, 6) NULL,
	[ToLitresStandardTemp] [decimal](18, 6) NULL,
	[Product] [varchar](100) NULL,
	[BlendNo] [varchar](20) NULL,
	[AluminiumSilicon] [varchar](50) NULL,
	[KinematicViscosity] [decimal](18, 6) NULL,
	[DensitySpecification] [decimal](18, 6) NULL,
	[TrnDate] [datetime] NULL,
	[strMessage] [varchar](200) NULL,
	[Operator] [varchar](50) NULL,
 CONSTRAINT [PK_mtTankLevelStaging] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC,
	[EntryNo] ASC,
	[Tank] ASC,
	[TankType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtTankMaster]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtTankMaster](
	[Tank] [varchar](50) NOT NULL,
	[Description] [varchar](100) NULL,
	[Tagname] [varchar](50) NOT NULL,
	[Uom] [varchar](10) NULL,
	[UomDescription] [varchar](50) NULL,
	[TemperatureTagname] [varchar](50) NULL,
	[TankCapacity] [decimal](18, 6) NULL,
	[TankType] [varchar](50) NULL,
	[Product] [varchar](100) NULL,
	[StandardDensity] [decimal](18, 6) NULL,
	[DateLastUpdated] [datetime] NULL,
	[Operator] [varchar](50) NULL,
 CONSTRAINT [PK_mtTankMaster] PRIMARY KEY CLUSTERED 
(
	[Tank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtTankMovementStaging]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtTankMovementStaging](
	[GUID] [uniqueidentifier] NOT NULL,
	[Tank] [varchar](50) NOT NULL,
	[Description] [varchar](100) NULL,
	[Tagname] [varchar](50) NULL,
	[FromDate] [datetime] NULL,
	[ToDate] [datetime] NULL,
	[Receipt] [decimal](18, 6) NULL,
	[Delivery] [decimal](18, 6) NULL,
	[Operator] [varchar](50) NULL,
	[TrnDate] [datetime] NULL,
 CONSTRAINT [PK_mtTankMovementFilter] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC,
	[Tank] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtTankProductHistory]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtTankProductHistory](
	[Tank] [varchar](50) NOT NULL,
	[Product] [varchar](100) NOT NULL,
	[DateUpdated] [datetime] NOT NULL,
	[Operator] [varchar](50) NULL,
 CONSTRAINT [PK_mtTankProductHistory] PRIMARY KEY CLUSTERED 
(
	[Tank] ASC,
	[Product] ASC,
	[DateUpdated] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtUser]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtUser](
	[Username] [varchar](50) NOT NULL,
	[Password] [varchar](50) NULL,
	[SysproUsername] [varchar](50) NULL,
	[SysproPassword] [varchar](50) NULL,
	[DefaultPrinter] [varchar](200) NULL,
	[DefaultApplication] [varchar](200) NULL,
 CONSTRAINT [PK_mtUser_1] PRIMARY KEY CLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtUserCompany]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtUserCompany](
	[Username] [varchar](50) NOT NULL,
	[Company] [varchar](4) NOT NULL,
 CONSTRAINT [PK_mtUserCompany] PRIMARY KEY CLUSTERED 
(
	[Username] ASC,
	[Company] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[mtUserRole]    Script Date: 1/24/2017 2:18:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[mtUserRole](
	[Username] [varchar](50) NOT NULL,
	[Role] [varchar](50) NOT NULL,
 CONSTRAINT [PK_mtUserRole] PRIMARY KEY CLUSTERED 
(
	[Username] ASC,
	[Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[mtSysproAdmin] ADD  CONSTRAINT [DF_mtSysproAdmin_UseRoles]  DEFAULT ((1)) FOR [UseRoles]
GO
