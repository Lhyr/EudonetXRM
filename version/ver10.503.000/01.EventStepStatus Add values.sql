--Curseur Tables EventStep
declare @tabId as numeric(18)

declare eventStep_cursor cursor for 
select distinct [DescId]
from [DESCADV] 
where [Parameter] = 42
and ltrim(rtrim([Value])) = '1'
and [DescId] % 100 = 0

open eventStep_cursor

fetch next from eventStep_cursor 
into @tabId

while @@FETCH_STATUS = 0
begin
	--traitement
	
	declare @descIdStatus as numeric(18)
	set @descIdStatus = @tabId + 24
	
	--En cours
	if not exists ( select * from [FILEDATA] where [DescId] = @descIdStatus and [Data] = '3')
	begin	
		INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05]
			   ,[Tip_Lang_00_Format]
			   ,[Tip_Lang_01_Format]
			   ,[Tip_Lang_02_Format]
			   ,[Tip_Lang_03_Format]
			   ,[Tip_Lang_04_Format]
			   ,[Tip_Lang_05_Format]
			   ,[Tip_Lang_06_Format]
			   ,[Tip_Lang_07_Format]
			   ,[Tip_Lang_08_Format]
			   ,[Tip_Lang_09_Format]
			   ,[Tip_Lang_10_Format]
		) VALUES (@descIdStatus, '3', 'En cours', 'In progress', 'Laufend', 'Lopend', 'En Curso', 'In corso'
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0)	
	end	
	
	--Terminé
	if not exists ( select * from [FILEDATA] where [DescId] = @descIdStatus and [Data] = '4')
	begin	
		INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05]
			   ,[Tip_Lang_00_Format]
			   ,[Tip_Lang_01_Format]
			   ,[Tip_Lang_02_Format]
			   ,[Tip_Lang_03_Format]
			   ,[Tip_Lang_04_Format]
			   ,[Tip_Lang_05_Format]
			   ,[Tip_Lang_06_Format]
			   ,[Tip_Lang_07_Format]
			   ,[Tip_Lang_08_Format]
			   ,[Tip_Lang_09_Format]
			   ,[Tip_Lang_10_Format]
		) VALUES (@descIdStatus, '4', 'Terminée', '', '', '', '', ''
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
		)	
	end	
	
	--Tentative d'annulation
	if not exists ( select * from [FILEDATA] where [DescId] = @descIdStatus and [Data] = '5')
	begin	
		INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05]
			   ,[Tip_Lang_00_Format]
			   ,[Tip_Lang_01_Format]
			   ,[Tip_Lang_02_Format]
			   ,[Tip_Lang_03_Format]
			   ,[Tip_Lang_04_Format]
			   ,[Tip_Lang_05_Format]
			   ,[Tip_Lang_06_Format]
			   ,[Tip_Lang_07_Format]
			   ,[Tip_Lang_08_Format]
			   ,[Tip_Lang_09_Format]
			   ,[Tip_Lang_10_Format]
		) VALUES (@descIdStatus, '5', 'Tentative d''annulation', '', '', '', '', ''
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
		)	
	end
	
	
	
	fetch next from eventStep_cursor 
    into @tabId
end 
close eventStep_cursor;
deallocate eventStep_cursor;