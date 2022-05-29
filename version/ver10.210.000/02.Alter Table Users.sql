
-- Modifié le 
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER96')
BEGIN            
	ALTER TABLE [USER] ADD [USER96] datetime  
END

IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101096)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101096, 'USER', 'USER96', 2, 0


IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101096)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101096, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'
	
	
-- Créé le	
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER95')
BEGIN            
	ALTER TABLE [USER] ADD [USER95] datetime  
END


IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101095)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101095, 'USER', 'USER95', 2, 0

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101095)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101095, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	



-- Crée par
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER97')
BEGIN            
	ALTER TABLE [USER] ADD [USER97] int
END

 
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101097)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101097, 'USER', 'USER97', 8, 0

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101097)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101097, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'


-- Modifié Par
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER98')
BEGIN            
	ALTER TABLE [USER] ADD [USER98] int  
END
 

IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101098)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101098, 'USER', 'USER98', 8, 0

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101098)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101098, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'


delete from [desc] where descid=	101023
-- Langue de connexion
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101023)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101023, 'USER', 'LANG', 1, 200

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101023)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101023, 'Langue de connexion', '', '', '', ''


 
 
 -- Password n'expire pas
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101028)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101028, 'USER', 'PwdNotExpire', 3, 1

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101028)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101028, 'Le mot de passe n''expire pas.', '', '', '', ''

	


IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101029)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101029, 'USER', 'UserLoginExternal', 1, 200

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101029)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101029, 'Login externe', '', '', '', ''	
	
	
	 /*
 -- Version Office
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101029)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101029, 'USER', 'PwdNotExpire', 3, 1

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101029)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101029, 'Stratégie de mot de passe.', '', '', '', ''
 
 -- Mode Export
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101028)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101028, 'USER', 'PwdNotExpire', 3, 1

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101028)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101028, 'Stratégie de mot de passe.', '', '', '', ''	
	*/
	
-- AVATAR	
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER75')
BEGIN            
	ALTER TABLE [USER] ADD [USER75] nvarchar(200)  
END	

IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER75_NAME')
BEGIN            
	ALTER TABLE [USER] ADD [USER75_NAME] nvarchar(200)  
END	


IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER75_TYPE')
BEGIN            
	ALTER TABLE [USER] ADD [USER75_TYPE] nvarchar(200)  
END	

	
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101075)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101075, 'USER', 'USER75', 13, 100

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101075)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101075, 'Avatar', 'Avatar', 'Avatar', 'Avatar', 'Avatar'	
	
	
-- 	Titre séparateur Coordonnées
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101024)
	INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
	select 101024, 'USER', 'USER24', 15, 1,1,2

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101024)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101024, 'Coordonnées', '', '', '', ''	
	
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER24')
BEGIN            
	ALTER TABLE [USER] ADD [USER24] bit
END		
	
	
-- 	Habilitations et Appartenances 
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101025)
	INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
	select 101025, 'USER', 'USER25', 15, 1,1,2

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101025)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101025, 'Habilitations et Appartenances', '', '', '', ''	
	
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER25')
BEGIN            
	ALTER TABLE [USER] ADD [USER25] bit
END		


-- 	Informations Complémentaires
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101026)
	INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
	select 101026, 'USER', 'USER26', 15, 1,1,2

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101026)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101026, 'Informations Complémentaires', '', '', '', ''	
	
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'USER26')
BEGIN            
	ALTER TABLE [USER] ADD [USER26] bit
END	

 
delete from [desc] WHERE [DESCID] = 101027 
delete from [res] WHERE [resid] = 101027
delete from [uservalue] WHERE [DESCID]= 101027 AND [TYPE] = 23 AND [TAB] = 101000
 
	 
 -- Group
 IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101027)
	INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], rowspan,colspan ) select 101027, 'USER', 'GROUPID', 1, 520,1,1 

DELETE FROM [RES] where [RESID] = 101027
IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101027)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101027, 'Group ID', 'Group ID', 'Group ID', 'Group ID', 'Group ID'	
	
	

IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 103001)
	INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], rowspan,colspan ) select 103001, 'GROUP', 'GROUPNAME', 1, 50,1,1
	
 IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 103001)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 103001, 'Groupe', 'Groupe', 'Groupe', 'Groupe', 'Groupe'	
		
	
	/*
IF NOT EXISTS ( SELECT 1 FROM [USERVALUE] WHERE [DESCID]= 101027 AND [TYPE] = 23 AND [TAB] = 101000)
	INSERT INTO[USERVALUE] (TAB, DESCID,[TYPE], [ENABLED],[INDEX], [LABEL], [ICON], VALUE ) SELECT 101000, 101027, 23, 1, 0, '', '','([USERID] NOT IN (SELECT [USERID] FROM [USER]))'	
 */
	
-- Maj des disporder
update [desc] set DispOrder=0,Rowspan=1,Colspan=1 where descid like '1010__'

	-- Bloc principal
update [desc] set Columns='70,A,25,70,A,25' where descid= 101000
update [desc] set DispOrder= (select max(disporder) +1 from [desc] where descid like  '1010__' ) , Rowspan=9, Colspan=1 where descid= 101075
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101001
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101002
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101010
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101007
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101009
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101008
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101004
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101018
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101023

--titre séparateur Coordonnées
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1  where descid= 101024
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=3 where descid= 101006
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101013
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101014

-- titre séparateur Habilitations et Appartenances 
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1 where descid= 101025
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101011
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101012
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101027
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101017
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101028
--update [desc] set DispOrder=18 where descid= 101017

-- Titre séparateur Informations complémentaire
update [desc] set DispOrder=(select max(disporder) + 2 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1 where descid= 101026
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=5  where descid= 101003

-- Login et level obligatoire
update [DESC] set Obligat=1 where DescId=101001
update [DESC] set Obligat=1 where DescId=101017

-- login en maj
 update [DESC] set [Case] = 1  where DescId = 101001	
	
 
	