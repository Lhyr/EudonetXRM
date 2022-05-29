IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'TOKEN' and c.name = 'APPNAME')
	ALTER TABLE [TOKEN] ADD APPNAME NVARCHAR(50)
	
IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'TOKEN' and c.name = 'FORUSERID')
	ALTER TABLE [TOKEN] ADD [FORUSERID] int