

	
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
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1010__' ) where descid= 101019

-- titre séparateur Habilitations et Appartenances 
update [desc] set DispOrder=(select max(disporder) +2 from [desc] where descid like  '1010__' ), Colspan=2, rowspan=1 where descid= 101025
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
	
 
	