
/**
*	RÃ©cupÃ¨re les userdisplayname ou le groupname de la valeur passÃ© en paramÃ¨tre d'une rubrique de type USER
*	SPH & HLA - 10/04/09
*	HLA - 02/10/2015 - ajout des useri et groupid dans la valeur de retour

select * from dbo.[getUserDisplayAdv]('105;1654;65',';')
*/

ALTER FUNCTION [dbo].[getUserDisplayAdv]
(
	@s varchar(max),
	@sep varchar(10)
) 
RETURNS TABLE
as

RETURN
(
	SELECT lib as value
	FROM (
		SELECT SUBSTRING(fd.lib.query('root').value('.', 'nvarchar(max)'), len(@sep)-1, LEN(fd.lib.query('root').value('.', 'nvarchar(max)'))) lib
		FROM (SELECT @s AS userIdList) tab
		CROSS APPLY (
			SELECT distinct display + @sep
			FROM (
					SELECT isnull(userdisplayname,'') + '#$|#$' + cast([userid] as varchar(50)) as display FROM [user]
					WHERE charindex(';' + cast([user].userid as varchar(10)) +';',';' + userIdList +';')>0

					UNION
		
					SELECT isnull(groupname,'') + '#$|#$G' + cast([groupid] as varchar(50)) as display FROM [group]
					WHERE charindex(';G' + cast([group].groupid as varchar(10)) +';',';' + userIdList +';')>0
				) TAB
			ORDER BY display + @sep --charindex(';' + cast(dataid as varchar(10)) +';',';' + FileDataList +';')
				-- On spécifie l'option type pour convertir le xml en varchar
				-- ceci règle le problème des caractères spéciaux.
			FOR XML PATH('')
				,root('root')
				,type
			) fd(lib)
		) Labels
)
