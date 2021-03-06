 
/*
*     Retourne la description de la rubrique
*     SPH & HLA - 10/04/09
*     HLA - MODIF - 28/08/2018
*/

CREATE PROCEDURE [dbo].[xsp_getTableDescFromDescIdV2] 
	@DescId NUMERIC,		-- DescId
	@UserId NUMERIC,		-- UserId
	@GroupId int,			-- GroupId
	@UserLevel int,			-- UserLevel
	@Lang varchar(20)		-- Lang
 
AS

declare @sLib nvarchar(100)

declare @sLibPj nvarchar(100)
declare @sLibDesc nvarchar(100)
declare @sLibInfo nvarchar(100)
declare @sLibNote nvarchar(100)

set @Lang = upper(@Lang)

select @sLib = RES from dbo.efc_getResid(@Lang, @DescId)
select @sLibPj = RES from dbo.efc_getResid(@Lang, @DescId + 91)
select @sLibDesc = RES from dbo.efc_getResid(@Lang, @DescId + 89)
select @sLibInfo = RES from dbo.efc_getResid(@Lang, @DescId + 93)
select @sLibNote = RES from dbo.efc_getResid(@Lang, @DescId + 94)

SELECT  @sLib as libelle, [desc].[ToolTipText], [desc].[ObligatReadOnly], [desc].[ProspectEnabled],
      @sLibPj as pjLib, [pj_desc].[icon] as pjIcon,
      @sLibDesc as descLib, [DESCRIPTION_DESC].[icon] as descIcon,
      @sLibInfo as infoLib, [info_desc].[icon] as infoIcon,
      @sLibNote as noteLib, [note_desc].[icon] as noteIcon,
      [desc].[TreatmentMaxRows], [desc].[rowspan], [desc].[colspan], [desc].[Disabled]
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, [desc].[viewpermid]), 1) VIEW_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_ADD.PermId), 1) ADD_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_ADD_LIST.PermId), 1) ADD_LIST_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_MODIF.PermId), 1) MODIF_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, t_DEL.PermId), 1) DEL_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, t_HISTO.PermId), 1) HISTO_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_DUPLI.PermId), 1) DUPLI_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, pj_desc.[viewpermid]), 1) VIEW_PJ
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_ADD_PJ.PermId), 1) ADD_PJ
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_MODIF_PJ.PermId), 1) MODIF_PJ
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_DEL_PJ.PermId), 1) DEL_PJ
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_ADD_MULTI.PermId), 1) ADD_MULTI_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_MODIF_MULTI.PermId), 1) MODIF_MULTI_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_DEL_MULTI.PermId), 1) DEL_MULTI_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_DUPLI_MULTI.PermId), 1) DUPLI_MULTI_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_MULTI_FROMFILTER.PermId), 1) MULTI_FROMFILTER_P
	, case when ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @groupid, [DESC].[LeftMenuLinksPermId]), 0) = 0 then 1 else 0 end  ACTION_MENU_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_GLOBAL_LINK.PermId), 1) GLOBAL_LINK_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_IMPORT_TAB.PermId), 1) IMPORT_TAB_P
	,ISNULL(dbo.xfc_getPermInfo(@UserId, @UserLevel, @GroupId, T_IMPORT_BKM.PermId), 1) IMPORT_BKM_P
	,[DESC].[InterPpNeeded]
	,[DESC].[InterPmNeeded]
	,[DESC].[InterEventNeeded]
	,[DESC].[InterEventHidden]
	,[DESC].[NoDefaultLink_100]
	,[DESC].[NoDefaultLink_200]
	,[DESC].[NoDefaultLink_300]
	,[DESC].[NoCascadePMPP]
	,[DESC].[NoCascadePPPM]
	,[DESC].[AutoCompletion]
	,ISNULL(COMPLETENAMEFORMAT, '0') AS CompleteNameFormat
	,ISNULL(IS_EVENT_STEP, '0') AS IsEventStep
FROM [desc]
INNER JOIN [res] ON [desc].[descid] = [res].[resid]

LEFT JOIN [TRAIT] AS T_ADD ON [desc].[descid] + 1 = T_ADD.[traitid]

