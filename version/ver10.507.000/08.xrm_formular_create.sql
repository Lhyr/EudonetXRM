if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'FORMULARXRM' and syscolumns.name like 'TYPE' )
BEGIN
	ALTER TABLE [FORMULARXRM] ADD [TYPE] smallint default (0)
END

