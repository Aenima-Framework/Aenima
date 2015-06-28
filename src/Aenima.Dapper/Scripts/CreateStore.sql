-- Tables
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Streams]') AND type in (N'U'))
BEGIN
    CREATE TABLE Streams
    (
         InternalId INT				NOT NULL	IDENTITY(1,1),
         Id         NVARCHAR(250)	NOT NULL,
         Type       NVARCHAR(250)	NOT NULL,
         Version	INT             NOT NULL	CONSTRAINT DF_Streams_Version	DEFAULT(0),
         IsDeleted  BIT				NOT NULL	CONSTRAINT DF_Streams_IsDeleted DEFAULT(0),
     
         CONSTRAINT PK_Streams PRIMARY KEY CLUSTERED (InternalId),
    );
    CREATE UNIQUE NONCLUSTERED INDEX IX_Streams_Status ON Streams(Id, Version, IsDeleted);
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Events]') AND type in (N'U'))
BEGIN
    CREATE TABLE Events
    (   
        Id                  UNIQUEIDENTIFIER	NOT NULL,
        Type                NVARCHAR(250)		NOT NULL,
        Data				NVARCHAR(MAX)		NOT NULL,
        Metadata			NVARCHAR(MAX)		NULL,
        CreatedOn	        DATETIME			NOT NULL,
        StreamInternalId    INT					NOT NULL,
        StreamVersion		INT					NOT NULL,
    
        CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Events_Streams FOREIGN KEY (StreamInternalId) REFERENCES Streams (InternalId)
    );
    CREATE NONCLUSTERED INDEX IX_Events_StreamInternalId_StreamVersion ON Events(StreamInternalId, StreamVersion);
END
GO
-- Types
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'StreamEvents' AND ss.name = N'dbo')
CREATE TYPE StreamEvents AS TABLE 
(
    Id                  UNIQUEIDENTIFIER	NOT NULL,
    Type                NVARCHAR(250)		NOT NULL,
    Data				NVARCHAR(MAX)		NOT NULL,
    Metadata			NVARCHAR(MAX)		NOT NULL,
    CreatedOn	        DATETIME			NOT NULL    DEFAULT(SYSUTCDATETIME()),
    StreamInternalId	INT         		NOT NULL,
    StreamVersion		INT					NOT NULL
);
GO
-- Stored Procedures
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppendStream]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[AppendStream]
    @StreamId              NVARCHAR(250),
    @StreamType            NVARCHAR(250),
    @ExpectedStreamVersion INT,
    @StreamEvents          STREAMEVENTS READONLY,
    @Result                INT = 1 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @StreamNotFoundResult       INT = -100,
        @StreamIsDeletedResult      INT = -200,
        @InvalidStreamVersionResult INT = -300,
        @StreamAlreadyExistsResult  INT = -400
        
    DECLARE
        @StreamInternalId INT = 0,
        @StreamVersion    INT = -1,
        @StreamIsDeleted  BIT = 1,
        @NewStreamVersion INT = -1

    -- get the provided new stream version
    SELECT @NewStreamVersion = MAX(StreamVersion)
      FROM @StreamEvents

    IF @ExpectedStreamVersion = -2 BEGIN
        -- upsert stream
        UPDATE Streams
           SET Version = @NewStreamVersion,
               IsDeleted = 0
         WHERE Id = @StreamId

        IF @@ROWCOUNT = 0 BEGIN
            INSERT INTO Streams(Id,Type,Version)
            SELECT @StreamId,
                   @StreamType,
                   @NewStreamVersion

            -- get new stream id
            SELECT @StreamInternalId = SCOPE_IDENTITY()
        END
        ELSE BEGIN
            SELECT @StreamInternalId = Streams.InternalId
              FROM Streams
             WHERE Streams.Id = @StreamId
        END
    END
    ELSE BEGIN
        -- get internal id and stream version
        SELECT @StreamInternalId = Streams.InternalId,
               @StreamVersion = Streams.Version,
               @StreamIsDeleted = Streams.IsDeleted
          FROM Streams
         WHERE Streams.Id = @StreamId

        -- should not exist
        IF @ExpectedStreamVersion = -1 AND @StreamInternalId > 0 BEGIN
            SET @Result = @StreamAlreadyExistsResult RETURN
        END

        -- if not exists
        IF @StreamInternalId = 0 BEGIN
            SET @Result = @StreamNotFoundResult RETURN
        END

        -- if deleted
        IF @StreamIsDeleted = 1 BEGIN
            SET @Result = @StreamIsDeletedResult RETURN
        END

        -- if not expected version
        IF @StreamVersion <> @ExpectedStreamVersion BEGIN
            SET @Result = @InvalidStreamVersionResult RETURN
        END

        -- update stream version
        UPDATE Streams
           SET Version = @NewStreamVersion
         WHERE Id = @StreamId
    END

    -- insert events
    INSERT INTO Events(StreamInternalId,StreamVersion,Id,Type,Data,Metadata,CreatedOn)
    SELECT @StreamInternalId,
           StreamVersion,
           Id,
           Type,
           Data,
           Metadata,
           CreatedOn
      FROM @StreamEvents
     ORDER BY StreamVersion
END ' 
END
GO
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadStream]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ReadStream]
    @StreamId    NVARCHAR(250),
    @FromVersion INT,
    @Count       INT,
    @ReadForward BIT = 1,
    @Result      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE
        @StreamNotFoundResult  INT = -100,
        @StreamIsDeletedResult INT = -200
        
    DECLARE
        @StreamInternalId INT = 0,
        @StreamVersion    INT = -1,
        @StreamIsDeleted  INT = 1

    -- get stream internal id and version
    SELECT @StreamInternalId = Streams.InternalId,
           @StreamVersion = Streams.Version,
           @StreamIsDeleted = Streams.IsDeleted
      FROM Streams
     WHERE Streams.Id = @StreamId

    -- exit if stream does not exist
    IF @StreamInternalId = 0 BEGIN
        SET @Result = @StreamNotFoundResult RETURN
    END

    -- if deleted
    IF @StreamIsDeleted = 1 BEGIN
        SET @Result = @StreamIsDeletedResult RETURN
    END
    
    -- otherwise return stream version
    SET @Result = @StreamVersion RETURN

    -- get events
    IF @ReadForward = 1 BEGIN
        SELECT TOP(@Count) Id,
                           Type,
                           Data,
                           Metadata,
                           CreatedOn,
                           @StreamId,
                           StreamVersion
          FROM Events
         WHERE StreamInternalId = @StreamInternalId
           AND StreamVersion >= @FromVersion
         ORDER BY StreamVersion ASC
    END
    ELSE BEGIN
        SELECT TOP(@Count) Id,
                           Type,
                           Data,
                           Metadata,
                           CreatedOn,
                           @StreamId,
                           StreamVersion
          FROM Events
         WHERE StreamInternalId = @StreamInternalId
           AND StreamVersion <= @FromVersion
         ORDER BY StreamVersion DESC
    END
END '
END
GO