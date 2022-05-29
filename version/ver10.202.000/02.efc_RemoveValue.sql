 

-- 
/*
SPH - CREA le 12/02/2016
Création d'un fonction pour retirer une valeur en particulier dans une chaine type csv
 utilisation : select dbo.[efc_RemoveCsVValue]('a;c;b;ccc;bo;','b',';')

 */
 
create  function [dbo].[efc_RemoveCsVValue](@value varchar(max), @valuetoremove varchar(max), @sep varchar(5))
returns varchar(max)
as
BEGIN

	declare @resultat varchar(max)
	set @resultat = ''

	-- si la chaine ne contient pas la valeur a retirer, on la retourne
	if  CHARINDEX(  @sep + @valuetoremove + @sep , @sep + @value + @sep) =0 OR isnull(@valuetoremove,'')=''
		return @value

	
	-- sinon, on utilise la fonction de split pour retirer la valeur chosie et on concat.
	-- a voir si on utilise xpath avec le risque de perdre des caractères spéciaux xml...
		-- sinon, on utilise la fonction de split pour retirer la valeur chosie et on concat.
	-- a voir si on utilise xpath avec le risque de perdre des caractères spéciaux xml...
	select @resultat= + @resultat +';' + element  from efc_splitCTE(@value,@sep)  
	where element <> @valuetoremove order by n
	OPTION(MAXRECURSION 0) 

 
	if left(@resultat,1)=';'
		 set @resultat = substring(@resultat,2,len(@resultat)-1)

	if @resultat=''
		 set @resultat = NULL

	return @resultat
  
 END
 