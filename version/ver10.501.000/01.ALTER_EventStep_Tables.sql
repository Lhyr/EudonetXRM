declare @strExecTemplate as varchar(max) =
	'IF NOT EXISTS (SELECT 1
				FROM sys.tables INNER JOIN syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name LIKE ''<TABNAME>'' AND syscolumns.name LIKE ''<SHORTFIELDNAME>24'')
	BEGIN
		ALTER TABLE [<TABNAME>] ADD [<SHORTFIELDNAME>24] VARCHAR(100) NULL
	END'

declare @tabId as numeric(18)

declare cursorTables cursor for

select distinct [DescId]
from [DESCADV]
where [Parameter] = 42
and ltrim(rtrim([Value])) = '1'
and [DescId] % 100 = 0

open cursorTables

fetch next from cursorTables 
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
	set @strExec = @strExecTemplate
	
	select @tabName = [File], @shortFieldName = [Field] from [DESC] where [DescId] = @tabId

	set @strExec = REPLACE(@strExec, '<TABNAME>', @tabName)
	set @strExec = REPLACE(@strExec, '<SHORTFIELDNAME>', @shortFieldName)

	EXEC (@strExec)
	
	
	declare @fieldId as numeric(18)
	set @fieldId = @tabId + 24
	
	--DESC
	IF NOT EXISTS (select 1 from [DESC] where [DescId] = @fieldId)
	BEGIN
		INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Multiple], [Popup], [PopupDescId], [BoundDescId], [Rowspan], [Colspan], [DispOrder], [x], [y]) 
		VALUES (@fieldId, @tabName, @shortFieldName + '24', 1, 100, 0, 3, @fieldId, 0, 1, 1, 6, 1, 2)
		
		UPDATE [DESC] SET
		[Case] = 0
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
		where [DescId] = @fieldId		
	END
	
	--RES
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldId)
	AND NOT EXISTS (select 1 from [RES] where [ResId] = @fieldId)
	BEGIN
		INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
		VALUES (@fieldId, 'Statut', 'Status', 'Status', 'Statuut', 'Estatuto', 'Statuto')
	END
	
	--FileDataParam
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldId)
	AND NOT EXISTS (select 1 from [FileDataParam] where [DescId] = @fieldId)
	BEGIN
		INSERT INTO [FileDataParam] ([DescId], [LangUsed], [DisplayMask], [SortBy])
		VALUES (@fieldId, 0, '[TEXT]', '[TEXT]')
	END
	
	--FileData
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldId)
	AND NOT EXISTS (select 1 from [FileData] where [DescId] = @fieldId and [Data] = '1')
	BEGIN
		INSERT INTO [FileData] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05], [Tip_Lang_00_Format], [Tip_Lang_01_Format], [Tip_Lang_02_Format], [Tip_Lang_03_Format], [Tip_Lang_04_Format], [Tip_Lang_05_Format], [Tip_Lang_06_Format], [Tip_Lang_07_Format], [Tip_Lang_08_Format], [Tip_Lang_09_Format], [Tip_Lang_10_Format])
		VALUES (@fieldId, '1', 'Annulé', 'Annulé', 'Annulé', 'Annulé', 'Annulé', 'Annulé', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
	END
	
	--FileData
	IF EXISTS (select 1 from [DESC] where [DescId] = @fieldId)
	AND NOT EXISTS (select 1 from [FileData] where [DescId] = @fieldId and [Data] = '2')
	BEGIN
		INSERT INTO [FileData] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05], [Tip_Lang_00_Format], [Tip_Lang_01_Format], [Tip_Lang_02_Format], [Tip_Lang_03_Format], [Tip_Lang_04_Format], [Tip_Lang_05_Format], [Tip_Lang_06_Format], [Tip_Lang_07_Format], [Tip_Lang_08_Format], [Tip_Lang_09_Format], [Tip_Lang_10_Format])
		VALUES (@fieldId, '2', 'Échec', 'Failure', 'Scheitern', 'Mislukking', 'Fracaso', 'Fallimento', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
	END
	
	
	fetch next from cursorTables 
    into @tabId
end 
close cursorTables;
deallocate cursorTables;

