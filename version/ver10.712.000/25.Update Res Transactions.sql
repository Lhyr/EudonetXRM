DECLARE @nTab INT = 119500

IF EXISTS(SELECT ResId FROM [RES] where LANG_00 = 'Date de transaction' and ResId = @nTab + 3) 
BEGIN
UPDATE [RES] SET LANG_01 = 'Date of transaction', LANG_02 = 'Date of transaction',  LANG_03 = 'Date of transaction', LANG_04 = 'Date of transaction' where ResId = @nTab + 3
END

IF EXISTS(SELECT * FROM [RES] where LANG_00 like 'Statut de la transaction' and ResId = @nTab + 5) 
BEGIN
UPDATE [RES] SET LANG_01 = 'Transaction status', LANG_02 = 'Transaction status',  LANG_03 = 'Transaction status', LANG_04 = 'Transaction status' where ResId = @nTab + 5
END

IF EXISTS(SELECT * FROM [RES] where LANG_00 like 'Catégorie du statut' and ResId = @nTab + 9) 
BEGIN
UPDATE [RES] SET LANG_01 = 'Status category', LANG_02 = 'Status category',  LANG_03 = 'Status category', LANG_04 = 'Status category' where ResId = @nTab + 9
END

IF EXISTS(SELECT * FROM [RES] where LANG_00 like 'Statut prestataire' and ResId = @nTab + 10) 
BEGIN
UPDATE [RES] SET LANG_01 = 'Provider status', LANG_02 = 'Provider status',  LANG_03 = 'Provider status', LANG_04 = 'Provider status' where ResId = @nTab + 10
END