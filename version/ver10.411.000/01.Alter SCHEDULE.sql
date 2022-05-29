--Ajout colonne Time
IF NOT EXISTS (
		SELECT 1
		FROM sys.tables
		INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name LIKE 'SCHEDULE'
			AND syscolumns.name LIKE 'Time'
		)
BEGIN
	ALTER TABLE [SCHEDULE] ADD [Time] TIME NULL
END
