ALTER PROCEDURE [dbo].[xsp_getTableInfo]
	@Descid numeric		-- DescId
AS 

SELECT  [DisplayFirstBkmOnly], IsNull([DisableGroup], 0) as [DisableGroup], [desc].[icon], [DESC].[IconColor],
	isnull([adrjoin], 0) as [adrjoin], isnull([interpp], 0) as [interpp], isnull([interpm], 0) as [interpm], 
	isnull([interevent], 0) as [interevent], isnull([intereventnum], 0) as [intereventnum], [file], 
	isnull([InterPPNeeded], 0) as [InterPPNeeded], isnull([InterPMNeeded], 0) as [InterPMNeeded], isnull([InterEventNeeded], 0) as [InterEventNeeded],
	[desc].[type], [ObligatReadOnly], [PagingEnabled], [CountOnDemand], [Field], [AutoBuildName], 
	[DeleteRulesId], [ChangeRulesId], [ColorRulesId], VIEW_H.[value] as VH, UPDT_H.[value] as UPDT_H, 
	[ProspectEnabled], [columns], isnull([alertenabled], 0) as [alertenabled], isnull([BreakLine],0) as [BreakLine],
	isnull([AutoCreate], 0) as [AutoCreate], isnull([AutoSave],0) as [AutoSave], isnull([CalendarEnabled], 0) as [CalendarEnabled], isnull([SelectionSourceTab], 0) AS [SelectionSourceTab]
	, isnull([ActiveTab], 0) as [ActiveTab], isnull([searchlimit], 3) as [SearchLimit]
FROM [desc] 
	LEFT JOIN [pref] on [pref].[tab] = [desc].[descid] and [pref].[userid] = 0
	LEFT JOIN [uservalue] as VIEW_H on [desc].[descid] = VIEW_H.[tab] and VIEW_H.[UserId] is null and VIEW_H.[type] = 7
	LEFT JOIN [uservalue] as UPDT_H on [desc].[descid] = UPDT_H.[tab] and UPDT_H.[UserId] is null and UPDT_H.[type] = 8
WHERE [desc].[descid] = @Descid
