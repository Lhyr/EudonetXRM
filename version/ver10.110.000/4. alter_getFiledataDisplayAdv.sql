/**
*     Récupère les userid et son groupid de la valeur passée en paramètre d'une rubrique de type USER
*     SPH & HLA - 10/04/09
*	MODIF KHA/SPH _ 30/05/2012 : passage de cte à crossapply pour les problèmes de limites de recursivité
*  


select * from 
*/
ALTER FUNCTION [dbo].[getFiledataDisplayAdv] (
	@s VARCHAR(max)
	,-- Valeur (id de la valeur)
	@sLang VARCHAR(20)
	,-- Langue en cours
	@nDescId INT
	,-- DescId du catalogue avancé
	@sep VARCHAR(10)
	,-- Séparateur de chaine
	@display VARCHAR(200) -- Masque spécifique
	)
RETURNS TABLE
AS
RETURN (
		SELECT lib + '#$|#$' + DATAID AS res
		FROM (
			SELECT RIGHT(dataid, LEN(dataid) - len(@sep)) dataid
			FROM (
				SELECT @s AS FileDataList
				) tab
			CROSS APPLY (
				SELECT isnull(@sep + dataid, '')
				FROM (
					SELECT cast(dataid AS VARCHAR(20)) AS dataid
						,isnull(CASE 
								WHEN upper(@sLang) = 'LANG_00'
									THEN lang_00
								WHEN upper(@sLang) = 'LANG_01'
									THEN lang_01
								WHEN upper(@sLang) = 'LANG_02'
									THEN lang_02
								WHEN upper(@sLang) = 'LANG_03'
									THEN lang_03
								WHEN upper(@sLang) = 'LANG_04'
									THEN lang_04
								WHEN upper(@sLang) = 'LANG_05'
									THEN lang_05
								WHEN upper(@sLang) = 'LANG_06'
									THEN lang_06
								WHEN upper(@sLang) = 'LANG_07'
									THEN lang_07
								WHEN upper(@sLang) = 'LANG_08'
									THEN lang_08
								WHEN upper(@sLang) = 'LANG_09'
									THEN lang_09
								ELSE lang_00
								END, '') AS Lib
					FROM [filedata]
					WHERE charindex(';' + cast(dataid AS VARCHAR(10)) + ';', ';' + FileDataList + ';') > 0
						AND descid = @nDescId
					) TAB
				ORDER BY lib --charindex(';' + cast(dataid as varchar(10)) +';',';' + FileDataList +';')
				FOR XML PATH('')
				) fd(dataid)
			) Dataid
		CROSS JOIN (
			SELECT lib
			FROM (
				SELECT SUBSTRING(fd.lib.query('root').value('.', 'nvarchar(max)'), len(@sep) + 1, LEN(fd.lib.query('root').value('.', 'nvarchar(max)'))) lib
				FROM (
					SELECT @s AS FileDataList
					) tab
				CROSS APPLY (
					SELECT isnull(@sep + replace(replace(replace(replace(replace(@display, '+''', ''), '''+', ''), '''', ''), '[TEXT]', isnull(lib, '')), '[DATA]', isnull([DATA], '')), '')
					FROM (
						SELECT isnull(CASE 
									WHEN upper(@sLang) = 'LANG_00'
										THEN lang_00
									WHEN upper(@sLang) = 'LANG_01'
										THEN lang_01
									WHEN upper(@sLang) = 'LANG_02'
										THEN lang_02
									WHEN upper(@sLang) = 'LANG_03'
										THEN lang_03
									WHEN upper(@sLang) = 'LANG_04'
										THEN lang_04
									WHEN upper(@sLang) = 'LANG_05'
										THEN lang_05
									WHEN upper(@sLang) = 'LANG_06'
										THEN lang_06
									WHEN upper(@sLang) = 'LANG_07'
										THEN lang_07
									WHEN upper(@sLang) = 'LANG_08'
										THEN lang_08
									WHEN upper(@sLang) = 'LANG_09'
										THEN lang_09
									ELSE lang_00
									END, '') AS Lib
							,[data]
							,dataid
						FROM [filedata]
						WHERE charindex(';' + cast(dataid AS VARCHAR(10)) + ';', ';' + FileDataList + ';') > 0
							AND descid = @nDescId
						) TAB
					ORDER BY lib --charindex(';' + cast(dataid as varchar(10)) +';',';' + FileDataList +';')
						-- On spécifie l'option type pour convertir le xml en varchar
						-- ceci règle le problème des caractères spéciaux.
					FOR XML PATH('')
						,root('root')
						,type
					) fd(lib)
				) Labels
			) GlobalResult
		)
