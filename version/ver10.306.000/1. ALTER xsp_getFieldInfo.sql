/*
*	Retourne les info du field
*	HLA - 14/03/17
*/

ALTER PROCEDURE [dbo].[xsp_getFieldInfo]
	@Descid numeric		-- DescId
AS 

select isnull([relation], 0) [relation], isnull([storage], 0) [storage], isnull([format], 0) as [format],
	[field], isnull([popup],0) as [popup], isnull([popupdescid], 0) as [popupdescid],
	isnull([multiple], 0) as [multiple], [unicode], 
	[viewrulesid], [changerulesid], [viewpermid], [updatepermid], [obligatrulesid], 
	isnull([bounddescid], 0) as [bounddescid], 
	(select case when T.type = 0 then 1 else 0 end
		from [desc] T
		where isnull([desc].[popupdescid],0) like '%01' and isnull([desc].[popupdescid],0) <> [desc].[descid]
			and isnull([desc].[popup],0) = 2 and T.[descid] = isnull([desc].[popupdescid],0) - isnull([desc].[popupdescid],0)%100
	) as specialpopup,
	rowspan, colspan, disporder, [default], isnull([defaultformat],0) as [defaultformat], 
	-- DESCADV_PARAMETER.ALIASSOURCE = 10
	(select top 1 Value from DESCADV where Parameter = 10 and DescId = @Descid) as [aliasparam]
from [desc]
where descid = @Descid
