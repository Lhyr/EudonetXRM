
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'EXTENSION' and syscolumns.name like 'EXTENSIONINSTANCE' )
BEGIN
	ALTER TABLE [EXTENSION] ADD EXTENSIONINSTANCE VARCHAR(1024)
END

INSERT INTO [DESC](DescId, [File], Field, Format, Length)
SELECT 101108, 'EXTENSION', 'EXTENSIONINSTANCE', 1, 1024
WHERE NOT EXISTS (
	SELECT DescId FROM [DESC] WHERE DescId = 101108
)

INSERT INTO [RES](ResId, LANG_00)
SELECT 101108, 'EXTENSIONINSTANCE'
WHERE NOT EXISTS (
	SELECT ResId FROM [RES] WHERE ResId = 101108
)


if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'EXTENSION' and syscolumns.name like 'EXTENSIONVERSION' )
BEGIN
	ALTER TABLE [EXTENSION] ADD EXTENSIONVERSION VARCHAR(255)
END

INSERT INTO [DESC](DescId, [File], Field, Format, Length)
SELECT 101109, 'EXTENSION', 'EXTENSIONVERSION', 1, 255
WHERE NOT EXISTS (
	SELECT DescId FROM [DESC] WHERE DescId = 101109
)

INSERT INTO [RES](ResId, LANG_00)
SELECT 101109, 'EXTENSIONVERSION'
WHERE NOT EXISTS (
	SELECT ResId FROM [RES] WHERE ResId = 101109
)
