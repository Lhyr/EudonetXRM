/*    GCH
      Correction du modèle de donnée
      04/11/2013
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
      --MergeFields à 
      update [desc] set [ReadOnly] = 0 where descid = @nTab+25
	  
      fetch next from curs into @nTab, @tabName
end
close curs
deallocate curs

-- Modifié par est nullable !
ALTER TABLE FORMULARXRM ALTER COLUMN ModifiedBy numeric(18,0) NULL 
