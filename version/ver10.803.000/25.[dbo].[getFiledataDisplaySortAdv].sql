IF (OBJECT_ID (N'[dbo].[getFiledataDisplaySortAdv]', N'IF') IS NOT NULL)
	drop function [dbo].[getFiledataDisplaySortAdv];

--go

/**
*     Récupère les userid et son groupid de la valeur passée en paramètre d'une rubrique de type USER

*/

begin try
	begin tran
	exec (N'Create FUNCTION [dbo].[getFiledataDisplaySortAdv] (
		@s VARCHAR(max)
		,-- Valeur (id de la valeur)
		@sLang VARCHAR(20)
		,-- Langue en cours
		@nDescId INT
		,-- DescId du catalogue avancé
		@sep VARCHAR(10)
		,-- Séparateur de chaine
		@display VARCHAR(200), -- Masque spécifique
		@sorted varchar(200))
	RETURNS TABLE
	AS
	RETURN (
			SELECT rtrim(ltrim(lib)) + ''#$|#$'' + DATAID AS res
			FROM (
				SELECT RIGHT(dataid, LEN(dataid) - len(@sep)) dataid
				FROM (
					SELECT @s AS FileDataList
					) tab
				CROSS APPLY (
					SELECT isnull(@sep + dataid, '''')
					FROM (
						SELECT cast(dataid AS VARCHAR(20)) AS dataid
							,[data]
							,isnull(CASE 
									WHEN upper(@sLang) = ''LANG_00''
										THEN lang_00
									WHEN upper(@sLang) = ''LANG_01''
										THEN lang_01
									WHEN upper(@sLang) = ''LANG_02''
										THEN lang_02
									WHEN upper(@sLang) = ''LANG_03''
										THEN lang_03
									WHEN upper(@sLang) = ''LANG_04''
										THEN lang_04
									WHEN upper(@sLang) = ''LANG_05''
										THEN lang_05
									WHEN upper(@sLang) = ''LANG_06''
										THEN lang_06
									WHEN upper(@sLang) = ''LANG_07''
										THEN lang_07
									WHEN upper(@sLang) = ''LANG_08''
										THEN lang_08
									WHEN upper(@sLang) = ''LANG_09''
										THEN lang_09
									ELSE lang_00
									END, '''') AS Lib
						FROM [filedata]
						WHERE charindex('';'' + cast(dataid AS VARCHAR(10)) + '';'', '';'' + FileDataList + '';'') > 0
							AND descid = @nDescId
						) TAB
					ORDER BY replace(replace(replace(replace(replace(@sorted, ''+'''''', ''''), ''''''+'', ''''), '''''''', ''''), ''[TEXT]'', isnull(lib, '''')), ''[DATA]'', isnull([DATA], '''')) 
					FOR XML PATH('''')
					) fd(dataid)
				) Dataid
			CROSS JOIN (
				SELECT lib
				FROM (
					SELECT SUBSTRING(fd.lib.query(''root'').value(''.'', ''nvarchar(max)''), len(@sep) + 1, LEN(fd.lib.query(''root'').value(''.'', ''nvarchar(max)''))) lib
					FROM (
						SELECT @s AS FileDataList
						) tab
					CROSS APPLY (
						SELECT isnull(@sep + replace(replace(replace(replace(replace(@display, ''+'''''', ''''), ''''''+'', ''''), '''''''', ''''), ''[TEXT]'', isnull(lib, '''')), ''[DATA]'', isnull([DATA], '''')), '''')
						FROM (
							SELECT isnull(CASE 
										WHEN upper(@sLang) = ''LANG_00''
											THEN lang_00
										WHEN upper(@sLang) = ''LANG_01''
											THEN lang_01
										WHEN upper(@sLang) = ''LANG_02''
											THEN lang_02
										WHEN upper(@sLang) = ''LANG_03''
											THEN lang_03
										WHEN upper(@sLang) = ''LANG_04''
											THEN lang_04
										WHEN upper(@sLang) = ''LANG_05''
											THEN lang_05
										WHEN upper(@sLang) = ''LANG_06''
											THEN lang_06
										WHEN upper(@sLang) = ''LANG_07''
											THEN lang_07
										WHEN upper(@sLang) = ''LANG_08''
											THEN lang_08
										WHEN upper(@sLang) = ''LANG_09''
											THEN lang_09
										ELSE lang_00
										END, '''') AS Lib
								,[data]
								,dataid
							FROM [filedata]
							WHERE charindex('';'' + cast(dataid AS VARCHAR(10)) + '';'', '';'' + FileDataList + '';'') > 0
								AND descid = @nDescId
							) TAB
						ORDER BY replace(replace(replace(replace(replace(@sorted, ''+'''''', ''''), ''''''+'', ''''), '''''''', ''''), ''[TEXT]'', isnull(lib, '''')), ''[DATA]'', isnull([DATA], ''''))
							-- On spécifie l''option type pour convertir le xml en varchar
							-- ceci règle le problème des caractères spéciaux.
						FOR XML PATH('''')
							,root(''root'')
							,type
						) fd(lib)
					) Labels
				) GlobalResult
			)')
	commit tran;
end try
begin catch

	declare @ErrorMessage as nvarchar(max);  
    declare @ErrorSeverity as int;  
    declare @ErrorState as int;  
  
    select @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();  
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState); 
	rollback tran;
end catch