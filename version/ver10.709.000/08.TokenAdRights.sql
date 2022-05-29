IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'TOKEN' and c.name = 'RIGHTS')
	ALTER TABLE [TOKEN] ADD RIGHTS NVARCHAR(50)
	
 