DECLARE @Table_Name varchar(max), @Command NVARCHAR(max)
DECLARE Tables_Names CURSOR  
    FOR SELECT [File] from [DESC] where DescId like '%00' and [File] like 'EVENT%'
OPEN Tables_Names  
FETCH NEXT FROM Tables_Names
INTO @Table_Name 
  
WHILE @@FETCH_STATUS = 0  
BEGIN

set @Command = 'IF NOT EXISTS (
					  SELECT * 
					  FROM   sys.columns 
					  WHERE  object_id = OBJECT_ID(''[dbo].['+@Table_Name+']'') AND name = ''OnBreak''
					  )
							BEGIN
							ALTER Table [' + @Table_Name + '] 
							Add OnBreak INT NOT NULL DEFAULT 0
							End;'

EXEC sp_executesql @Command

FETCH NEXT FROM Tables_Names
INTO @Table_Name 

END

CLOSE Tables_Names;  
DEALLOCATE Tables_Names; 

