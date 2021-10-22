USE [Adr_Logging]
GO

/****** Object:  UserDefinedFunction [dbo].[f_round5min]    Script Date: 1/25/2017 2:16:19 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



create function [dbo].[f_round5min]
(
@date datetime
) returns datetime
as
begin -- adding 150 seconds to round off instead of truncating
return dateadd(minute, datediff(minute, '1900-01-01', dateadd(second, 150, @date))/5*5, 0)
end
GO

