-- Stored Procedures
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppendStream]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[AppendStream]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ReadStream]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ReadStream]
GO
-- Types
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'StreamEvents' AND ss.name = N'dbo')
DROP TYPE [dbo].[StreamEvents]
GO
-- Tables
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Events]') AND type in (N'U'))
DROP TABLE [dbo].[Events]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Streams]') AND type in (N'U'))
DROP TABLE [dbo].[Streams]
GO




