/**
*	Récupèrere les nom des tables et rubriques
*	CNA  - 26/07/2018
*/

CREATE  function [dbo].[getTabsFieldsDisplay]
(
	@s as varchar(max),			-- Valeur (descid des tables/champs séparés par des ';')
	@sLang as varchar(max),		-- Langue en cours
	@sep varchar(10),			-- Séparateur de chaine
	@bDisplayTabName as bit		-- Indique si on préfixe avec le nom de la table (pour les rubriques uniquements)
) 
returns nvarchar(max)
as
begin

	DECLARE @resultat NVARCHAR(max)
	
	IF ISNULL(@sep, '') = ''
		SET @sep = ';'
		
	IF @bDisplayTabName IS NULL
		SET @bDisplayTabName = 1
		
	
	SELECT @resultat = coalesce(@resultat + @sep, '') + isnull([ResTab] + '.', '') + [ResField]
	FROM [RES]
	UNPIVOT([ResField] FOR [LgField] IN (
				lang_00
				,lang_01
				,lang_02
				,lang_03
				,lang_04
				,lang_05
				,lang_06
				,lang_07
				,lang_08
				,lang_09
				,lang_10
				)) AS [uField]
	LEFT JOIN [RES]
	UNPIVOT([ResTab] FOR [LgTab] IN (
				lang_00
				,lang_01
				,lang_02
				,lang_03
				,lang_04
				,lang_05
				,lang_06
				,lang_07
				,lang_08
				,lang_09
				,lang_10
				)) AS [uTab] ON @bDisplayTabName = 1
		AND [uField].[ResId] % 100 <> 0
		AND [uField].[ResId] - [uField].[ResId] % 100 = [uTab].[ResId]
	WHERE charindex(';' + cast([uField].[ResId] AS VARCHAR(20)) + ';', ';' + @s + ';') > 0
		AND [LgField] = @sLang
		AND isnull([LgTab], @sLang) = @sLang

	
	RETURN @resultat

end
