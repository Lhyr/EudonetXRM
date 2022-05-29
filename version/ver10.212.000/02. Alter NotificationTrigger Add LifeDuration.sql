set nocount on;
-- Table NotificationTrigger
DECLARE @nTab INT = 114200 
begin
	-- Ajout de colonne Status
	IF NOT EXISTS (
			SELECT 1
			FROM sys.columns sc
			INNER JOIN sys.tables st ON st.object_id = sc.object_id
				AND st.NAME collate french_ci_ai = 'NotificationTrigger' collate french_ci_ai
			WHERE sc.NAME collate french_ci_ai = 'LifeDuration' collate french_ci_ai
			)
	BEGIN
		ALTER TABLE [NotificationTrigger] ADD LifeDuration INT DEFAULT NULL	
	END

	-- Existance dans DESC
	IF NOT EXISTS (
			SELECT 1
			FROM [DESC]
			WHERE [DescId] = @nTab + 26
			)
	BEGIN
		insert into [desc] ([DescId], [File], [Field], [Format], [Length], [TooltipText]) select  @nTab + 26, 'NotificationTrigger', 'LifeDuration', 10, 0, 'Durée de vie en jours'
		update [DESC] set [DispOrder] = 26, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 26
	END
		
	-- Existance dans RES
	IF NOT EXISTS (
			SELECT 1
			FROM [RES]
			WHERE [ResId] = @nTab + 26
			)
	BEGIN
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select  @nTab + 26, 'Suppression', 'Delete', 'Delete', 'Delete', 'Delete'			
	END
END