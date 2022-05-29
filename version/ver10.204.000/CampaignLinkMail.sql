--/**********************************************************************************************************************/
/*    GCH
      Création du champ de répondre à et nom apparent
      18/12/2014
*/
set nocount on;
declare @nTab int,
	@tabName as varchar(100)
	
declare curs cursor for
	SELECT [DescId], [FILE] FROM [DESC] WHERE [TYPE] = 3
open curs
fetch next from curs
      into @nTab, @tabName
while @@FETCH_STATUS = 0
begin
	--3427	Répondre à
	if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like @tabName and syscolumns.name like 'TPL27')
	BEGIN
		exec('ALTER TABLE ['+@tabName+'] ADD [TPL27] [varchar](500) NULL');
	END	
	if((select COUNT(*) from [DESC] where DescId = @nTab+27 ) <= 0)
	begin
		insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+27, @tabName, 'TPL27', 1, 500, 27, 1      
	end
	if((select COUNT(*) from [Res] where ResId = @nTab+27 ) <= 0)
	begin
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+27, 'Répondre à', 'Reply to', 'Reply to', 'Reply to', 'Reply to'  
	end

	--/**********************************************************************************************************************/
	--3426	Nom apparent
	if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like @tabName and syscolumns.name like 'TPL26')
	BEGIN
		exec('ALTER TABLE ['+@tabName+'] ADD [TPL26] [varchar](500) NULL');
	END      
	if((select COUNT(*) from [DESC] where DescId = @nTab+26 ) <= 0)
	begin
		insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+26, @tabName, 'TPL26', 1, 500, 26, 1      
	end
	if((select COUNT(*) from [Res] where ResId = @nTab+26 ) <= 0)
	begin
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+26, 'Nom apparent', 'Display name', 'Display name', 'Display name', 'Display name'  
	end
	--/**********************************************************************************************************************/

	-- HLA - Les rubriques Nom apparent et Répondre à ne doivent pas être en lecture seul
	update [desc] set [readonly] = null where descid = @nTab+27
	update [desc] set [readonly] = null where descid = @nTab+26
	
	fetch next from curs into @nTab, @tabName
end

close curs
deallocate curs
