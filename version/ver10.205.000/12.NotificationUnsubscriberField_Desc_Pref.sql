set nocount on;

DECLARE @nTab INT = 114300 
if (
	 Not EXISTS ( select * from [desc] where descid = @nTab )
	 )
begin
	delete [res] where resid >=@nTab AND resid < @nTab + 100
	delete [FileDataParam] where descid >=@nTab AND descid < @nTab + 100
	delete [UserValue] where descid >=@nTab AND descid < @nTab + 100
	delete [desc] where descid >=@nTab AND descid < @nTab + 100
	
	
	--Desc/res
	
	-- Table NotificationUnsubscriber
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [type], [InterEvent], [InterEventNum], [ActiveBkmEvent], [ActiveTab] ) select @nTab, 'NotificationUnsubscriber', 'NotificationUnsubscriber', 0, 0, 2, 1, 114200, 1, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab, 'Désabonnés', 'Unsubscriber', 'Unsubscriber', 'Unsubscriber', 'Unsubscriber'
	
	-- UserId
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 1, 'NotificationUnsubscriber', 'NotificationUnsubscriber01', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 1, 'Utilisateur désabonné', 'Unsubscriber', 'Unsubscriber', 'Unsubscriber', 'Unsubscriber'

	-- Email
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 2, 'NotificationUnsubscriber', 'Email', 1, 20
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 2, 'Courriel', 'Email', 'Email', 'Email', 'Email'	
	
	-- Telephone
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 3, 'NotificationUnsubscriber', 'Telephone', 1, 20
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 3, 'Téléphone', 'Telephone', 'Telephone', 'Telephone', 'Telephone'	
	
	-- Unsubscribe
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 4, 'NotificationUnsubscriber', 'Unsubscribe', 3, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 4, 'Désabonné', 'Unsubscribe', 'Unsubscribe', 'Unsubscribe', 'Unsubscribe'
	
	
	-- Cahmp systeme
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 84, 'NotificationUnsubscriber', 'NotificationUnsubscriber84', 3, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 92, 'NotificationUnsubscriber', 'NotificationUnsubscriber92', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 92, 'Notification de', 'Notification of', 'Notification of', 'Notification of', 'Notification of', 'Notification of'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 99, 'NotificationUnsubscriber', 'NotificationUnsubscriber99', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'		
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 95, 'NotificationUnsubscriber', 'NotificationUnsubscriber95', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 96, 'NotificationUnsubscriber', 'NotificationUnsubscriber96', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 97, 'NotificationUnsubscriber', 'NotificationUnsubscriber97', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 98, 'NotificationUnsubscriber', 'NotificationUnsubscriber98', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	insert into pref (tab,userid, listcol) select @nTab, 0, cast(@nTab + 1 as varchar(10)) + ';' + cast(@nTab + 4 as varchar(10)) + ';' + cast(@nTab + 99 as varchar(10))
	insert into pref (tab,userid, listcol) select  @nTab, userid, cast(@nTab + 1 as varchar(10)) + ';' + cast(@nTab + 4 as varchar(10)) + ';' + cast(@nTab + 99 as varchar(10)) from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', cast(@nTab + 1 as varchar(10)) + ';' + cast(@nTab + 4 as varchar(10)) + ';' + cast(@nTab + 99 as varchar(10)), '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', cast(@nTab + 1 as varchar(10)) + ';' + cast(@nTab + 4 as varchar(10)) + ';' + cast(@nTab + 99 as varchar(10)), '0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
	
	
	--Mise en page Table NotificationUnsubscriber	
	update [desc] set [DispOrder] = 4 where [descid] = @nTab + 2
	update [desc] set [DispOrder] = 5 where [descid] = @nTab + 3
	update [desc] set [DispOrder] = 6 where [descid] = @nTab + 4 
	

END