--USE [RampPro]
--GO
/****** Object:  StoredProcedure [dbo].[ELMAH_LogError]    Script Date: 3/27/2015 7:33:40 PM ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ELMAH_LogError')
   exec('DROP PROCEDURE [dbo].[ELMAH_LogError]')
GO
CREATE PROCEDURE [dbo].[ELMAH_LogError] 
(
    @ErrorId UNIQUEIDENTIFIER,
    @Application NVARCHAR(60),
    @Host NVARCHAR(30),
    @Type NVARCHAR(100),
    @Source NVARCHAR(60),
    @Message NVARCHAR(500),
    @User NVARCHAR(50),
    @AllXml NTEXT,
    @StatusCode INT,
    @TimeUtc DATETIME
)
AS
    SET NOCOUNT ON

    INSERT
    INTO
        [ELMAH_Error]
        (
            [ErrorId],
            [Application],
            [Host],
            [Type],
            [Source],
            [Message],
            [User],
            [AllXml],
            [StatusCode],
            [TimeUtc]
        )
    VALUES
        (
            @ErrorId,
            @Application,
            @Host,
            @Type,
            @Source,
            @Message,
            @User,
            @AllXml,
            @StatusCode,
            @TimeUtc
        )

