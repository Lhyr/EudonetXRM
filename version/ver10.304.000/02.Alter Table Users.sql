 
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101032)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 101032, 'USER', 'IsProfile', 3, 1

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101032)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101032, 'Profil', '', '', '', ''

	
delete from res where resid = 101028
IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101028)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101028, 'Le mot de passe n''expire pas.', '', '', '', ''

	
if not exists (select 1 from resadv where descid=101032 and [type]=2 and id_lang=0)
	INSERT INTO [RESADV] (descid, lang, id_lang, [type] ) select 101032, 'Définir cet utilisateur comme profil de référence pour les préférences', 0,2
	
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


-- extentsion
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1  where descid= 101031
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101020

-- titre séparateur Habilitations et Appartenances 
update [desc] set DispOrder=(select max(disporder) +2 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1 where descid= 101025
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101011
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101012
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101027
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101017
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101028
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101032
--update [desc] set DispOrder=18 where descid= 101017

-- Titre séparateur Informations complémentaire
update [desc] set DispOrder=(select max(disporder) + 1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1 where descid= 101026
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=5  where descid= 101003

-- Login et level obligatoire
update [DESC] set Obligat=1 where DescId=101001
update [DESC] set Obligat=1 where DescId=101017

-- login en maj
 update [DESC] set [Case] = 1  where DescId = 101001	
	
 
	