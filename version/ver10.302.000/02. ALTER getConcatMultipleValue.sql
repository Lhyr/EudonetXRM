/**
*	Concatène des valeurs de catalogue sans retourner de doublons
*	HLA - 06/02/2013
*/
ALTER FUNCTION [dbo].[getConcatMultipleValue]
(
	@origineValue varchar(max),
	@addValue varchar(max),
	@sep varchar(10)
) 
RETURNS varchar(max)
as
BEGIN

	declare @resultat varchar(max)
	
	SET @resultat = 
		STUFF((
			select distinct @sep + TAB.[Element] AS [text()]
			from (
				select [Element] from [dbo].[efc_split](isnull(@origineValue + @sep, '') + isnull(@addValue, ''), @sep)
			) TAB FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)')
		, 1, 1, '')

	RETURN @resultat
END



