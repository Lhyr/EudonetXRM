IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'SPECIFS' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'ADMINURL' collate french_ci_ai
		)
BEGIN
	ALTER TABLE SPECIFS ADD ADMINURL varchar(1000)
END


 IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'SPECIFS' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'DATECREA' collate french_ci_ai
		)
BEGIN
	ALTER TABLE SPECIFS ADD DATECREA datetime not null DEFAULT getdate()
END


 
 IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'SPECIFS' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'DATEUPD' collate french_ci_ai
		)
BEGIN
	ALTER TABLE SPECIFS ADD DATEUPD datetime   null  
END


 