IF( NOT EXISTS ( SELECT 1  FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SPECIFS' AND COLUMN_NAME = 'DISPORDER' ))
BEGIN
	ALTER TABLE [SPECIFS]
	ADD DispOrder INT NULL
END