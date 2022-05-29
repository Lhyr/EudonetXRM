DECLARE @Table_Name varchar(max), @Command NVARCHAR(max)
DECLARE Tables_Names CURSOR  
    FOR SELECT [File] 
  FROM [DESC] where DescId in (select DescId from [DESCADV] where Parameter=42)
OPEN Tables_Names  
FETCH NEXT FROM Tables_Names
INTO @Table_Name 
  
WHILE @@FETCH_STATUS = 0  
BEGIN

set @Command ='IF EXISTS (
					  SELECT * 
					  FROM   sys.columns 
					  WHERE  object_id = OBJECT_ID(''[dbo].['+@Table_Name+']'') AND name = ''EVT24''
					  )
							BEGIN
							UPdate [' + @Table_Name + ']  SET [EVT24] = 0 where [EVT24] is null
							End;'

 

EXEC sp_executesql @Command

FETCH NEXT FROM Tables_Names
INTO @Table_Name 

END

CLOSE Tables_Names;  
DEALLOCATE Tables_Names; 