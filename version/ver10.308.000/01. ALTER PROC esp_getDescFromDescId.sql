/*
*	Retourne la description des rubrique
*	SPH & HLA - CREATION - 10/04/09
*	HLA - MODIF - 17/03/2017
*/
ALTER PROCEDURE [dbo].[esp_getDescFromDescId]
	@Descid numeric,	-- DescId
	@UserId numeric		-- userid
AS 

declare @ulevel int
declare @groupid int 
declare @langid int 
declare @sLang varchar(20)

declare @sLib nvarchar(max)

select @ulevel = userlevel, @groupid= groupid, @sLang = upper(isnull(lang,'LANG_00')) from [user] where userid = @UserId

-- Libelle de la rubrique
select @sLib = case
	when @sLang='LANG_00' then lang_00
	when @sLang='LANG_01' then lang_01
	when @sLang='LANG_02' then lang_02
	when @sLang='LANG_03' then lang_03
	when @sLang='LANG_04' then lang_04
	when @sLang='LANG_05' then lang_05
	when @sLang='LANG_06' then lang_06
	when @sLang='LANG_07' then lang_07
	when @sLang='LANG_08' then lang_08
	when @sLang='LANG_09' then lang_09
	else lang_00 end
from [res] where [res].[resid] = @Descid

declare @idlang as int
set @idlang = cast( replace(@slang,'lang_','') as int)

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
	case 
			when P_VIEW.mode = 0 then case when @ulevel >= P_VIEW.level then 1 else 0 end
			when P_VIEW.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 then 1 else 0 end
			when P_VIEW.mode = 3 then case when (@ulevel >= P_VIEW.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
			when P_VIEW.mode = 2 then case when (@ulevel >= P_VIEW.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
			else 1
	end VIEW_P,
	case 
			when P_UP.mode = 0 then case when @ulevel >= P_UP.level then 1 else 0 end
			when P_UP.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0 then 1 else 0 end
			when P_UP.mode = 3 then case when (@ulevel >= P_UP.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0) then 1 else 0 end
			when P_UP.mode = 2 then case when (@ulevel >= P_UP.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_UP.[user] +';') > 0) then 1 else 0 end
			else 1
	end UPDATE_P, 
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
	ISNULL(CANCELLASTVALUE,'') AS CancelLastValueAllowed
	
from [desc]
	inner join [res] on [desc].[descid] = [res].[resid]
	left join [FILEDATAPARAM] fdp on [desc].[descid] = fdp.[descid]
	left join [PERMISSION] AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid]
	left join [permission] AS P_UP ON [desc].[updatepermid] = P_UP.[permissionid] 
	left join 
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
				 FROM [DESC]
			LEFT JOIN (
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
						)) AS [DESCADVPIVOT] ON [DESCADVPIVOT].DescId = [desc].DescId
			WHERE [DESC].[DESCID] = @descid
		) AS DESCP ON DESCP.DESCID = [DESC].[DESCID]

where [desc].[descid] = @Descid
