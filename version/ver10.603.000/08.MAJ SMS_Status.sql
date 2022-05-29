 
 
 declare cursorTables cursor for

	SELECT [tab].[DescId], tab.[file] FROM [DESC] tab WHERE [tab].[TYPE] = 4
				
		
open cursorTables


declare @tabId as numeric(18)

declare @tabname as varchar(500)

declare @ssql as varchar(max)

fetch next from cursorTables 
into @tabId, @tabname

while @@FETCH_STATUS = 0
begin

 set @tabId = @tabId +29

	set @ssql ='update ['+@tabname+'] set TPL29 =
	case 
		when ISNULL(tpl85,0) = 0 then  (select top 1 dataid from FILEDATA where Data=''LM_302001'' and DescId='+cast(@tabId as varchar(10))+')
		 when ISNULL(tpl85,0) = 1 then  (select top 1 dataid from FILEDATA where Data=''LM_302000'' and DescId='+cast(@tabId as varchar(10))+')
		 when ISNULL(tpl85,0) = 4 then  (select top 1 dataid from FILEDATA where Data=''Default'' and DescId='+cast(@tabId as varchar(10))+')
		 else null
	end
	where TPL29 is null
	'
	print @ssql
	exec( @ssql)


		fetch next from cursorTables 
    into @tabId , @tabname
end 
close cursorTables;
deallocate cursorTables;

 