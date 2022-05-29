if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'SPECIFS' and syscolumns.name like 'IsStatic' )
BEGIN
	ALTER TABLE [SPECIFS] ADD [IsStatic] bit default (0)
END