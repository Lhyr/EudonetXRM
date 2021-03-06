 

/**
*	Récupère la valeur à afficher du catalogue en fonction du masque
*	SPH & HLA - 10/04/09
*/

ALTER function [dbo].[getFiledataDisplay]
(
	@s varchar(max),			-- Valeur (id de la valeur)
	@sLang varchar(20),			-- Langue en cours
	@nDescId int,				-- DescId du catalogue avancé
	@sep varchar(10),			-- Séparateur de chaine
	@display varchar(200)		-- Masque spécifique
) 
returns nvarchar(max)
as

BEGIN

	declare @resultat nvarchar(max)
	declare @rsdataid varchar(max)

	If @display =''
		set @display = '[TEXT]'

	-- Reconstitue le mask pour evité l'éxecution SQL
	set @display = replace(@display, '+''', '')
	set @display = replace(@display, '''+', '')
	set @display = replace(@display, '''', '')

	 
	
	SELECT  

		@resultat=COALESCE(@resultat + @sep, '')  +  replace(replace(@display, '[TEXT]', isnull(RES,'')), '[DATA]', isnull([DATA],'')) , 
		@rsdataid =COALESCE(@rsdataid + ';', '') + cast(dataid as varchar(10)) 
		 
	FROM  FILEDATA UNPIVOT (RES for LG in (lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10)) as u
	WHERE  DescId = @nDescId and LG = @slang and    charindex(';'+cast(dataid as varchar(10))+';',';'+@s+';')>0 	
	ORDER BY   RES   ,DATA
--	ORDER BY replace(replace(@display, '[TEXT]', isnull(RES,'')), '[DATA]', isnull([DATA],''))  

 
	 
	set @resultat = @resultat + '#$|#$' + @rsdataid
	return @resultat

END
