 
/*
*	Retourne la description d'une rubriques 
*	HLA - CREATION - 10/04/09
*/
ALTER PROCEDURE [dbo].[xsp_getDescFromDescId]
	@DescId numeric,		-- DescId
	@UserId numeric,		-- UserId
	@GroupId int,			-- GroupId
	@UserLevel int,			-- UserLevel
	@Lang varchar(20)		-- Lang
AS 

declare @sLib nvarchar(max)
declare @idlang as int

set @idlang = cast(replace(lower(@Lang),'lang_','') as int)

-- Libelle de la rubrique
select @sLib = RES from dbo.efc_getResid(@Lang, @Descid)

declare @tooltip as nvarchar(max)
select top  1 @tooltip=lang from resadv where descid=@descid and [type] = 2 and [id_lang] = @idlang

declare @watermark as nvarchar(max)
select top  1 @watermark=lang from resadv where descid=@descid and [type] = 1 and [id_lang] =@idlang

select @sLib as libelle,[desc].[file],
	isnull(@tooltip,ToolTipText) as tooltiptext,
	[desc].[case],[desc].[TreeViewUserList],[desc].[FullUserList],[desc].[AutoCompletion],
	[desc].[length],[desc].[BoundDescid],cast(isnull([desc].[ReadOnly],0) as numeric) as ReadOnly,[desc].[ExternalDataSource],[desc].[Formula],
	[desc].[rowspan], [desc].[colspan], [desc].[html],isnull(ComputedFieldEnabled,0) ComputedFieldEnabled,
	cast(isnull([desc].[Obligat],0) as numeric) as [Obligat], cast(isnull([DESC].[NoDefaultClone],0)as numeric) as [NoDefaultClone],
	ISNULL([desc].[ProspectEnabled],0) as [ProspectEnabled],
	
	ISNULL(P_VIEW.P,1)  VIEW_P,	 	
	ISNULL(P_UP.P,1) UPDATE_P, 

	(select count(1) from [UserValue] AS UV where isnull(UV.[userid],0) = 0 and UV.[Type] = 4 and UV.[DescId] = @Descid) UserValueExist,
	(select count(1) from [UserValue] AS MF where isnull(MF.[userid],0) = 0 and MF.[Type] = 6 and MF.[DescId] = @Descid And Isnull(MF.value,'')<>'') MF,
	ISNULL([DESC].[InterEventHidden],0), disporder, X, Y, [default],[defaultformat], [Bold], [Italic], [Underline], [ForeColor], [Flat], [Icon], [IconColor],
	ISNULL([DESC].[TabIndex], 0) AS [TabIndex],	
	ISNULL(fdp.SequenceMode,0) AS SequenceMode, ISNULL(fdp.SelectedValueColor,'') AS SelectedValueColor,
	
	ISNULL(@watermark,'') AS WaterMark,
	
	ISNULL(SUPERADMINONLY,'0') AS SuperAdminOnly,
	ISNULL(FIELDSTYLE,'') AS FieldStyle,
	ISNULL(VALUECOLOR,'') AS ValueColor,
	ISNULL(LABELHIDDEN,'') AS LabelHidden,
	ISNULL(MAXIMIZEVALUE,'') AS MaximizeValue,
	ISNULL(BUTTONCOLOR,'') AS ButtonColor,
	ISNULL(ROOTURL,'') AS RootURL,
	ISNULL(DISPLAYINACTIONBAR,'') AS DisplayInActionBar,
	ISNULL(ASSOCIATEFIELD,'') AS AssociateField,
	ISNULL(RELATIONSOURCE,'') AS RelationSource,
	ISNULL(CANCELLASTVALUE,'') AS CancelLastValueAllowed,
	ISNULL(EMAILSUGGESTIONS,'') AS EmailSuggestionsEnabled
	
FROM [DESC]
	INNER JOIN [RES] ON [desc].[descid] = [res].[resid]
	LEFT JOIN  [FILEDATAPARAM] fdp on [desc].[descid] = fdp.[descid]
	LEFT JOIN  dbo.cfc_getPermInfo(@UserId, @UserLevel, @GroupId)  AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid]
	LEFT JOIN  dbo.cfc_getPermInfo(@UserId, @UserLevel, @GroupId)  AS P_UP  ON [desc].[updatepermid] = P_UP.[permissionid]
	LEFT JOIN  
		(
			SELECT [DESCADVPIVOT].[DESCID]
				,[0] AS 'ALL'
				,[1] AS 'SUPERADMINONLY'
				,[2] AS 'FIELDSTYLE'
				,[3] AS 'VALUECOLOR'
				,[4] AS 'LABELHIDDEN'
				,[5] AS 'BUTTONCOLOR'
				,[6] AS 'XRMONLY'
				,[7] AS 'MAXIMIZEVALUE'
				,[8] AS 'NOAMDMIN'
				,[11] AS 'ROOTURL'
				,[12] AS 'DISPLAYINACTIONBAR'
				,[17] AS 'ASSOCIATEFIELD'
				,[18] AS 'CANCELLASTVALUE'
				,[19] AS 'RELATIONSOURCE'
				,[39] AS 'EMAILSUGGESTIONS'
				 FROM [DESC]
			LEFT JOIN  (
				SELECT [DESCID]
					,[PARAMETER]
					,[VALUE]
				FROM [DESCADV]
				WHERE [DESCID]   = @descid
				) R
			PIVOT(max(value) FOR parameter IN (
						[0]
						,[1]
						,[2]
						,[3]
						,[4]
						,[5]
						,[6]
						,[7]
						,[8]
						,[11]
						,[12]
						,[17]
						,[18]
						,[19]
						,[39]
						)) AS [DESCADVPIVOT] ON [DESCADVPIVOT].DescId = [desc].DescId
			WHERE [DESC].[DESCID] = @descid
		) AS DESCP ON DESCP.DESCID = [DESC].[DESCID]

where [desc].[descid] = @Descid
