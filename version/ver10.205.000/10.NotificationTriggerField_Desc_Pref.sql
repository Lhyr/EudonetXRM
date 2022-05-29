set nocount on;


DECLARE @nTab INT = 114200 
begin
    	
	-- LA TABLE NotificationTrigger est pour les admins
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

	DELETE [res] where resid >= @nTab AND resid < @nTab + 100
	DELETE [FileDataParam] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [UserValue] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [DESC] where DESCid >=@nTab AND DESCid < @nTab + 100
	
	
	-- NotificationTrigger
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) SELECT @nTab, @VisuPermId, 'NotificationTrigger', 'NotificationTrigger', 0, 0, 0, 1, 25
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab, 'Déclencheurs de notifications', 'Notification Triggers', 'Notification Triggers', 'Notification Triggers', 'Notification Triggers'

	-- Libellé
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 1, 'NotificationTrigger', 'Label', 1, 1000
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 1, 'Libellé', 'Label', 'Label', 'Label', 'Label'
	   
	-- Icon
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 8, 'NotificationTrigger', 'Icon', 1, 25,'Image Source : Pictogramme[[BR]]Par défaut pictogramme de l''onglet'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 8, 'Icône', 'Icon', 'Icon', 'Icon', 'Icon'

    -- Color
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 80, 'NotificationTrigger', 'Color', 1, 20,'Image Source : Pictogramme[[BR]]Par défaut pictogramme de l''onglet'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 80, 'Couleur', 'Color', 'Color', 'Color', 'Color'

	-- Image
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 9, 'NotificationTrigger', 'Image', 10, 0,'DescId de la rubrique de type Image[[BR]]Par défaut pictogramme de l''onglet'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 9, 'Champ image', 'Image field', 'Image field', 'Image field', 'Image field'
	
	-- ImageSource
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 7, 'NotificationTrigger', 'ImageSource', 10, 0, '0- Pictogramme (Icône + Coleur) [[BR]]1- Avatar utilisateur lié à la fiche référence [[BR]]2- Avatar utilisateur connecté déclenchant la notification [[BR]]3- Image présente dans la fiche référence (Champ image)[[BR]]Par défaut pictogramme de l''onglet'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 7, 'Image source', 'source image', 'source image', 'source image', 'source image'
	
    -- TriggerAction
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 10, 'NotificationTrigger', 'TriggerAction', 10, 0,'0- Déclencheur désactivé [[BR]]1- Déclenchement sur une création [[BR]]2- Déclenchement sur une modification [[BR]](Somme des valeurs)- Déclenchement sur plusieurs évènements ex:(1+2)=3, déclenchement sur une création OU une modification'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 10, 'Evènement déclencheur', 'Trigger events', 'Trigger events', 'Trigger events', 'Trigger events'

    -- TriggerTargetDescid
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 11, 'NotificationTrigger', 'TriggerTargetDescid', 10, 0, 'DescId de la rubrique sur laquelle s''abonner'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 11, 'Champ déclencheur', 'Table/field trigger', 'Table/field trigger', 'Table/field trigger', 'Table/field trigger'
	
	-- ConditionalFilterIdTrigger
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 12, 'NotificationTrigger', 'ConditionalFilterIdTrigger', 10, 0, 'L''ID du filtre vérifiant la condition de déclenchement de la notification'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 12, 'Déclenchement basé sur un filtre', 'Trigger base on filter', 'Trigger base on filter', 'Trigger base on filter', 'Trigger base on filter'
		
	-- ConditionalSqlTrigger
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 13, 'NotificationTrigger', 'ConditionalSqlTrigger', 1, 1000,'en cours'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 13, 'Déclenchement basé sur une condition SQL', 'Trigger based on SQL condition', 'Trigger based on SQL condition', 'Trigger based on SQL condition', 'Trigger based on SQL condition'
		
	-- NotificationPriority
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 15, 'NotificationTrigger', 'NotificationPriority', 10, 0, '1- faible [[BR]]2- Moyenne [[BR]]3- Haute'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 15, 'Priorité', 'Priority', 'Priority', 'Priority', 'Priority'
	
	-- NotificationType
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 17, 'NotificationTrigger', 'NotificationType', 10, 0, '0- A la validation [[BR]]1- Programmée(en cours) [[BR]]2- Rappels planning'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 17, 'Type de notification', 'Notification type', 'Notification type', 'Notification type', 'Notification type'

	-- BroadcastType
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 18, 'NotificationTrigger', 'BroadcastType', 10, 0, '0- Notifications créées mais pas diffusées (en cours) [[BR]]1- Notification XRM  [[BR]]2- Notifications Mobile (en cours) [[BR]]4- Notifications Mail [[BR]]8- Notifications SMS (en cours) [[BR]](Somme des valeurs)- Diffusion multiple ex:(1+4)=5, diffusion xrm ET mail'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 18, 'Mode de diffusion', 'Broadcast type', 'Broadcast type', 'Broadcast type', 'Broadcast type'

	-- ExpirationDate
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 19, 'NotificationTrigger', 'ExpirationDate', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 19, 'Date d''expiration', 'Expiration date', 'Expiration date', 'Expiration date', 'Expiration date'

    -- TriggerDate
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 20, 'NotificationTrigger', 'TriggerDate', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 20, 'Date de déclenchement', 'Notification date', 'Notification date', 'Notification date', 'Notification date'
			
	-- Subscribers
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [multiple], [ToolTipText]) SELECT @nTab + 21, 'NotificationTrigger', 'Subscribers', 8, 512, 1, 'Abonnés à la notification en cours'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 21, 'Abonnés', 'Subscribers', 'Subscribers', 'Subscribers', 'Subscribers'
		
	-- SubscribersFreeEmailField
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 22, 'NotificationTrigger', 'SubscribersFreeEmailField', 1, 500, 'Abonnés par mails(mails séparés par des ";")[[BR]]Les abonnés reçoivent des notifications par mail, même si le mode de diffusion exclut le mail'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 22, 'Abonnés e-mail', 'E-mail subscribers', 'E-mail subscribers', 'E-mail subscribers', 'E-mail subscribers'

	-- SubscribersFreeTelField
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 23, 'NotificationTrigger', 'SubscribersFreeTelField', 1, 500, 'Abonnés par SMS(tels séparés par des ";")[[BR]]Les abonnés reçoivent des notifications par SMS, même si le mode de diffusion exclut le SMS'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 23, 'Abonnés SMS', 'SMS subscribers', 'SMS subscribers', 'SMS subscribers', 'SMS subscribers'
   
	-- SubscribersTargetDescId
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 24, 'NotificationTrigger', 'SubscribersTargetDescId', 10, 0,'DescId de la rubrique des abonnés dans la fiche référence'
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 24, 'Rubrique abonnés', 'Subscribers field', 'Subscribers field', 'Subscribers field', 'Subscribers field'

	
	-- Champ system
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 84, 'NotificationTrigger', 'NotificationTrigger84', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 88, 'NotificationTrigger', 'NotificationTrigger88', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 88, 'Notification de', 'Notification of', 'Notification of', 'Notification of', 'Notification of', 'Notification of'
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 95, 'NotificationTrigger', 'NotificationTrigger95', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 96, 'NotificationTrigger', 'NotificationTrigger96', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 97, 'NotificationTrigger', 'NotificationTrigger97', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 98, 'NotificationTrigger', 'NotificationTrigger98', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 99, 'NotificationTrigger', 'NotificationTrigger99', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	insert into pref (tab,userid, listcol) select @nTab, 0, '114201;114210;114211;114212;114218;114219;114221;114224'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '114201;114210;114211;114212;114218;114219;114221;114224' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '114201;114210;114211;114212;114218;114219;114221;114224', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '114201;114210;114211;114212;114218;114219;114221;114224', '0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
	
	
	--Mise en page Table NotificationTrigger
	update [DESC] set [columns] = '200,500,25' where [DescId] = @nTab  
	update [DESC] set [DispOrder] = 1, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 1 	
	update [DESC] set [DispOrder] = 4, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 8 
	update [DESC] set [DispOrder] = 5, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 80
	update [DESC] set [DispOrder] = 6, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 7 
	update [DESC] set [DispOrder] = 7, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 9 	
	update [DESC] set [DispOrder] = 8, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 10 
	update [DESC] set [DispOrder] = 9, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 11
	update [DESC] set [DispOrder] = 10, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 12 
	update [DESC] set [DispOrder] = 11, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 13 
	update [DESC] set [DispOrder] = 13, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 15 
	update [DESC] set [DispOrder] = 14, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 16 
	update [DESC] set [DispOrder] = 15, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 17 
	update [DESC] set [DispOrder] = 16, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 18 
	--update [DESC] set [DispOrder] = 17, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 19 
	--update [DESC] set [DispOrder] = 18, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 20 
	update [DESC] set [DispOrder] = 19, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 21
	update [DESC] set [DispOrder] = 20, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 22 
	update [DESC] set [DispOrder] = 21, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 23 
    update [DESC] set [DispOrder] = 24, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 24
    

END