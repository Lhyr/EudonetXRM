IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'MAILSTATUS' and c.name = 'MAILSTATUSDATEUPDATED')
	ALTER TABLE [MAILSTATUS] ADD MAILSTATUSDATEUPDATED DATETIME
	
 