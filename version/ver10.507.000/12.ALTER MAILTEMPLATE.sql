DECLARE @idSubject INT = 107002
DECLARE @idMailTemplatePreHeader INT = 107012
DECLARE @bIsUnicode BIT = 0

IF EXISTS (
		SELECT *
		FROM [CONFIGADV]
		WHERE [Parameter] = 'FULL_UNICODE'
			AND [Value] = '1'
		)
BEGIN
	SET @bIsUnicode = 1
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.tables
		INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name LIKE 'MAILTEMPLATE'
			AND syscolumns.name LIKE 'PreHeader'
		)
BEGIN
	IF @bIsUnicode = 1
	BEGIN
		ALTER TABLE [MAILTEMPLATE] ADD [PreHeader] NVARCHAR(140) NULL;
	END
	ELSE
	BEGIN
		ALTER TABLE [MAILTEMPLATE] ADD [PreHeader] VARCHAR(140) NULL;
	END
END

IF NOT EXISTS (
		SELECT *
		FROM [DESC]
		WHERE [DescId] = @idMailTemplatePreHeader
		)
BEGIN
	INSERT INTO [DESC] (
		[DescId]
		,[File]
		,[Field]
		,[Format]
		,[Length]
		,[Case]
		,[ToolTipText]
		,[Formula]
		,[Default]
		,[Historic]
		,[Obligat]
		,[Multiple]
		,[Popup]
		,[PopupDescId]
		,[BoundDescId]
		,[ActiveTab]
		,[ActiveBkmEvent]
		,[ActiveBkmPp]
		,[ActiveBkmPm]
		,[GetLevel]
		,[ViewLevel]
		,[UserLevel]
		,[InterEvent]
		,[InterPp]
		,[InterPm]
		,[Rowspan]
		,[Colspan]
		,[DispOrder]
		,[TabIndex]
		,[Bold]
		,[Italic]
		,[Underline]
		,[Flat]
		,[ForeColor]
		,[AutoBuildName]
		,[Mask]
		,[Type]
		,[InterEventNum]
		,[DefaultFormat]
		,[Disabled]
		,[Columns]
		,[ExternalDataSource]
		,[Icon]
		,[ReadOnly]
		,[ChangeRulesId]
		,[AlertEnabled]
		,[Html]
		,[RowsBkm]
		,[AutoSave]
		,[ViewPermId]
		,[UpdatePermId]
		,[BkmViewRulesId_100]
		,[BkmViewRulesId_200]
		,[BkmViewRulesId_300]
		,[ObligatRulesId]
		,[LabelAlign]
		,[Relation]
		,[Storage]
		,[Unicode]
		,[Rows]
		,[NoDefaultClone]
		,[PagingEnabled]
		,[BkmAddRulesId_100]
		,[BkmAddRulesId_200]
		,[BkmAddRulesId_300]
		,[NoDefaultLink_100]
		,[NoDefaultLink_200]
		,[NoDefaultLink_300]
		,[DeleteRulesId]
		,[DefaultLink_100]
		,[DefaultLink_200]
		,[DefaultLink_300]
		,[AutoSelectEnabled]
		,[AutoSelectValue]
		,[LeftMenuLinksPermId]
		,[InterPpNeeded]
		,[InterPmNeeded]
		,[BkmInlineModeEnabled]
		,[NbrBkmInline]
		,[CountOnDemand]
		,[TreatmentMaxRows]
		,[EventAnnexListEnabled]
		,[PmAnnexListEnabled]
		,[PpAnnexListEnabled]
		,[SizeLimit]
		,[Scrolling]
		,[RestrictedSearch]
		,[ColorRulesId]
		,[ViewRulesId]
		,[SavePJ2Text]
		,[InterEventNeeded]
		,[DraftAutoSaveActive]
		,[DraftAutoSaveInterval]
		,[BkmViewPermId_100]
		,[BkmViewPermId_200]
		,[BkmViewPermId_300]
		,[SearchLimit]
		,[ProspectEnabled]
		,[SavePJ2TextField]
		,[BkmPagingMode]
		,[Parameters]
		,[AdvancedSearchDescId]
		,[ComputedColsInList]
		,[ComputedColsInBkm]
		,[ComputedFieldEnabled]
		,[DblWindowDisplayedFields]
		,[DblWindowShowConfidential]
		,[DblWindowHideToolTip]
		,[ActiveScroll]
		,[DisableGroup]
		,[DetailBkmBarButtons]
		,[TreeViewUserList]
		,[FullUserList]
		,[BkmAutoLinkPmToPPEnabled]
		,[ObligatReadOnly]
		,[DisplayFirstBkmOnly]
		,[IntellimailEnabled]
		,[InterEventHidden]
		,[BreakLine]
		,[SAVEBKMPAGING]
		,[SelectionSourceTab]
		,[NoCascadePPPM]
		,[NoCascadePMPP]
		,[AutoCompletion]
		,[IconColor]
		)
	SELECT @idMailTemplatePreHeader
		,[File]
		,'PreHeader'
		,1
		,140
		,[Case]
		,'Le texte d''aperçu permet de définir le texte qui s''affiche à la suite de l''objet dans les listes de courriels des messageries, s''il n''est pas renseigné alors la première ligne du corps du courriel est utilisée. La longeur conseillée pour votre preheader est de 35 caractères.'
		,[Formula]
		,[Default]
		,[Historic]
		,[Obligat]
		,[Multiple]
		,[Popup]
		,[PopupDescId]
		,[BoundDescId]
		,[ActiveTab]
		,[ActiveBkmEvent]
		,[ActiveBkmPp]
		,[ActiveBkmPm]
		,[GetLevel]
		,[ViewLevel]
		,[UserLevel]
		,[InterEvent]
		,[InterPp]
		,[InterPm]
		,[Rowspan]
		,[Colspan]
		,[DispOrder]
		,[TabIndex]
		,[Bold]
		,[Italic]
		,[Underline]
		,[Flat]
		,[ForeColor]
		,[AutoBuildName]
		,[Mask]
		,[Type]
		,[InterEventNum]
		,[DefaultFormat]
		,[Disabled]
		,[Columns]
		,[ExternalDataSource]
		,[Icon]
		,[ReadOnly]
		,[ChangeRulesId]
		,[AlertEnabled]
		,[Html]
		,[RowsBkm]
		,[AutoSave]
		,[ViewPermId]
		,[UpdatePermId]
		,[BkmViewRulesId_100]
		,[BkmViewRulesId_200]
		,[BkmViewRulesId_300]
		,[ObligatRulesId]
		,[LabelAlign]
		,[Relation]
		,[Storage]
		,@bIsUnicode
		,[Rows]
		,[NoDefaultClone]
		,[PagingEnabled]
		,[BkmAddRulesId_100]
		,[BkmAddRulesId_200]
		,[BkmAddRulesId_300]
		,[NoDefaultLink_100]
		,[NoDefaultLink_200]
		,[NoDefaultLink_300]
		,[DeleteRulesId]
		,[DefaultLink_100]
		,[DefaultLink_200]
		,[DefaultLink_300]
		,[AutoSelectEnabled]
		,[AutoSelectValue]
		,[LeftMenuLinksPermId]
		,[InterPpNeeded]
		,[InterPmNeeded]
		,[BkmInlineModeEnabled]
		,[NbrBkmInline]
		,[CountOnDemand]
		,[TreatmentMaxRows]
		,[EventAnnexListEnabled]
		,[PmAnnexListEnabled]
		,[PpAnnexListEnabled]
		,[SizeLimit]
		,[Scrolling]
		,[RestrictedSearch]
		,[ColorRulesId]
		,[ViewRulesId]
		,[SavePJ2Text]
		,[InterEventNeeded]
		,[DraftAutoSaveActive]
		,[DraftAutoSaveInterval]
		,[BkmViewPermId_100]
		,[BkmViewPermId_200]
		,[BkmViewPermId_300]
		,[SearchLimit]
		,[ProspectEnabled]
		,[SavePJ2TextField]
		,[BkmPagingMode]
		,[Parameters]
		,[AdvancedSearchDescId]
		,[ComputedColsInList]
		,[ComputedColsInBkm]
		,[ComputedFieldEnabled]
		,[DblWindowDisplayedFields]
		,[DblWindowShowConfidential]
		,[DblWindowHideToolTip]
		,[ActiveScroll]
		,[DisableGroup]
		,[DetailBkmBarButtons]
		,[TreeViewUserList]
		,[FullUserList]
		,[BkmAutoLinkPmToPPEnabled]
		,[ObligatReadOnly]
		,[DisplayFirstBkmOnly]
		,[IntellimailEnabled]
		,[InterEventHidden]
		,[BreakLine]
		,[SAVEBKMPAGING]
		,[SelectionSourceTab]
		,[NoCascadePPPM]
		,[NoCascadePMPP]
		,[AutoCompletion]
		,[IconColor]
	FROM [DESC]
	WHERE [DescId] = @idSubject
END

IF NOT EXISTS (
		SELECT *
		FROM [RES]
		WHERE [ResId] = @idMailTemplatePreHeader
		)
BEGIN
	INSERT INTO [RES] (
		[ResId]
		,[LANG_00]
		,[LANG_01]
		,[LANG_02]
		,[LANG_03]
		,[LANG_04]
		,[LANG_05]
		,[LANG_06]
		,[LANG_07]
		,[LANG_08]
		,[LANG_09]
		,[LANG_10]
		)
	VALUES (
		@idMailTemplatePreHeader
		,'Texte d''aperçu'
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		,NULL /*TODO*/
		)
END
