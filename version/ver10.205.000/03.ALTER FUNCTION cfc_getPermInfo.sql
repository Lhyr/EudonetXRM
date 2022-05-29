/**
*	Récupère les permissions 
*	SPH 
*	18/12/2014 : GCH - si permmode null : on a les droits de visu. (bug #35938)
select * from [dbo].[cfc_getPermInfo](29,99,1 ) where [permissionid] in(319,	320,	321,	322)
*	CRU 10/05/2015 : Si le mode est à NONE, on a les droits également
*/

ALTER FUNCTION [dbo].[cfc_getPermInfo]
(	
	@userid as numeric,
	@userlevel as numeric,
	@groupid as numeric 
	
)
RETURNS TABLE
as

RETURN
(

select  p.[permissionid], 
	case 
			when P.mode = 0 then 
				case 
					when @userlevel >= P.level then 1
					else 0 
				end
			when P.mode = 1 then 
				case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0 then 1 else 0 end
				
			when P.mode = 3 then case when (@userlevel >= P.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0) then 1 else 0 end
			
			when P.mode = 2 then case when (@userlevel >= P.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0) then 1 else 0 end
			when (P.mode is null OR P.mode = -1) then 1
			else 99
	end P
	
from 
	[permission] P

)
