/*
*	Retourne la description des rubrique
*	SPH & HLA - 10/04/09
*/

ALTER PROCEDURE [dbo].[esp_getDescFromDescId]
	@Descid numeric,		-- DescId
	@UserId numeric	-- userid
AS 

declare @ulevel int
declare @groupid int 
declare @sLang varchar(20)

declare @sLib nvarchar(max)

select @ulevel = userlevel, @groupid= groupid, @sLang = upper(lang) from [user] where userid = @UserId

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

select @sLib as libelle,[desc].[file],[desc].[ToolTipText],[desc].[case],[desc].[TreeViewUserList],[desc].[FullUserList],
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
	(select count(1) from [UserValue] AS MF where isnull(MF.[userid],0) = 0 and MF.[Type] = 6 and MF.[DescId] = @Descid And Isnull(MF.value,'')<>'') MF
	,isnull([DESC].[InterEventHidden],0), disporder, [default],[defaultformat], [Bold], [Italic], [Underline], [ForeColor], [Flat]
from [desc] inner join [res] 
		on [desc].[descid] = [res].[resid] 
		LEFT JOIN [PERMISSION] AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid] 
		LEFT JOIN [permission] AS P_UP ON [desc].[updatepermid] = P_UP.[permissionid] 
where [desc].[descid] = @Descid order by libelle