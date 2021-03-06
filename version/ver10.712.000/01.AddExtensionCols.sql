if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'EXTENSION' and syscolumns.name like 'EXTENSIONHASCUSTOMPARAM' )
BEGIN
	ALTER TABLE [EXTENSION] ADD EXTENSIONHASCUSTOMPARAM bit default 0
	exec('update EXTENSION set EXTENSIONHASCUSTOMPARAM = 0')
END

INSERT INTO [DESC](DescId, [File], Field, Format, Length)
SELECT 101112, 'EXTENSION', 'EXTENSIONHASCUSTOMPARAM', 3, 0
WHERE NOT EXISTS (
	SELECT DescId FROM [DESC] WHERE DescId = 101112
)

INSERT INTO [RES](ResId, LANG_00)
SELECT 101112, 'EXTENSIONHASCUSTOMPARAM'
WHERE NOT EXISTS (
	SELECT ResId FROM [RES] WHERE ResId = 101112
)
