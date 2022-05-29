IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'MAPLANG' and c.name LIKE 'LANG_DEFAULT')
BEGIN
	-- Création de la nouvelle colonne COLSPREF.XrmWidgetId
	ALTER TABLE [MAPLANG] ADD [LANG_DEFAULT] BIT NOT NULL DEFAULT 0

END