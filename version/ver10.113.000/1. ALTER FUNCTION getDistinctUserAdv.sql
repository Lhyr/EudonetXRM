
/**
*	Récupère les userdisplayname ou le groupname de la valeur passés en paramètre d'une rubrique de type USER
*	SPH & HLA - 10/04/09
*	41798 CRU 14/10/15 : On ne tient pas compte de UserHidden
*/
ALTER FUNCTION [dbo].[getDistinctUserAdv]
(
	@sValue varchar(max),
	@sep varchar(10)
)
RETURNS TABLE
as

RETURN
(
	SELECT RIGHT(o.list, LEN(o.list)-1) as [Value]
	FROM (select @sValue as UsersList) tab
	CROSS APPLY
	(
		select @sep + isnull(TAB.display,'')
		from (
			select cast(userid as varchar(20)) as display, isnull(userdisplayname, '') as UserDisplayName from [user]
			where charindex(';' + cast(userid as varchar(10)) +';',';' + UsersList +';')>0 --and IsNull([UserHidden],0) = 0
			
			union
			
			select cast(userid as varchar(20)) as display, isnull(userdisplayname, '') as UserDisplayName from [group]
			inner join [user] on [user].[groupid] = [group].[groupid]
			where charindex(';G' + cast([group].groupid as varchar(10)) +';',';' + UsersList +';')>0 --and IsNull([UserHidden],0) = 0
		) TAB
		ORDER BY TAB.UserDisplayName

		FOR XML PATH('')
	) o (list)
)
