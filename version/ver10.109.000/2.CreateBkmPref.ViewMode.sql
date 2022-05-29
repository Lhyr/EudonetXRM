IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'BKMPREF' and c.name = 'ViewMode')
BEGIN
	ALTER TABLE BKMPREF ADD ViewMode NUMERIC
END