LEFT JOIN [TRAIT] AS T_ADD_LIST ON [desc].[descid] + 15 = T_ADD_LIST.[traitid]

LEFT JOIN [TRAIT] AS T_MODIF ON [desc].[descid] + 2 = T_MODIF.[traitid]

LEFT JOIN [TRAIT] AS T_DEL ON [desc].[descid] + 3 = T_DEL.[traitid]

LEFT JOIN [TRAIT] AS T_HISTO ON [desc].[descid] + 4 = T_HISTO.[traitid]

LEFT JOIN [TRAIT] AS T_DUPLI ON [desc].[descid] + 5 = T_DUPLI.[traitid]

/* ICON PJ */
LEFT JOIN [desc] AS PJ_DESC ON [desc].[file] = [pj_desc].[file]
	AND [pj_desc].[descid] = (@Descid + 91)
/* DROIT TREATEMENT PJ */

/* ICON Description */
LEFT JOIN [desc] AS DESCRIPTION_DESC ON [desc].[file] = [DESCRIPTION_DESC].[file]
	AND [DESCRIPTION_DESC].[descid] = (@Descid + 89)
/* ICON INFO */
LEFT JOIN [desc] AS INFO_DESC ON [desc].[file] = [info_desc].[file]
	AND [info_desc].[descid] = (@Descid + 93)
/* ICON NOTE */
LEFT JOIN [desc] AS NOTE_DESC ON [desc].[file] = [note_desc].[file]
	AND [note_desc].[descid] = (@Descid + 94)
/* TRAITEMENTS */
LEFT JOIN [TRAIT] AS T_ADD_MULTI ON [desc].[descid] + 6 = T_ADD_MULTI.[traitid]

LEFT JOIN [TRAIT] AS T_MODIF_MULTI ON [desc].[descid] + 7 = T_MODIF_MULTI.[traitid]

LEFT JOIN [TRAIT] AS T_DEL_MULTI ON [desc].[descid] + 8 = T_DEL_MULTI.[traitid]

LEFT JOIN [TRAIT] AS T_DUPLI_MULTI ON [desc].[descid] + 9 = T_DUPLI_MULTI.[traitid]

LEFT JOIN [TRAIT] AS T_ADD_PJ ON [desc].[descid] + 11 = T_ADD_PJ.[traitid]

LEFT JOIN [TRAIT] AS T_MODIF_PJ ON [desc].[descid] + 12 = T_MODIF_PJ.[traitid]

LEFT JOIN [TRAIT] AS T_DEL_PJ ON [desc].[descid] + 13 = T_DEL_PJ.[traitid]

LEFT JOIN [TRAIT] AS T_MULTI_FROMFILTER ON [desc].[descid] + 17 = T_MULTI_FROMFILTER.[traitid]

-- Import en masse sur un onglet
LEFT JOIN [TRAIT] AS T_IMPORT_TAB ON [desc].[descid] + 18 = T_IMPORT_TAB.[traitid]

-- Import en masse depuis un signet
LEFT JOIN [TRAIT] AS T_IMPORT_BKM ON [desc].[descid] + 19 = T_IMPORT_BKM.[traitid]


LEFT JOIN [TRAIT] AS T_GLOBAL_LINK ON [desc].[descid] + 16 = T_GLOBAL_LINK.[traitid]

LEFT JOIN (
	SELECT [DESCADVPIVOT].[DESCID]
		,[9] AS 'COMPLETENAMEFORMAT'
		,[42] AS 'IS_EVENT_STEP'
	FROM [DESC]
	LEFT JOIN (
		SELECT [DESCID]
			,[PARAMETER]
			,[VALUE]
		FROM [DESCADV]
		WHERE [DESCID] = @descid
		) R
	PIVOT(max(value) FOR parameter IN ([9], [42])) AS [DESCADVPIVOT] ON [DESCADVPIVOT].DescId = [desc].DescId
	WHERE [DESC].[DESCID] = @descid
	) AS DESCP ON DESCP.DESCID = [DESC].[DESCID]
WHERE [desc].[descid] = @DescId
ORDER BY libelle
