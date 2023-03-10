--USE [RampPro]
--GO
/****** Object:  StoredProcedure [dbo].[ELMAH_GetErrorXml]    Script Date: 3/27/2015 7:33:06 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ELMAH_GetErrorXml')
   exec('DROP PROCEDURE [dbo].[ELMAH_GetErrorXml]')
GO
CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml] 
(
    @Application NVARCHAR(60),
    @ErrorId UNIQUEIDENTIFIER
)
AS
 
    SET NOCOUNT ON

    SELECT 
        [AllXml]
    FROM 
        [ELMAH_Error]
    WHERE
        [ErrorId] = @ErrorId
    AND
        [Application] = @Application

