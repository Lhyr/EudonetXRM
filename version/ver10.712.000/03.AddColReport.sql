IF NOT EXISTS (SELECT 1
	FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name = 'REPORT' AND syscolumns.name = 'IsScheduled')
BEGIN
	EXEC('ALTER TABLE [REPORT] ADD IsScheduled BIT DEFAULT 0');
	DELETE [res] WHERE resid = 115014
	DELETE [desc] WHERE descid = 115014
	INSERT INTO [desc] ([DescId], [File], [Field], [Format], [Length]) SELECT 105014, 'REPORT', 'IsScheduled', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT 105014, 'Envoi planifié', 'Scheduled sending', '', '', ''
	
	EXEC('UPDATE REPORT SET IsScheduled = CASE WHEN ISNULL(SCHEDULEPARAM,'''') <> '''' THEN 1 ELSE 0 END');
	
	EXEC('UPDATE SELECTIONS SET ListCol = ''105008;105004;105014'' WHERE Tab = 105000');
END