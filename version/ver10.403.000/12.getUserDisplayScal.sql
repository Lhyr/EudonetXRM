/**
*	Récupèrere les userdisplayname ou le groupname de la valeur passÃ© en paramÃ¨tre d'une rubrique de type USER
*	SPH  - 
*/

CREATE  function [dbo].getUserDisplayScal
(
	@s varchar(max),
	@sep varchar(10)
) 
returns varchar(max)
as
begin

declare @resultat varchar(max)
set @resultat =''

declare @sIntSep varchar(5)
set @sIntSep = '#$|#$'

select @resultat = @resultat + @sep + isnull(TAB.display,'') +@sIntSep + id
from (
	select userdisplayname as display, cast(userid as varchar(10)) id from [user]
	where charindex(';' + cast(userid as varchar(10)) +';',';' + @s +';')>0
	
	union
	
	select groupname as display,  'G' + cast(groupid as varchar(10))  id from [group]
	--inner join [user] on [user].[groupid] = [group].[groupid]
	where charindex(';G' + cast([group].groupid as varchar(10)) +';',';' + @s +';')>0
) TAB

if len(@resultat)>0
	set @resultat =  right(@resultat,len(@resultat) - len(@sep)) 

return @resultat
end
