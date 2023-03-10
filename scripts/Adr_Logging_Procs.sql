USE [Adr_Logging]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTankLevels]    Script Date: 1/25/2017 2:11:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_GetTankLevels]
(
	@Guid UNIQUEIDENTIFIER
	,@Tagname varchar(50)
	,@FromDate DATETIME
	,@ToDate DATETIME	
)
AS
	--DECLARE @Guid UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000'
	--,@Tagname varchar(50) = '01lt019.value'
	--,@FromDate DATETIME = '2016-12-05 13:30:00.000'
	--,@ToDate DATETIME = '2017-12-05 20:30:00.000'

	IF(@Guid =CAST(0x0 AS UNIQUEIDENTIFIER))
	BEGIN
	SET @Guid = NEWID()
	END

	DECLARE @Temperature DECIMAL(18,6) = '45'
	DECLARE @strMessage VARCHAR(200) = ''
	DECLARE @StartValue DECIMAL(18,6) = (SELECT	Value AS StartValue	FROM Adr_DBLog_Analogs WHERE Tagname = @Tagname AND dt = dbo.f_round5min(@FromDate))

	IF(@StartValue IS NULL)
	BEGIN
		SET @strMessage = 'No value found for Start Date-Time. '
	END

	DECLARE @EndValue DECIMAL(18,6) = (SELECT Value AS EndValue	FROM Adr_DBLog_Analogs WHERE Tagname = @Tagname AND dt = dbo.f_round5min(@ToDate))

	IF(@EndValue IS NULL)
	BEGIN
		SET @strMessage += 'No value found for End Date-Time. '
	END

	SELECT @Guid AS GUID, @Tagname AS Tagname, dbo.f_round5min(@FromDate) AS FromDate, dbo.f_round5min(@ToDate) AS ToDate,
	ISNULL(@StartValue,0) AS StartValue, ISNULL(@EndValue,0) AS EndValue, ISNULL(@Temperature,0) AS Temperature, GETDATE() AS TrnDate, @strMessage AS strMessage
GO
/****** Object:  StoredProcedure [dbo].[sp_GetTankMovements]    Script Date: 1/25/2017 2:11:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_GetTankMovements]
(@EntryGuid UNIQUEIDENTIFIER,
 @Tagname VARCHAR(50),
 @FromDate DATETIME,
 @ToDate DATETIME)
 AS
WITH vwData AS
(
SELECT Value, dt,
ROW_NUMBER() OVER (ORDER BY dt ) AS RowNum
FROM Adr_DBLog_Analogs WITH(NOLOCK)
WHERE Tagname = @Tagname
and dt BETWEEN @FromDate AND @ToDate
--order by dt DESC
),
vwDetail AS
(
SELECT 
CASE WHEN A.Value < B.Value THEN B.Value - A.Value ELSE 0 END AS INCREASE,
CASE WHEN A.Value > B.Value THEN A.Value - B.Value ELSE 0 END AS DECREASE
FROM vwData A WITH(NOLOCK)
INNER JOIN vwData B WITH(NOLOCK)
ON A.RowNum = B.RowNum - 1
WHERE B.Value - A.Value > 0.5 OR A.Value - B.Value > 0.5
)
SELECT @Tagname AS Tagname, @FromDate AS FromDate, @ToDate AS ToDate, SUM(ISNULL(INCREASE,0)) * 1000 AS Receipt, SUM(ISNULL(DECREASE,0)) * 1000 AS Delivery FROM vwDetail


GO
