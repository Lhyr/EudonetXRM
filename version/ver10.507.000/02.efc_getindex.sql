
		
/*********************************************************************
	CREATED :	SPH 
	DATE  : 05/11/2019
 
 select * from [dbo].[efc_GetIndex]](3103 )

****************************************************************************/
ALTER FUNCTION [dbo].[efc_GetIndex] (@descid int)
RETURNS TABLE
AS

return 
		SELECT  descid,i.name idxName, o.name tname, c.name cname, is_included_column 
		FROM sys.index_columns ic
		INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
		INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
		INNER JOIN sys.objects o ON c.object_id = o.object_id

		inner join [DESC] d on d.[File] collate french_ci_ai = o.name  collate french_ci_ai and d.[Field] collate french_ci_ai = c.name collate french_ci_ai

		where DescId = @descid