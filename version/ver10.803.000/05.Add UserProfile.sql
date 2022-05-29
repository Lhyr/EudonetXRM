IF NOT EXISTS (SELECT 1
	FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name = 'USER' AND syscolumns.name = 'USER_PROFILE')
BEGIN
	ALTER TABLE [USER] ADD USER_PROFILE NUMERIC(18,0)
END

DELETE [res] WHERE resid = 101036
DELETE [desc] WHERE descid = 101036
INSERT INTO [desc] ([DescId], [File], [Field], [Format], [Length]) SELECT 101036, 'USER', 'USER_PROFILE', 8, 0
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT 101036, 'Profil affecté', 'Assigned profile', '', '', ''