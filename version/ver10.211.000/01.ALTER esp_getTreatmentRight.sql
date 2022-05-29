/*
SPH - récupère les permission/droit de traitement
Par defaut :
      10    Imprimer
      20    Exporter
      30    Filtrer
      40    Emailing
      41    Faxing
      42    Voicing
      60    Publipostage (Microsoft Word)
      70    Publipostage (HTML)
	  80	Publipostage (PDF)
      800   Rapport graphique
*/

ALTER PROCEDURE [dbo].[esp_getTreatmentRight]
      @UserId numeric,             -- userid
      @userlevel numeric,                -- @userlevel
      @groupid numeric,            -- @@groupid
      @traitids varchar(200) = '800;10;20;30;40;41;42;60;70;80'
AS 

select traitid N, p.[user],p.mode,

	case 
		when P.mode = 0 then case when @userlevel >= P.level then 1 else 0 end
		when P.mode = 1 then 
			case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0 then 1 else 0 end
		when P.mode = 3 then case when (@userlevel >= P.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0) then 1 else 0 end
		when P.mode = 2 then case when (@userlevel >= P.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P.[user] +';') > 0) then 1 else 0 end
			else 99
	end P
      
from trait as t  
left join [permission] P on P.[permissionid] =T.[permid] 
where charindex(';'+cast(traitid as varchar(10))+';', ';'+@traitids+';') > 0