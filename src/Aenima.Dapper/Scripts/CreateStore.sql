------------------------------------------------------------------------------
-- Tables
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Streams]') AND type in (N'U'))
BEGIN
    CREATE TABLE Streams
    (
         InternalId INT				NOT NULL	IDENTITY(1,1),
         Id         NVARCHAR(250)	NOT NULL,
         Version	INT             NOT NULL	CONSTRAINT DF_Streams_Version	DEFAULT(0),
         IsDeleted  BIT				NOT NULL	CONSTRAINT DF_Streams_IsDeleted DEFAULT(0),
     
         CONSTRAINT PK_Streams PRIMARY KEY CLUSTERED (InternalId),
    );
    CREATE UNIQUE NONCLUSTERED INDEX IX_Streams_Status ON Streams(Id, Version, IsDeleted);
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Events]') AND type in (N'U'))
BEGIN
    CREATE TABLE Events
    (   
        Id                  UNIQUEIDENTIFIER	NOT NULL,
        Type                NVARCHAR(250)		NOT NULL,
        Data				NVARCHAR(MAX)		NOT NULL,
        Metadata			NVARCHAR(MAX)		NULL,
        CreatedOn 			datetime2(7) 		NOT NULL DEFAULT (sysutcdatetime()),
        StreamInternalId    INT					NOT NULL,
        StreamVersion		INT					NOT NULL,
    
        CONSTRAINT PK_Events PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Events_Streams FOREIGN KEY (StreamInternalId) REFERENCES Streams (InternalId)
    );
    CREATE NONCLUSTERED INDEX IX_Events_StreamInternalId_StreamVersion ON Events(StreamInternalId, StreamVersion);
END

------------------------------------------------------------------------------
-- Types
------------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'StreamEvents' AND ss.name = N'dbo')
CREATE TYPE StreamEvents AS TABLE 
(
    Id                  UNIQUEIDENTIFIER	NOT NULL,
    Type                NVARCHAR(250)		NOT NULL,
    Data				NVARCHAR(MAX)		NOT NULL,
    Metadata			NVARCHAR(MAX)		NOT NULL,
    StreamVersion		INT					NOT NULL
);

------------------------------------------------------------------------------
-- Stored Procedures
------------------------------------------------------------------------------
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppendStream]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[AppendStream]
  @StreamId              NVARCHAR(250),
    @ExpectedStreamVersion INT,
    @StreamEvents          STREAMEVENTS READONLY
AS
BEGIN

    SET NOCOUNT ON;

    DECLARE
        @StreamInternalId INT = 0,
		@StreamVersion INT = -1

	-- get internal id and stream version
	SELECT @StreamInternalId = Streams.InternalId,
		   @StreamVersion = Streams.Version
	  FROM Streams
	 WHERE Streams.Id = @StreamId

	-- exit since there''s a concurrency exception
	IF @StreamVersion <> @ExpectedStreamVersion BEGIN
		SELECT @StreamVersion RETURN
	END

	-- get new stream version
	SELECT @StreamVersion = MAX(StreamVersion)
	  FROM @StreamEvents

	-- stream exists
	IF @StreamInternalId > 0 BEGIN

		-- update version and undelete
		UPDATE Streams
		   SET Version   = @StreamVersion,
			   IsDeleted = 0
		 WHERE Id = @StreamId

	END
	-- stream does not exist
	ELSE BEGIN

		-- create stream
		INSERT INTO Streams(Id,Version)
		SELECT @StreamId,
			   @StreamVersion

		-- get new internal stream id
		SELECT @StreamInternalId = SCOPE_IDENTITY()

	END

    -- append events
    INSERT INTO Events(StreamInternalId,StreamVersion,Id,Type,Data,Metadata)
    SELECT @StreamInternalId,
           StreamVersion,
           Id,
           Type,
           Data,
           Metadata
      FROM @StreamEvents
     ORDER BY StreamVersion

	 SELECT @StreamVersion
END ' 
END

IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadStream]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ReadStream]
   @StreamId    NVARCHAR(250),
    @FromVersion INT,
    @Count       INT,
    @ReadForward BIT = 1,
	@Error	     INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	SET @Error = 0

    DECLARE
        @StreamNotFoundError INT = -100,
        @StreamDeletedError  INT = -200
        
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
		SET @Error = @StreamNotFoundError; 
		SELECT TOP(0) Data, Metadata FROM Events -- because of Dapper.QueryAsync bug
		RETURN
    END

    -- if deleted
    IF @StreamIsDeleted = 1 BEGIN
		SET @Error = @StreamDeletedError; 
		SELECT TOP(0) Data, Metadata FROM Events -- because of Dapper.QueryAsync bug
		RETURN 
    END
    
    -- get events
    IF @ReadForward = 1 BEGIN
        SELECT TOP(@Count) Data,
                           Metadata
          FROM Events
         WHERE StreamInternalId = @StreamInternalId
           AND StreamVersion >= @FromVersion
         ORDER BY StreamVersion ASC
    END
    ELSE BEGIN
        SELECT TOP(@Count) Data,
                           Metadata
          FROM Events
         WHERE StreamInternalId = @StreamInternalId
           AND StreamVersion <= @FromVersion
         ORDER BY StreamVersion DESC
    END

	RETURN @StreamVersion
END'
END
