
declare @name as varchar(1000)


declare delidx cursor for
	SELECT DISTINCT i.name a
		FROM sys.index_columns ic
		INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
		INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
		INNER JOIN sys.objects o ON c.object_id = o.object_id
		WHERE object_name(ic.object_id) collate french_ci_ai = 'FILEMAP_EXCHANGE' collate french_ci_ai
 
		
 
 open delidx


  FETCh delidx into @name
 WHILE @@FETCH_STATUS = 0
BEGIN
	execute(  'DROP INDEX [' +@name +'] ON [dbo].[filemap_exchange]')
	 FETCh delidx into @name
END

close delidx
deallocate delidx