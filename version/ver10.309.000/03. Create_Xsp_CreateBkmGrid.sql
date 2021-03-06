-- MOU le 05/09/2017
-- Procédure permettant de créer un signet grille. Le signet est unique pour un  onglet (PP XOR PM XOR EVENT XOR EVENT_XXX)
-- @parentTab numeric : descid de la table parente sauf les templates (PP, PM, EVENT, EVENT_XXX)                     
-- @bkmlabel varchar(255) : Libelle du signet en Lang_00 
-- @newBkmTab numeric Output : Nouveau descid de la table template créé	
/*
Ex:
declare @descid numeric
exec xsp_CreateBkmGridSpecif @200, 'Signet Grille PP', @descid Output
print @bkmtab
*/
CREATE PROC [dbo].[xsp_CreateBkmGridSpecif] @parentTab numeric, @bkmlabel varchar(255),	@descid numeric Output								
AS
BEGIN
    
	-- pas de signet grille sur address
	if(@parentTab = 400) 
		RETURN
			
	--  pas de signet grille si le fichier n'est pas principal
	if(not exists(select * from [desc] where  descid = @parentTab and [type] = 0)) 
		RETURN
	
	-- Liaison avec PP
	declare @interpp bit = 0             
    if(@parentTab = 200)
		set @interpp = 1	
		
	-- XOR Liaison avec PM
    declare @interpm bit = 0
	if(@parentTab = 300)
		set @interpm = 1
	
	-- Pas besoin de @parentTab dans ces cas
	if(@interpm = 1 or @interpp = 1)
		set @parentTab = 0	
	
	--  24 type grille		
	EXEC esp_CreateVirtualTemplate @interpp, @interpm, @parentTab, 24, @bkmlabel, NULL, @descid Output

	-- Pas de template de créer
	IF ISNULL(@descid,0) = 0 
		RETURN

	 -- Désactivation du mode Onglet
	Update [DESC] set ActiveTab = 0 where DescId  =  @descid

	-- Activation du mode signet
	Update [PREF] set ActiveBkm = 1 Where Tab = @descid AND UserId = 0 

END;


			