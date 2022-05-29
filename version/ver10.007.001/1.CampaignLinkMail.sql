/* 	GCH
	Création du champ user display name et reply to qui ont été oublié en aout dernier
	31/07/2014
 */
 set nocount on;
declare @nTab int,
	@tabName as varchar(100),
	@nDescId as int,
	@sql as varchar(max)
	
declare curs cursor for
	SELECT [DescId], [FILE] FROM [DESC] WHERE [TYPE] = 3
open curs
fetch next from curs
	into @nTab, @tabName
while @@FETCH_STATUS = 0
begin	
	
	set @nDescId = 26
	if not exists (SELECT 1
			from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
			where sys.tables.name like @tabName and syscolumns.name like 'TPL'+convert(varchar(max),@nDescId) )
	BEGIN
		--Nom apparent
		set @sql = 'ALTER TABLE ['+@tabName+'] ADD [TPL'+convert(varchar(max),@nDescId)+'] [varchar](500) NULL'
		exec(@sql);
	END
	if not exists ( select 1 from [desc] where descid = @nTab+@nDescId )
	begin
	
		delete from [res] where resid = @nTab+@nDescId
		insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+@nDescId, @tabName, 'TPL'+convert(varchar(max),@nDescId), 1, 500, @nDescId, 0
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+@nDescId, 'Nom apparent', 'Display name', 'Display name', 'Display name', 'Display name'
	end
	set @nDescId = 27
	if not exists (SELECT 1
			from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
			where sys.tables.name like @tabName and syscolumns.name like 'TPL'+convert(varchar(max),@nDescId) )
	BEGIN
		--Répondre à
		set @sql = 'ALTER TABLE ['+@tabName+'] ADD [TPL'+convert(varchar(max),@nDescId)+'] [varchar](500) NULL'
		exec(@sql);
	END
		
	if not exists ( select 1 from [desc] where descid = @nTab+@nDescId )
	begin
		delete from [res] where resid = @nTab+@nDescId
		insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder], [readonly]) select @nTab+@nDescId, @tabName, 'TPL'+convert(varchar(max),@nDescId), 1, 500, @nDescId, 0
		insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+@nDescId, 'Répondre à', 'Reply to', 'Reply to', 'Reply to', 'Reply to'
	end
	fetch next from curs into @nTab, @tabName
end

close curs
deallocate curs