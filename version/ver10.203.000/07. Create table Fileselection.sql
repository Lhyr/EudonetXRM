IF NOT EXISTS (SELECT 1
		from  sys.tables
		where sys.tables.name = 'FILESELECTION'  )
BEGIN

	CREATE TABLE [dbo].[FILESELECTION](
		[SelectionID] [int] IDENTITY(1,1) PRIMARY KEY,
		[SelectionType] [int] NOT NULL,
		[FileID] [int] NULL,
		[UserID] [int] NULL,
		[TableID] [int] NULL,
		[SourceTableID] [int] NULL
	)


END
