set nocount on;


DECLARE @nTab INT = 119500
begin
    	
	-- LA TABLE PAYMENTTRANSACTION est pour les admins pour l'insant
	-- On recupere la permId si on l'a deja creee	
    DECLARE @VisuPermId INT = 0   
    SELECT @VisuPermId = IsNull([ViewPermId], 0) FROM [DESC] WHERE DescId = @nTab 
	if(@VisuPermId = 0)
	BEGIN
     -- 0:Niveau seulement, 1:Utilisateur seulement, 2:Utilisateur ou Niveau, 3:Utilisateur et Niveau 
     -- que pour les admins  
     -- Id des users séparés par ";"  ex: 19;89;265;185;184	
	 INSERT INTO [Permission] ([Mode], [Level], [User]) values (2, 99, NULL)
	 SELECT @VisuPermId = SCOPE_IDENTITY()
	END

	-- Raz
	DELETE [res] where (resid >= @nTab  AND ResId <  @nTab + 40 ) OR (ResId > @nTab + 70 AND resid < @nTab + 100 )
	DELETE [FileDataParam] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	DELETE [UserValue] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	DELETE [DESC] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	
	
	--  Table
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) 
	SELECT @nTab, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRAN', 0, 0, 0, 1, 25

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab, 'Transactions', 'Transactions', '', '', ''

 
	-- champs fixes
		-- Ref Eudo 
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length],  [obligat], readonly) 
	SELECT @nTab + 1, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANREFEUDO', 1, 40,  1,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab +1 , 'Référence de paiement Eudonet', '', '', '', '' 
	
		-- hostchechoutid
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 2, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANREFPRESTA', 1, 200,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 2, 'Référence de paiement prestataire', '', '', '', '' 
	
 		-- date
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 3, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANDATEPAYMENT', 2, 0,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 3, 'Date de transaction', '', '', '', '' 
 
 		-- montant
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 4, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANAMOUNT', 5, 2 ,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 4, 'Référence de paiement', '', '', '', '' 
	
		-- statut
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 5, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANSTATUS', 10, 0 ,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 5, 'Statut de la transaction', '', '', '', '' 

	-- DescId table destinataire
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab +6, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANTARGETDESCID', 10, 0 ,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 6, 'Table destinataire', '', '', '', '' 
	
		-- File Id  table destinataire
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 7, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANTARGETFILEID', 10, 0,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 7, 'Fiche destinataire', '', '', '', '' 


		-- Informations retours
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [readonly], html) 
	SELECT @nTab + 8, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANRETURNINFOS', 9, 1, 1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 8, 'Informations retours', '', '', '', '' 
	
	
		-- code
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 9, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANCODE', 10, 0 ,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 9, 'Catégorie du statut', '', '', '', '' 

		-- category
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], readonly) 
	SELECT @nTab + 10, @VisuPermId, 'PAYMENTTRANSACTION', 'PAYTRANCATEGORY', 10, 0 ,  1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 
	SELECT @nTab + 10, 'Statut prestataire', '', '', '', '' 
	
	
	-- Champ system
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 84, 'PAYMENTTRANSACTION', 'PAYTRAN84', 3, 0
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 88, 'PAYMENTTRANSACTION', 'PAYTRAN88', 8, 0
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 88, 'Transaction de', 'Transaction of', 'Transaction of', 'Transaction of', 'Transaction of', 'Transaction of'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 91, 'PAYMENTTRANSACTION', 'PAYTRAN91', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01) SELECT @nTab + 91, 'Annexes', 'Annexes'	

	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 95, 'PAYMENTTRANSACTION', 'PAYTRAN95', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 96, 'PAYMENTTRANSACTION', 'PAYTRAN96', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 97, 'PAYMENTTRANSACTION', 'PAYTRAN97', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 98, 'PAYMENTTRANSACTION', 'PAYTRAN98', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 99, 'PAYMENTTRANSACTION', 'PAYTRAN99', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	--Pref/selections
	
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	insert into pref (tab,userid, listcol) select @nTab, 0, '119501'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '119501' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '119501', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '119501', '0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
	
	
	--Mise en page Table WorkflowStep
	update [DESC] set [columns] = '200,500,25' where [DescId] = @nTab  
	update [DESC] set [DispOrder] = 1, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 1 	
	update [DESC] set [DispOrder] = 2, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 2
	update [DESC] set [DispOrder] = 3, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 3
	update [DESC] set [DispOrder] = 4, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 4
	update [DESC] set [DispOrder] = 5, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 5
	update [DESC] set [DispOrder] = 6, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 6
	update [DESC] set [DispOrder] = 7, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 7
	update [DESC] set [DispOrder] = 8, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 8
	update [DESC] set [DispOrder] = 9, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 9
    update [DESC] set [DispOrder] = 10, [Rowspan] = 1, [Colspan] = 1 where [DescId] = @nTab + 10
	
	 
	

END

IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = @nTab ) = 0
BEGIN
--Masquer par défaut la table Transactions
INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (@nTab, 56, 1, 0)

END
	