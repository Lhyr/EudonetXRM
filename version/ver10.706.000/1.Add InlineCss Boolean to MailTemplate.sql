--Tâche 4 705
--Ajout des colonnes dans la table MAILTEMPLATE
DECLARE @idISHTML INT = 107004
DECLARE @idMailTemplateInlineCSS INT = 107013

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[MAILTEMPLATE]') 
         AND name = 'InlineCss'
)
BEGIN
Alter Table [MAILTEMPLATE] 
Add InlineCss bit Not NULL DEFAULT 1;
end

IF NOT EXISTS (
		SELECT *
		FROM [DESC]
		WHERE [DescId] = @idMailTemplateInlineCSS
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
	SELECT @idMailTemplateInlineCSS
		,[File]
		,'InlineCss'
		,3
		,0
		,[Case]
		,''
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
	FROM [DESC]
	WHERE [DescId] = @idISHTML
END

IF NOT EXISTS (
		SELECT *
		FROM [RES]
		WHERE [ResId] = @idMailTemplateInlineCSS
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
		@idMailTemplateInlineCSS
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




-- Ajout de inlinecss dans campaign
IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'InlineCss'
)
BEGIN
Alter Table [CAMPAIGN] 
Add InlineCss bit Not NULL DEFAULT 1;
end
