/*    Mad
      Correction de la res mail
      04/11/2013
*/
set nocount on;
declare @nTab int
      
declare curs cursor for
      SELECT [DescId] FROM [DESC] WHERE [TYPE] = 3
open curs
fetch next from curs
      into @nTab
while @@FETCH_STATUS = 0
begin     
  if(exists(select * from res where resid = @nTab + 27))
  begin
	update [RES] set [LANG_00] = 'Répondre à' where resid = @nTab + 27
  end	  
  fetch next from curs into @nTab
end
close curs
deallocate curs

