-- Cr�ation des nouvelles colonnes dans DESC
IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'HOMEPAGEADVANCED' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'Tooltip' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [HOMEPAGEADVANCED] ADD Tooltip NVARCHAR(MAX)
END
