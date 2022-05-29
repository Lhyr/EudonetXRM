DECLARE @nTab INT = 119500

IF  EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[PAYMENTTRANSACTION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Réf transaction' and ResId = @nTab + 1) 
begin
DELETE [res] where ResId = @nTab + 1
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 1, 'Réf transaction', 'Ref transaction', 'Ref transaction', 'Ref transaction', 'Ref transaction'
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Réf prestataire' and ResId = @nTab + 2) 
begin
DELETE [res] where ResId = @nTab + 2
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
SELECT @nTab + 2 , 'Référence de paiement prestataire', 'Provider payment reference', 'Provider payment reference', 'Provider payment reference', 'Provider payment reference' 
END	

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Date de transaction' and ResId = @nTab + 3) 
begin
DELETE [res] where ResId = @nTab + 3
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 3, 'Date de transaction', 'Date of transaction', 'Date of transaction', 'Date of transaction', 'Date of transaction' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Montant' and  ResId = @nTab + 4) 
begin
DELETE [res] where ResId = @nTab + 4
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 4, 'Montant', 'Amount', 'Amount', 'Amount', 'Amount' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Statut de la transaction' and  ResId = @nTab + 5) 
begin
DELETE [res] where ResId = @nTab + 5
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 5, 'Statut de la transaction', 'Transaction status', 'Transaction status', 'Transaction status', 'Transaction status' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Onglet' and ResId = @nTab + 6) 
begin
DELETE [res] where ResId = @nTab + 6
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 6, 'Onglet', 'Tab', 'Tab', 'Tab', 'Tab'  
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'ID de la fiche' and ResId = @nTab + 7) 
begin
DELETE [res] where ResId = @nTab + 7
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 7, 'ID de la fiche', 'Record Id', 'Record Id', 'Record Id', 'Record Id' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Infos retour paiement' and ResId = @nTab + 8) 
begin
DELETE [res] where ResId = @nTab + 8
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 8, 'Infos retour paiement', 'Return payment info', 'Return payment info', 'Return payment info', 'Return payment info' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Catégorie du statut' and  ResId = @nTab + 9) 
begin
DELETE [res] where ResId = @nTab + 9
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 9, 'Catégorie du statut', 'Status category', 'Status category', 'Status category', 'Status category' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Statut prestataire' and ResId = @nTab + 10) 
begin
DELETE [res] where ResId = @nTab + 10
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 10, 'Statut prestataire', 'Provider status', 'Provider status', 'Provider status', 'Provider status'
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Statut du paiement' and ResId = @nTab + 11) 
begin
DELETE [res] where ResId = @nTab + 11
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 11, 'Statut du paiement', 'Payment Status', 'Payment Status', 'Payment Status', 'Payment Status' 
END

IF  NOT EXISTS(SELECT * FROM Res where lang_00 = 'Transaction de' and ResId = @nTab + 88) 
begin
DELETE [res] where ResId = @nTab + 88
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 88, 'Transaction de', 'Transaction of', 'Transaction of', 'Transaction of', 'Transaction of' 
END

END