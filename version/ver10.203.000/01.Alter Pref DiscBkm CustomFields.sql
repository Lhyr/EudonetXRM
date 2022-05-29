IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'PREF' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'DiscCustomFields' collate french_ci_ai
		)
BEGIN
	ALTER TABLE PREF ADD DiscCustomFields VARCHAR(128)
END