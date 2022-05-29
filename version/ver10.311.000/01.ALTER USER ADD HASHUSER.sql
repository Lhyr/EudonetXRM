IF NOT EXISTS (SELECT 1
	FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name like 'USER' and syscolumns.name like 'HashUser' )
BEGIN
	ALTER TABLE [dbo].[USER] ADD [HashUser] VARCHAR(MAX) NULL;
END 