 

-- 
/*
SPH - CREA le 12/02/2016
Création d'un fonction pour retirer une valeur en particulier dans une chaine type csv
 utilisation : select dbo.[efc_RemoveCsVMultiValue]('a;c;b;ccc;bo;','b;bo',';')

 */
 
create  function [dbo].[efc_RemoveCsVMultiValue](@value varchar(max), @valuetoremove varchar(max), @sep varchar(5))
returns varchar(max)
as
BEGIN
	declare @resultat varchar(max)
	set @resultat = ''
	
	-- si la chaine ne contient pas la valeur a retirer, on la retourne
	if not exists(select 1 from efc_splitCTE(@value,@sep) aa inner join efc_splitCTE(@valuetoremove,@sep) bb on aa.element = bb.element)
		return @value
	
	-- sinon, on utilise la fonction de split pour retirer la valeur chosie et on concat.
	-- a voir si on utilise xpath avec le risque de perdre des caractères spéciaux xml...
		-- sinon, on utilise la fonction de split pour retirer la valeur chosie et on concat.
	-- a voir si on utilise xpath avec le risque de perdre des caractères spéciaux xml...
	select @resultat= + @resultat +';' + element  from efc_splitCTE(@value,@sep)  
	where element<> '' and element not in( select element from efc_splitCTE( @valuetoremove,@sep) ) order by n
	OPTION(MAXRECURSION 0) 

 
	if left(@resultat,1)=';'
		 set @resultat = substring(@resultat,2,len(@resultat)-1)

	if @resultat=''
		 set @resultat = NULL

	return @resultat
 END
 