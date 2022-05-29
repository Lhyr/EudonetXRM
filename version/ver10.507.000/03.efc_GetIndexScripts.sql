		
/*********************************************************************
	CREATED :	SPH 
	DATE  : 05/11/2019
 
 select * from [dbo].[efc_GetIndex]](3103 )

****************************************************************************/
ALTER FUNCTION [dbo].[efc_GetIndexScripts] (@descid int)
RETURNS TABLE
AS
return

SELECT  i.name,t.name tname,
'IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name=''' +i.name + ''' AND  OBJECT_NAME(object_id) = '''+t.name+''') CREATE ' + 
CASE WHEN is_primary_key=1 THEN 'CLUSTERED' 
WHEN is_primary_key=0 and is_unique_constraint=0 THEN 'NONCLUSTERED'
WHEN is_primary_key=0 and is_unique_constraint=1 THEN 'UNIQUE' END  
+ ' INDEX ' +
QUOTENAME(i.name) + ' ON ' +
QUOTENAME(t.name) + ' ( '  + 
STUFF(REPLACE(REPLACE((
        SELECT QUOTENAME(c.name) + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE '' END AS [data()]
        FROM sys.index_columns AS ic
        INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH
    ), '<row>', ', '), '</row>', ''), 1, 2, '') + ' ) '  -- keycols
+ COALESCE(' INCLUDE ( ' +
    STUFF(REPLACE(REPLACE((
        SELECT QUOTENAME(c.name) AS [data()]
        FROM sys.index_columns AS ic
        INNER JOIN sys.columns AS c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
        ORDER BY ic.index_column_id
        FOR XML PATH
    ), '<row>', ', '), '</row>', ''), 1, 2, '') + ' ) ',    -- included cols
    '') as [Create],
'DROP INDEX ' + QUOTENAME(i.name) + ' ON ' + QUOTENAME(t.name) as [Drop],
'ALTER INDEX ' + QUOTENAME(i.name)  + ' ON ' +QUOTENAME(t.name) + ' REBUILD ' as [Rebuild]
FROM sys.tables AS t
INNER JOIN sys.indexes AS i ON t.object_id = i.object_id

WHERE t.is_ms_shipped = 0
and i.name in ( select idxName from [dbo].[efc_GetIndex](@descid ) )
AND i.type <> 0
 