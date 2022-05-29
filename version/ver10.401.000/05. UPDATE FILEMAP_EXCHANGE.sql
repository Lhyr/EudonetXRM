
IF NOT EXISTS (SELECT 1
	FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name like 'FILEMAP_EXCHANGE' and syscolumns.name like 'orig' )
BEGIN
	ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ADD [orig] VARCHAR(50) NULL;
END


IF NOT EXISTS (SELECT 1
	FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name like 'FILEMAP_EXCHANGE' and syscolumns.name like 'SecondarySourceFileId' )
BEGIN
	ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ADD [SecondarySourceFileId] VARCHAR(500) NULL;
END



IF NOT EXISTS (SELECT 1
	FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name like 'FILEMAP_EXCHANGE' and syscolumns.name like 'SecondaryMasterSourceFileId' )
BEGIN
	ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ADD [SecondaryMasterSourceFileId] VARCHAR(500) NULL;
END


ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ALTER COLUMN [MasterSourceFileId] [varchar] (500) COLLATE DATABASE_DEFAULT NULL
ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ALTER COLUMN [SecondaryMasterSourceFileId] [varchar] (500) COLLATE DATABASE_DEFAULT NULL
ALTER TABLE [dbo].[FILEMAP_EXCHANGE] ALTER COLUMN [Orig] [varchar] (50) COLLATE DATABASE_DEFAULT NULL
 