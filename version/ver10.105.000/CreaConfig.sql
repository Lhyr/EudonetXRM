/*
GCH 20150409 - Il manque les information de config sur edn_formular
*/
declare @nUId int
declare curs cursor for
	select userid from [USER] where UserId not in (SELECT [userid] from [config])
open curs
fetch next from curs into @nUId
while @@FETCH_STATUS = 0
begin
	exec esp_creaPref @nUId,0	
	fetch next from curs into @nUId
end
close curs
deallocate curs
