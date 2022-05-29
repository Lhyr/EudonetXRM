 /**
*	Récupère les valeurs de fin d'arborescence - version fonction de table
*	SPH  - 23/10/2014
*/  
 
 
CREATE  FUNCTION [dbo].[getFiledataArboLastChildrenDisplayAdv]
( 
	@s varchar(max),			-- Valeur (id de la valeur)
	@sLang varchar(20),			-- Langue en cours
	@nDescId int,				-- DescId du catalogue avancé
	@sep varchar(10),			-- Séparateur de chaine
	@display varchar(200)		-- Masque spécifique
)
RETURNS TABLE
as

RETURN
(

	WITH mycte (dataid, data, lib, p)
	as
	(
		select dataid, data,
			case
				when upper(@sLang)='LANG_00' then lang_00
				when upper(@sLang)='LANG_01' then lang_01
				when upper(@sLang)='LANG_02' then lang_02
				when upper(@sLang)='LANG_03' then lang_03
				when upper(@sLang)='LANG_04' then lang_04
				when upper(@sLang)='LANG_05' then lang_05
				when upper(@sLang)='LANG_06' then lang_06
				when upper(@sLang)='LANG_07' then lang_07
				when upper(@sLang)='LANG_08' then lang_08
				when upper(@sLang)='LANG_09' then lang_09
				else lang_00
			end, cast(
			case 
				when upper(@sLang)='LANG_00' then lang_00
				when upper(@sLang)='LANG_01' then lang_01
				when upper(@sLang)='LANG_02' then lang_02
				when upper(@sLang)='LANG_03' then lang_03
				when upper(@sLang)='LANG_04' then lang_04
				when upper(@sLang)='LANG_05' then lang_05
				when upper(@sLang)='LANG_06' then lang_06
				when upper(@sLang)='LANG_07' then lang_07
				when upper(@sLang)='LANG_08' then lang_08
				when upper(@sLang)='LANG_09' then lang_09
				else lang_00
			end as varchar(max) )
		from filedata
		where descid = @nDescId and parentdataid is null and charindex(';'+cast(dataid as varchar(10))+';', ';'+@s+';') > 0

		UNION ALL

		select enf.dataid, enf.data, 
			case
				when upper(@sLang)='LANG_00' then enf.lang_00
				when upper(@sLang)='LANG_01' then enf.lang_01
				when upper(@sLang)='LANG_02' then enf.lang_02
				when upper(@sLang)='LANG_03' then enf.lang_03
				when upper(@sLang)='LANG_04' then enf.lang_04
				when upper(@sLang)='LANG_05' then enf.lang_05
				when upper(@sLang)='LANG_06' then enf.lang_06
				when upper(@sLang)='LANG_07' then enf.lang_07
				when upper(@sLang)='LANG_08' then enf.lang_08
				when upper(@sLang)='LANG_09' then enf.lang_09
				else enf.lang_00
			end, cast(mycte.p + 
			case when isnull( 
				case
					when upper(@sLang)='LANG_00' then enf.lang_00
					when upper(@sLang)='LANG_01' then enf.lang_01
					when upper(@sLang)='LANG_02' then enf.lang_02
					when upper(@sLang)='LANG_03' then enf.lang_03
					when upper(@sLang)='LANG_04' then enf.lang_04
					when upper(@sLang)='LANG_05' then enf.lang_05
					when upper(@sLang)='LANG_06' then enf.lang_06
					when upper(@sLang)='LANG_07' then enf.lang_07
					when upper(@sLang)='LANG_08' then enf.lang_08
					when upper(@sLang)='LANG_09' then enf.lang_09
					else enf.lang_00
				end ,'') = '' and isnull(enf.data,'') = '' then null
			else




				COALESCE(mycte.p + @sep, '') +  replace(replace(replace(replace(replace(@display, '[TEXT]', isnull(
				case
					when upper(@sLang)='LANG_00' then enf.lang_00
					when upper(@sLang)='LANG_01' then enf.lang_01
					when upper(@sLang)='LANG_02' then enf.lang_02
					when upper(@sLang)='LANG_03' then enf.lang_03
					when upper(@sLang)='LANG_04' then enf.lang_04
					when upper(@sLang)='LANG_05' then enf.lang_05
					when upper(@sLang)='LANG_06' then enf.lang_06
					when upper(@sLang)='LANG_07' then enf.lang_07
					when upper(@sLang)='LANG_08' then enf.lang_08
					when upper(@sLang)='LANG_09' then enf.lang_09
					else enf.lang_00
				end
				,'')), '[DATA]', isnull(enf.data,'')), '+''', ''), '''+', ''), '''', '')
			end as varchar(max))
		from filedata as enf inner join mycte on mycte.dataid = enf.parentdataid
		where descid = @nDescId and charindex(';'+cast(enf.dataid as varchar(10))+';',';' + @s + ';')>0
	)


	select ( rtrim(ltrim(stuff((
		select @sep + case when isnull(lib,'') = '' and isnull(data,'') = '' then ''
			else 

 replace(replace(replace(
replace(replace(@display, '[TEXT]', isnull(lib,'')), '[DATA]', isnull(data,''))
, '+''', ''), '''+', ''), '''', '')
 end
		from mycte
		where mycte.dataid in (
			SELECT child.[DataId] child_dataid FROM [FileData] child 
			left join [FileData] as p on child.[parentdataid] = p.[DataId]
			where  child.descid = @nDescId And p.DescId = @nDescId and charindex(';'+cast(child.[DataId] as varchar(10))+';',';' + @s + ';')>0
			 and (child.[DataId] not in (
				select p.dataid from [FileData] p
				left join [FileData] as child on child.[parentdataid] = p.[DataId]
				where charindex(';'+cast(child.[DataId] as varchar(10))+';',';' + @s + ';')>0
			))
		)
			order by mycte.p
		FOR XML PATH('')),1,len(@sep),'')
	))) + '#$|#$' + ( rtrim(ltrim(stuff((
		select @sep + cast(dataid as varchar(10))
		from mycte
		where mycte.dataid in (
			SELECT child.[DataId] child_dataid FROM [FileData] child 
			left join [FileData] as p on child.[parentdataid] = p.[DataId]
			where  child.descid = @nDescId And p.DescId = @nDescId and charindex(';'+cast(child.[DataId] as varchar(10))+';',';' + @s + ';')>0
			 and (child.[DataId] not in (
				select p.dataid from [FileData] p
				left join [FileData] as child on child.[parentdataid] = p.[DataId]
				where charindex(';'+cast(child.[DataId] as varchar(10))+';',';' + @s + ';')>0
			))
		)
		order by mycte.p
		
		FOR XML PATH('')),1,len(@sep),'')
	))) as res 
)
