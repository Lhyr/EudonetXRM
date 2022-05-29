
/*
*	Retourne la description de visu de la table
*	HLA - Creation - 04/10/11
*	JBE - Modif - 14/11/11
*	HLA - Modif - 15/11/11
*	MAB - Reprise du tri de JBE par Libellé après correctif dans le code XRM
*/

ALTER PROCEDURE [dbo].[xsp_getTabViewRight]
	@Descid varchar(MAX),	-- Liste des DescId de la table
	@UserId numeric			-- UserId
AS 


declare @uLevel int
declare @uGroupId int 
declare @sLang varchar(200)

select @uLevel = userLevel, @uGroupId = groupId, @sLang = upper(lang) from [user] where userid = @UserId

select [desc].[DescId], [desc].[Disabled], 
	case
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
	as Libelle,
	/* Droits de visu */
	case 
			when P_VIEW.mode = 0 then case when @uLevel >= P_VIEW.level then 1 else 0 end
			when P_VIEW.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+cast( @uGroupId as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 then 1 else 0 end
			when P_VIEW.mode = 3 then case when (@uLevel >= P_VIEW.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @uGroupId as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
			when P_VIEW.mode = 2 then case when (@uLevel >= P_VIEW.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @uGroupId as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
			else 1
	end VIEW_P
from [desc]
	INNER JOIN [res] on [desc].[descid] = [res].[resid] 
	LEFT JOIN [PERMISSION] AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid] 
where charindex(';' + cast([desc].[DescId] as varchar(20)) + ';', ';' + @Descid + ';') > 0
--order by charindex(';' + cast([desc].[DescId] as varchar(20)) + ';', ';' + @Descid + ';')
order by Libelle