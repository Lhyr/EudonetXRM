/*
*	Retourne les info du field
*	HLA - 14/03/17
*/
ALTER PROCEDURE [dbo].[xsp_getFieldInfo] @Descid NUMERIC -- DescId
AS
SELECT isnull([relation], 0) [relation]
	,isnull([storage], 0) [storage]
	,(
		CASE 
			WHEN [Format] = 8 AND (
					SELECT count(1)
					FROM [UserValue] AS GROUPSONLY
					WHERE isnull(GROUPSONLY.[Enabled], 0) = 1 AND GROUPSONLY.[Type] = 23 AND GROUPSONLY.[DescId] = [DESC].[DescId]
					) > 0
				THEN 14
			ELSE ISNULL([Format], 0)
			END
		) AS [Format]
	,[field]
	,isnull([popup], 0) AS [popup]
	,isnull([popupdescid], 0) AS [popupdescid]
	,isnull([multiple], 0) AS [multiple]
	,[unicode]
	,[viewrulesid]
	,[changerulesid]
	,[viewpermid]
	,[updatepermid]
	,[obligatrulesid]
	,isnull([bounddescid], 0) AS [bounddescid]
	,(
		SELECT CASE 
				WHEN T.type = 0
					THEN 1
				ELSE 0
				END
		FROM [desc] T
		WHERE isnull([desc].[popupdescid], 0) LIKE '%01' AND isnull([desc].[popupdescid], 0) <> [desc].[descid] AND isnull([desc].[popup], 0) = 2 AND T.[descid] = isnull([desc].[popupdescid], 0) - isnull([desc].[popupdescid], 0) % 100
		) AS specialpopup
	,rowspan
	,colspan
	,disporder
	,[default]
	,isnull([defaultformat], 0) AS [defaultformat]
	,
	-- DESCADV_PARAMETER.ALIASSOURCE = 10
	(
		SELECT TOP 1 Value
		FROM DESCADV
		WHERE Parameter = 10 AND DescId = @Descid
		) AS [aliasparam]
	,
	-- DESCADV_PARAMETER.ALIASrelation = 19
	(
		SELECT TOP 1 Value
		FROM DESCADV
		WHERE Parameter = 19 AND DescId = @Descid
		) AS [RelationSource]
FROM [desc]
WHERE descid = @Descid
