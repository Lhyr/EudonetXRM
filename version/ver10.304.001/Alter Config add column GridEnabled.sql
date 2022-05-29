if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'CONFIG' and syscolumns.name like 'GridEnabled' )
BEGIN
	ALTER TABLE [CONFIG] ADD GridEnabled BIT NULL
END