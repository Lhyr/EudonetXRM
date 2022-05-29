USE EUDOLOG

IF EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME = '[DATABASES]'
		WHERE sc.NAME = 'HashDatabase'
		)
BEGIN
	ALTER TABLE [DATABASES] DROP COLUMN [HashDatabase]
END