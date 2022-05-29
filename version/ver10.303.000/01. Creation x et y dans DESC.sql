-- Création des nouvelles colonnes dans DESC
IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'DESC' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'x' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [DESC] ADD x int
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'DESC' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'y' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [DESC] ADD y int
END
