

-- AlertEnabled
IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'CONFIG' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'AlertEnabled' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [CONFIG] ADD AlertEnabled BIT DEFAULT NULL	
END
