IF EXISTS(select * from [RES] where [ResId] in (101032, 101036))

BEGIN
	UPDATE [RES] SET [LANG_00] = 'Cet utilisateur est un profil', [LANG_01] = '', [LANG_02] = '', [LANG_04] = '', [LANG_05] = '' WHERE [ResId] = 101032
	UPDATE [RES] SET [LANG_00] = 'Reprendre le profil de', [LANG_01] = '' WHERE [ResId] = 101036
END