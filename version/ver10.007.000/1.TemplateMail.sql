/* 	GCH - 16/07/2014
 */
 set nocount on;
declare @nTab int,
	@tabName as varchar(100)
	
declare curs cursor for
	SELECT [DescId], [FILE] FROM [DESC] WHERE [TYPE] = 3
open curs
fetch next from curs
	into @nTab, @tabName
while @@FETCH_STATUS = 0
begin
	--GCH - 16/07/2014
	update [desc] set [format] = 21 where descid = @nTab+25
	
	fetch next from curs into @nTab, @tabName
end

close curs
deallocate curs