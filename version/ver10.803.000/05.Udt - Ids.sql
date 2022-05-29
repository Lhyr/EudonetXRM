IF TYPE_ID(N'udt_EdnIds') IS NULL
BEGIN
	CREATE TYPE [dbo].[udt_EdnIds] AS TABLE([Id] [int] NULL)
END



IF TYPE_ID(N'udt_EdnIds2') IS NULL
BEGIN
	CREATE TYPE [dbo].[udt_EdnIds2] AS TABLE(
	[Id1] [int] NULL,
	[Id2] [int] NULL
	
	)
END


