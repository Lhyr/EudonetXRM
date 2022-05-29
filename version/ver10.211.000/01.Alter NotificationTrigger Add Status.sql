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
			WHERE sc.NAME collate french_ci_ai = 'Status' collate french_ci_ai
			)
	BEGIN
		ALTER TABLE [NotificationTrigger] ADD Status BIT DEFAULT NULL	
	END

	-- Existance dans DESC
	IF NOT EXISTS (
			SELECT 1
			FROM [DESC]
			WHERE [DescId] = @nTab + 25
			)
	BEGIN
		insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select  @nTab + 25, 'NotificationTrigger', 'Status', 3, 0
		update [DESC] set [DispOrder] = 25, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 25
	END
		
	-- Existance dans RES
	IF NOT EXISTS (
			SELECT 1
			FROM [RES]
			WHERE [ResId] = @nTab + 25
			)
	BEGIN
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select  @nTab + 25, 'Statut', 'Status', 'Status', 'Status', 'Status'			
	END
END