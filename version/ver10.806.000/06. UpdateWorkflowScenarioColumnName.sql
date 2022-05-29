DECLARE @nTab INT = 119200 
BEGIN

	UPDATE [res] Set lang_00 = 'Onglet Campagne' WHERE resid = @nTab + 4	
	
	UPDATE [res] Set lang_00 = 'Onglet Destinataires' WHERE resid = @nTab + 5
	
END