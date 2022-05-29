declare @strExecTemplate as varchar(max) =
	'IF NOT EXISTS (SELECT 1
				FROM sys.tables INNER JOIN syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name LIKE ''<TABNAME>'' AND syscolumns.name LIKE ''<SHORTFIELDNAME><SHORTFIELDDESCID>'')
	BEGIN
		ALTER TABLE [<TABNAME>] ADD [<SHORTFIELDNAME><SHORTFIELDDESCID>] <FIELDFORMAT>
	END'

--Curseur Tables EventStep
declare @tabId as numeric(18)

declare eventStep_cursor cursor for 
select distinct [DescId]
from [DESCADV] 
where [Parameter] = 42
and ltrim(rtrim([Value])) = '1'
and [DescId] % 100 = 0

open eventStep_cursor

fetch next from eventStep_cursor 
into @tabId

while @@FETCH_STATUS = 0
begin
	--traitement
	
	--Colonne SQL
	declare @tabName as varchar(max)
	declare @shortFieldName as varchar(max)
	declare @strExec as varchar(max)	
	set @tabName = null	
	set @shortFieldName = null
	
	select @tabName = [File], @shortFieldName = [Field] from [DESC] where [DescId] = @tabId	
	
	--EVT25 - Annuler Étape
	set @strExec = @strExecTemplate
	set @strExec = REPLACE(@strExec, '<TABNAME>', @tabName)
	set @strExec = REPLACE(@strExec, '<SHORTFIELDNAME>', @shortFieldName)
	set @strExec = REPLACE(@strExec, '<SHORTFIELDDESCID>', '25')
	set @strExec = REPLACE(@strExec, '<FIELDFORMAT>', 'BIT NULL')
	
	EXEC (@strExec)
	
	--EVT26 - Désactivé
	set @strExec = @strExecTemplate
	set @strExec = REPLACE(@strExec, '<TABNAME>', @tabName)
	set @strExec = REPLACE(@strExec, '<SHORTFIELDNAME>', @shortFieldName)
	set @strExec = REPLACE(@strExec, '<SHORTFIELDDESCID>', '26')
	set @strExec = REPLACE(@strExec, '<FIELDFORMAT>', 'BIT NULL')
	
	EXEC (@strExec)
	
	
	declare @fieldIdCancel as numeric(18)
	set @fieldIdCancel = @tabId + 25
	
	--DESC
	--EVT25 - Annuler Étape
	IF NOT EXISTS (select 1 from [DESC] where [DescId] = @fieldIdCancel)
	BEGIN
		INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
		VALUES (@fieldIdCancel, @tabName, @shortFieldName + '25', 25,  0)
	END
	
	declare @fieldIdDisabled as numeric(18)
	set @fieldIdDisabled = @tabId + 26
	
	--EVT26 - Désactivée
	IF NOT EXISTS (select 1 from [DESC] where [DescId] = @fieldIdDisabled)
	BEGIN
		INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
		VALUES (@fieldIdDisabled, @tabName, @shortFieldName + '26', 3,  0)
	END
	
	UPDATE [DESC] SET
		[Multiple] = 0
		,[Popup] = 0
		,[PopupDescId] = 0
		,[BoundDescId] = 0
		,[Case] = 0
		,[Historic] = 0
		,[Obligat] = 0
		,[ActiveTab] = 0
		,[ActiveBkmPP] = 1
		,[ActiveBkmPM] = 1
		,[ActiveBkmEvent] = 1
		,[GetLevel] = 1
		,[ViewLevel] = 1
		,[UserLevel] = 0
		,[InterPP] = 0
		,[InterPM] = 0
		,[InterEvent] = 0
		,[TabIndex] = 0
		,[Bold] = 0
		,[Italic] = 0
		,[Underline] = 0
		,[Flat] = 0
		,[Disabled] = 0
		,[Unicode] = 0
		,[NbrBkmInline] = 0
		,[TreatmentMaxRows] = 0
		,[TreeViewUserList] = 0
		,[FullUserList] = 0
		,[BreakLine] = 0
		,[NoCascadePPPM] = 0
		,[NoCascadePMPP] = 0
		,[AutoCompletion] = 0
		WHERE [DescId] IN (@fieldIdCancel, @fieldIdDisabled)
	
	
	--RES
	--EVT25 - Annuler Étape
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldIdCancel)
	AND NOT EXISTS (select 1 from [RES] where [ResId] = @fieldIdCancel)
	BEGIN
		INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
		VALUES (@fieldIdCancel, 'Annuler Étape', '', '', '', '', '')
	END
	
	--EVT26 - Désactivée
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldIdDisabled)
	AND NOT EXISTS (select 1 from [RES] where [ResId] = @fieldIdDisabled)
	BEGIN
		INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
		VALUES (@fieldIdDisabled, 'Désactivée', '', '', '', '', '')
	END
	
	--Mise à jour de la mise en page
	/*
	select [res].[LANG_00], [descid], [descid] % 100, [DispOrder], [Colspan], [RowSpan], [x], [y],
	'UPDATE [DESC] SET [DispOrder] = ' + cast([DispOrder] as varchar) + ', [Colspan] = ' + cast([Colspan] as varchar) + ', [RowSpan] = ' + cast([RowSpan] as varchar) + ', [x] = ' + cast([x] as varchar) + ', [y] = ' + cast([y] as varchar) + ' WHERE [DescId] = @tabId + ' + cast([descid] % 100 as varchar) + ' --' + isnull([res].[LANG_00], '')
	from [desc]
	left join [res] on [res].[resid] = [desc].[descid]
	where [descid] between xx00 and xx69
	order by [DispOrder] asc
	*/
	UPDATE [DESC] SET [DispOrder] = 1, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 0 WHERE [DescId] = @tabId + 1 --Nom de l'étape
	UPDATE [DESC] SET [DispOrder] = 3, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 1 WHERE [DescId] = @tabId + 2 --Type
	UPDATE [DESC] SET [DispOrder] = 4, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 1 WHERE [DescId] = @tabId + 6 --Mode d'exécution
	UPDATE [DESC] SET [DispOrder] = 5, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 2 WHERE [DescId] = @tabId + 20 --Campagnes Mail
	UPDATE [DESC] SET [DispOrder] = 6, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 2 WHERE [DescId] = @tabId + 24 --Statut
	UPDATE [DESC] SET [DispOrder] = 7, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 3 WHERE [DescId] = @tabId + 22 --Étape parente
	UPDATE [DESC] SET [DispOrder] = 8, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 3 WHERE [DescId] = @tabId + 23 --Historisée
	UPDATE [DESC] SET [DispOrder] = 9, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 4 WHERE [DescId] = @tabId + 25 --Annuler
	UPDATE [DESC] SET [DispOrder] = 10, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 4 WHERE [DescId] = @tabId + 26 --Désactivée
	UPDATE [DESC] SET [DispOrder] = 13, [Colspan] = 2, [RowSpan] = 1, [x] = 0, [y] = 6 WHERE [DescId] = @tabId + 8 --Détail de la source ajoutée
	UPDATE [DESC] SET [DispOrder] = 14, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 7 WHERE [DescId] = @tabId + 9 --Filtre avancé
	UPDATE [DESC] SET [DispOrder] = 16, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 8 WHERE [DescId] = @tabId + 10 --Modèle d'import
	UPDATE [DESC] SET [DispOrder] = 17, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 8 WHERE [DescId] = @tabId + 11 --Nom du fichier importé
	UPDATE [DESC] SET [DispOrder] = 18, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 9 WHERE [DescId] = @tabId + 12 --Formulaire
	UPDATE [DESC] SET [DispOrder] = 19, [Colspan] = 1, [RowSpan] = 1, [x] = 1, [y] = 9 WHERE [DescId] = @tabId + 13 --Lien vers le formulaire
	UPDATE [DESC] SET [DispOrder] = 22, [Colspan] = 2, [RowSpan] = 1, [x] = 0, [y] = 11 WHERE [DescId] = @tabId + 14 --Détail de l'envoi
	UPDATE [DESC] SET [DispOrder] = 23, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 12 WHERE [DescId] = @tabId + 16 --Filtre sur les destinataires
	UPDATE [DESC] SET [DispOrder] = 27, [Colspan] = 2, [RowSpan] = 1, [x] = 0, [y] = 14 WHERE [DescId] = @tabId + 15 --Planification/programmation
	UPDATE [DESC] SET [DispOrder] = 28, [Colspan] = 2, [RowSpan] = 1, [x] = 0, [y] = 15 WHERE [DescId] = @tabId + 3 --Fréquence
	UPDATE [DESC] SET [DispOrder] = 29, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 16 WHERE [DescId] = @tabId + 4 --Date de l'étape
	UPDATE [DESC] SET [DispOrder] = 33, [Colspan] = 2, [RowSpan] = 1, [x] = 0, [y] = 18 WHERE [DescId] = @tabId + 18 --Titre séparateur champ système
	UPDATE [DESC] SET [DispOrder] = 34, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 19 WHERE [DescId] = @tabId + 19 --Id tache planifiée
	UPDATE [DESC] SET [DispOrder] = 36, [Colspan] = 1, [RowSpan] = 1, [x] = 0, [y] = 20 WHERE [DescId] = @tabId + 17 --DescId onglet Destinataires
	UPDATE [DESC] SET [DispOrder] = 38, [Colspan] = 2, [RowSpan] = 4, [x] = 0, [y] = 21 WHERE [DescId] = @tabId + 21 --JSON infos récurences		
	
	fetch next from eventStep_cursor 
    into @tabId
end 
close eventStep_cursor;
deallocate eventStep_cursor;