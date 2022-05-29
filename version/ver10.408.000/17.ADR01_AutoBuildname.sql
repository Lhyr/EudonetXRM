	IF NOT EXISTS (
			SELECT 1
			FROM sys.tables
			INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
			WHERE sys.tables.NAME LIKE 'ADDRESS'
				AND syscolumns.NAME LIKE 'ADR01_AUTOBUILD'
			)
	BEGIN
		ALTER TABLE [ADDRESS] ADD [ADR01_AUTOBUILD] [BIT] NULL 
	END
	
	
	UPDATE [DESC] SET AutoBuildName = '$201$ - $301$' WHERE DescId = 400