IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'EDNCOUNTRIES' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'CurrencyCode' collate french_ci_ai
		)
BEGIN
	ALTER TABLE EDNCOUNTRIES ADD CurrencyCode CHAR(3) NOT NULL DEFAULT ''
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'EDNCOUNTRIES' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'CurrencySymbol' collate french_ci_ai
		)
BEGIN
	ALTER TABLE EDNCOUNTRIES ADD CurrencySymbol NVARCHAR(20) NOT NULL DEFAULT ''
END