 
ALTER FUNCTION [dbo].[getAutoBuildNameAdvCI] (
	@d DATETIME,
@sPM01 nvarchar(max),
@sPP01 nvarchar(max),
@sPP02 nvarchar(max),
@sPP03 nvarchar(max), 
@sMask nvarchar(50),
@sEVT01 nvarchar(max), 
@nEVTID numeric,
@nDateFormat int 
) RETURNS TABLE AS RETURN (
SELECT
	CASE
		-- Si il n'y a pas d'event
		WHEN @nEVTID IS NULL THEN ''
		-- Si la rubrique "EVT01" n'est pas vide
		WHEN ISNULL(@sEVT01, '') <> '' THEN @sEVT01
		-- Si les rubriques de PP ou/et de PM sont vide et EVT95 n'est pas vide, on retourne le EVT95
		WHEN (
			CHARINDEX('$201$', @sMask) = 0 OR
			CHARINDEX('$201$', @sMask) <> 0 AND
			LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, ''))) = ''
			) AND
			(
			CHARINDEX('$301$', @sMask) = 0 OR
			CHARINDEX('$301$', @sMask) <> 0 AND
			LTRIM(RTRIM(ISNULL(@sPM01, ''))) = ''
			) AND
			@d IS NOT NULL THEN CONVERT(varchar(10), @d, @nDateFormat)
		ELSE
			-- DEBUT RIGHT
			CASE
				WHEN RIGHT(
					-- DEBUT LEFT
					CASE
						WHEN LEFT(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1) = '-' THEN RTRIM(LTRIM(STUFF(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1, 1, '')))
						ELSE RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							))
					END
					-- FIN LEFT
					, 1) = '-' THEN RTRIM(LTRIM(STUFF(
					-- DEBUT LEFT
					CASE
						WHEN LEFT(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1) = '-' THEN RTRIM(LTRIM(STUFF(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1, 1, '')))
						ELSE RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							))
					END
					-- FIN LEFT
					, LEN(
					-- DEBUT LEFT
					CASE
						WHEN LEFT(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1) = '-' THEN RTRIM(LTRIM(STUFF(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1, 1, '')))
						ELSE RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							))
					END
					-- FIN LEFT
					), 1, '')))
				ELSE
					-- DEBUT LEFT
					CASE
						WHEN LEFT(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1) = '-' THEN RTRIM(LTRIM(STUFF(RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							)), 1, 1, '')))
						ELSE RTRIM(LTRIM(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(@sMask, '$201$', LTRIM(RTRIM(ISNULL(@sPP03 + ' ', '') + ISNULL(@sPP01, '') + ' ' + ISNULL(@sPP02, '')))) -- PP
							, '$301$', LTRIM(RTRIM(ISNULL(@sPM01, '')))) -- PM
							, '$95$', CASE
								WHEN @d IS NULL THEN ''
								ELSE CONVERT(varchar(10), @d, @nDateFormat)
							END) -- EVT95
							, '  ', ' '), '- -', '-') -- CARACTERES SPECIAUX
							))
					END
			-- FIN LEFT
			END
	-- FIN RIGHT
	END AS res
)



