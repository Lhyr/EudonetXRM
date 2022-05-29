IF NOT EXISTS (
		SELECT 1
		FROM sys.tables
		WHERE name LIKE 'EDNOPERATION'
		)
BEGIN
	CREATE TABLE dbo.[EDNOPERATION] (
		[OperationId] INT NOT NULL
		,[ProductCode] SMALLINT NOT NULL
		,[Type] [tinyint] NOT NULL
		,[Quantity] INT NOT NULL
		,[Date] [datetime] NOT NULL
		,[CreatedOn] [datetime] NOT NULL
		,[UserId] INT NULL
		,CONSTRAINT UC_OperationId UNIQUE (OperationId)
		)
END
