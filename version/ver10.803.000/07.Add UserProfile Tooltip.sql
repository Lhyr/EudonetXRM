DELETE [RESADV] WHERE [DescId] = 101036

INSERT INTO [RESADV] ([DescId], [ID_LANG], LANG, [TYPE]) SELECT 101036, 0, 'Profil contenant les préférences d''affichage par défaut de l''utilisateur', 2

DECLARE @ID_LANG_COUNT INT
SET @ID_LANG_COUNT = 1
WHILE @ID_LANG_COUNT <= 10
BEGIN
		INSERT INTO [RESADV] ([DescId], [ID_LANG], LANG, [TYPE]) SELECT 101036, @ID_LANG_COUNT, '', 2
		SET @ID_LANG_COUNT = @ID_LANG_COUNT + 1
END