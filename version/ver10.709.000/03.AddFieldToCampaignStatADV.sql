SET NOCOUNT ON;

DECLARE @nTab INT = 111100
DECLARE @nDescId INT = 111119

DELETE FROM [DESC] WHERE DESCID = @nDescId
DELETE FROM [RES] WHERE  RESID = @nDescId

INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0, @nDescId % 100, 1, 1
		

INSERT INTO [res] (resid, lang_00, lang_01) SELECT @nDescId, 'Nombre de mail de test analysés','Number of Test emails analysed'


update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' and disporder < 69)  where descid= @nDescId		