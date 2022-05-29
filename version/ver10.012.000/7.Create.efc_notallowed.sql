create FUNCTION [dbo].[efc_getNotAllowedField](@nTab as numeric, @sList as varchar(max)) RETURNS table AS  
  return 



select Element  from dbo.[efc_split](@sList,';') 
where element not in 

(
select [desc].[descid] from cfc_getliaison(@nTab)
inner join [desc] on [desc].[file] = relationfile
 where isrelation = 1
union
select [descid] from [desc] where (descid - descid %100)  = @nTab
union
select [descid] from [desc] where descid like '2__' and @nTab = 300
union
select [descid] from [desc] where descid like '3__' and @nTab = 200
union
select [descid] from [desc] where descid like '4__' and (@nTab = 300 Or @nTab = 200)
) 

