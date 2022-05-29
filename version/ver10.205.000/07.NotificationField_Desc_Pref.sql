set nocount on;

DECLARE @nTab INT = 114000 
begin
	delete [res] where resid >=@nTab AND resid < @nTab + 100
	delete [FileDataParam] where descid >=@nTab AND descid < @nTab + 100
	delete [UserValue] where descid >=@nTab AND descid < @nTab + 100
	delete [desc] where descid >=@nTab AND descid < @nTab + 100
	
	ALTER TABLE [DESC] 
	ALTER column [ToolTipText] nvarchar(max)
	
	--Desc/res
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [type], [ActiveTab]) select @nTab, 'NOTIFICATION', 'NOTIFICATION', 0, 0, 0, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab, 'Notifications', 'Notification', 'Notification', 'Notification', 'Notification'

	--titre
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 1, 'NOTIFICATION', 'Title', 1, 255
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 1, 'Titre', 'Title', 'Title', 'Title', 'Title'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 2, 'NOTIFICATION', 'TitleLong', 1, 2000
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 2, 'Titre Long', 'Title Long', 'Title Long', 'Title Long', 'Title Long'
	
	--Description
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 3, 'NOTIFICATION', 'Description', 1, 2000
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 3, 'Description', 'Description', 'Description', 'Description', 'Description'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select @nTab + 4, 'NOTIFICATION', 'DescriptionLong', 9, 0, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 4, 'Description Long', 'Description Long', 'Description Long', 'Description Long', 'Description Long'	
	
	--Corps courriel
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select @nTab + 5, 'NOTIFICATION', 'EmailBody', 9, 0, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 5, 'Corps', 'Body', 'Body', 'Body', 'Body'	
	
	--Type
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 6, 'NOTIFICATION', 'NotificationType', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 6, 'Type', 'Type', 'Type', 'Type', 'Type'	
	
	--Icone, Image
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 7, 'NOTIFICATION', 'Icon', 22, 25
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 7, 'Icône', 'Icon', 'Icon', 'Icon', 'Icon'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 8, 'NOTIFICATION', 'Image', 13, 100
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 8, 'Image', 'Image', 'Image', 'Image', 'Image'
	
	--Date Purge, date notification
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 9, 'NOTIFICATION', 'ExpirationDate', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 9, 'Date de purge', 'Purge date', 'Purge date', 'Purge date', 'Purge date'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 10, 'NOTIFICATION', 'NotificationDate', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 10, 'Date notification', 'Notification date', 'Notification date', 'Notification date', 'Notification date'	
	
	--Table et fiche parentes
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 11, 'NOTIFICATION', 'ParentTab', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 11, 'Table parente', 'Parent table', 'Parent table', 'Parent table', 'Parent table', 'Parent table'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 12, 'NOTIFICATION', 'ParentFileId', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 12, 'Fiche parente', 'Parent file', 'Parent file', 'Parent file', 'Parent file', 'Parent file'
	
	--Telephone et adresse courriel
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 13, 'NOTIFICATION', 'Email', 1, 2000
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 13, 'Courriel', 'Email', 'Email', 'Email', 'Email'	
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 14, 'NOTIFICATION', 'Telephone', 1, 2000
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 14, 'Téléphone', 'Telephone', 'Telephone', 'Telephone', 'Telephone'
	
	--Lue
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 15, 'NOTIFICATION', 'Read', 3, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 15, 'Lue', 'Read', 'Read', 'Read', 'Read'	
	
	--Toasted Date
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 16, 'NOTIFICATION', 'ToastedDate', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 16, 'Date toaster', 'Toasted date', 'Toasted date', 'Toasted date', 'Toasted date'
	
	--Status
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 17, 'NOTIFICATION', 'NotificationStatus', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 17, 'Statut', 'Status', 'Status', 'Status', 'Status'
	
	-- BroadcastType
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length], [ToolTipText]) SELECT @nTab + 18, 'NOTIFICATION', 'BroadcastType', 10, 0, '0- Notifications créées mais pas diffusées (en cours) [[BR]]1- Notification XRM  [[BR]]2- Notifications Mobile (en cours) [[BR]]4- Notifications Mail [[BR]]8- Notifications SMS (en cours) [[BR]](Somme des valeurs)- Diffusion multiple ex:(1+4)=5, diffusion xrm ET mail'
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 18, 'Mode de diffusion', 'Broadcast type', 'Broadcast type', 'Broadcast type', 'Broadcast type'
	
	--Couleur
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 80, 'NOTIFICATION', 'Notification80', 1, 20
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 80, 'Couleur', 'Color', 'Color', 'Color', 'Color'
	
	--Champs Système
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 84, 'NOTIFICATION', 'Notification84', 3, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	--insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 92, 'NOTIFICATION', 'Notification92', 8, 0
	--insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 92, 'Notification de', 'Notification of', 'Notification of', 'Notification of', 'Notification of', 'Notification of'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 88, 'NOTIFICATION', 'Notification88', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 88, 'Notification de', 'Notification of', 'Notification of', 'Notification of', 'Notification of', 'Notification of'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 99, 'NOTIFICATION', 'Notification99', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	--Créé/Modifié le/par
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 95, 'NOTIFICATION', 'Notification95', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 96, 'NOTIFICATION', 'Notification96', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 97, 'NOTIFICATION', 'Notification97', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 98, 'NOTIFICATION', 'Notification98', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
	
	--Pref/selections
	--On affiche Titre et créée le  from pref where tab=114000
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	insert into pref (tab,userid, listcol) select @nTab, 0, '114001;114010;114016;114015;114009;114017'	
	insert into pref (tab,userid, listcol) select  @nTab, userid, '114001;114010;114016;114015;114009;114017'	 from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '114001;114010;114016;114015;114009;114017', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '114001;114010;114016;114015;114009;114017', '0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
	
	
	--Mise en page Table NOTIFICATION
	update [desc] set [BreakLine] = 14 where [descid] = @nTab --breakLine
	update [desc] set [DispOrder] = 1, [Rowspan] = 0, [Colspan] = 2 where [descid] = @nTab + 1 --titre
	update [desc] set [DispOrder] = 3, [Rowspan] = 0, [Colspan] = 2 where [descid] = @nTab + 2 --titre long
	update [desc] set [DispOrder] = 5, [Rowspan] = 0, [Colspan] = 2 where [descid] = @nTab + 3 --description
	update [desc] set [DispOrder] = 7, [Rowspan] = 2, [Colspan] = 2 where [descid] = @nTab + 4 --description long
	update [desc] set [DispOrder] = 10, [Rowspan] = 2, [Colspan] = 2 where [descid] = @nTab + 5 --corps
	update [desc] set [DispOrder] = 13, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 6 --type
	update [desc] set [DispOrder] = 14, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 7 --icone
	--update [desc] set [DispOrder] = 15, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 8 --image
	update [desc] set [DispOrder] = 16, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 9 --date de purge
	update [desc] set [DispOrder] = 17, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 10 --date notification
	update [desc] set [DispOrder] = 19, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 11 --parent tab
	update [desc] set [DispOrder] = 20, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 12 --parent file
	update [desc] set [DispOrder] = 22, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 13 --Courriel
	update [desc] set [DispOrder] = 23, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 14 --Téléphone
	update [desc] set [DispOrder] = 25, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 15 --Lue
	update [desc] set [DispOrder] = 26, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 16 --Toasted Date
	update [desc] set [DispOrder] = 27, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 17 --Statuts
	update [desc] set [DispOrder] = 28, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 18 --Statuts
	update [desc] set [DispOrder] = 29, [Rowspan] = 0, [Colspan] = 0 where [descid] = @nTab + 80 --BroadcastType

END