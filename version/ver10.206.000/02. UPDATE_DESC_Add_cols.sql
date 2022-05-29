IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'DESC' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'NoCascadePPPM' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [DESC] ADD NoCascadePPPM BIT DEFAULT 0	
END



IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'DESC' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'NoCascadePMPP' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [DESC] ADD NoCascadePMPP BIT DEFAULT 0	
END


-- Recherche predective
IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'DESC' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'AutoCompletion' collate french_ci_ai
		)
BEGIN
	ALTER TABLE [DESC] ADD AutoCompletion INT DEFAULT 0	
END



