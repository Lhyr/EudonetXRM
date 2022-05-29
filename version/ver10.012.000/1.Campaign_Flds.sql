
set nocount on;
/*GCH le 01/10/2014 - #32596 : [DEV] [MAILING] - 2 - Envoi différé - Rejouer le filtre lors de l'envoi - 1 - Deux nouveaux champs dans campagnes - SQL*/
--DescId
declare @nTab numeric(18,0), @nDescId numeric(18,0)
set @nTab = 106000
--RequestMode
set @nDescId = 30
IF NOT EXISTS(
	SELECT OBJ.* FROM SYSOBJECTS OBJ
		inner join sys.columns COL on COL.object_id = OBJ.id
	where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
		and OBJ.id = object_id(N'[dbo].[CAMPAIGN]')
		and COL.name = 'QueryMode'
		)
begin
	ALTER TABLE [dbo].[CAMPAIGN] ADD [QueryMode] tinyint null default(0)
end
if (
	 Not EXISTS ( select * from [desc] where (descid = @nTab+@nDescId and [Field] = 'QueryMode'))
	 )
begin
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) select @nTab+@nDescId, 'CAMPAIGN', 'QueryMode', 10, 0, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+@nDescId, 'Mode d''éxécution de la requête', 'Mode of query execution', 'Mode of query execution', 'Mode of query execution', 'Mode of query execution'
end

--Request
set @nDescId = 31
IF NOT EXISTS(
	SELECT OBJ.* FROM SYSOBJECTS OBJ
		inner join sys.columns COL on COL.object_id = OBJ.id
	where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
		and OBJ.id = object_id(N'[dbo].[CAMPAIGN]')
		and COL.name = 'Query'
		)
begin
	ALTER TABLE [dbo].[CAMPAIGN] ADD [Query] varchar(max) null default(0)
end
if (
	 Not EXISTS ( select * from [desc] where (descid = @nTab+@nDescId and [Field] = 'Query'))
	 )
begin
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) select @nTab+@nDescId, 'CAMPAIGN', 'Query', 1, -1, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab+@nDescId, 'Requête', 'Query', 'Query', 'Query', 'Query'
end

