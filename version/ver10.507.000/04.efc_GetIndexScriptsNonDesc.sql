 
 
alter FUNCTION [dbo].[efc_GetIndexScriptsNonDesc] (@file varchar(200), @field varchar(200))
RETURNS TABLE
AS
return

SELECT distinct i.name,t.name tname,
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
AND i.name in(		SELECT   i.name 
		FROM sys.index_columns ic
		INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
		INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
		INNER JOIN sys.objects o ON c.object_id = o.object_id

		where o.name collate french_ci_ai = @file  and c.name  collate french_ci_ai = @field)
AND i.type <> 0
 