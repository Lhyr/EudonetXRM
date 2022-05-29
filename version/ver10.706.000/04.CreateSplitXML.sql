CREATE  FUNCTION [dbo].[efc_splitxml](@sChaine as varchar(max), @sep as varchar(5)  ) RETURNS table AS 
 return 

SELECT Split.a.value('.', 'NVARCHAR(MAX)') [value]
FROM
(
    SELECT CAST('<X>'+REPLACE(@sChaine, @sep, '</X><X>')+'</X>' AS XML) AS String
) AS A
CROSS APPLY String.nodes('/X') AS Split(a);
