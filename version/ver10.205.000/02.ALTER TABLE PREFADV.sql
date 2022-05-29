IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'PREFADV' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'Tab' collate french_ci_ai
		)
BEGIN
	ALTER TABLE PREFADV ADD Tab NUMERIC NOT NULL DEFAULT '0'
	
	DROP INDEX [IX_PREFADV] ON [dbo].[PREFADV]
	
	CREATE UNIQUE NONCLUSTERED INDEX [IX_PREFADV] ON [dbo].[PREFADV]
	(
		[Parameter] ASC,
		[UserId] ASC,
		[Tab] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END