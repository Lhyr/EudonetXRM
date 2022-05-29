/*
US:2579 - tache 3692
SPHAM/ASTAN
28/10/2020
*/

SET NOCOUNT ON;

DECLARE @nTab INT = 111100
DECLARE @nDescId INT = 111101

DELETE FROM [DESC] WHERE DESCID  >= @nTab AND DESCID < @nTab + 100
DELETE FROM [RES] WHERE  RESID >= @nTab AND RESID < @nTab + 100
 
DELETE [PERMISSION] WHERE [PERMISSION].PermissionId IN (SELECT PermId from TRAIT where TraitId BETWEEN @nTab AND @nTab+99)
DELETE [TRAIT] WHERE TraitId BETWEEN @nTab AND @nTab+99

DELETE [FILEDATAPARAM] WHERE DescId BETWEEN @nTab AND @nTab+99
DELETE [USERVALUE] WHERE DescId BETWEEN @nTab AND @nTab+99

	
INSERT INTO [DESC] ([DescId], [File], [Field], [Format]
	, [Length], [Type], [DispOrder], [ActiveTab], [ActiveBkmEvent], GetLevel, Userlevel 
	, InterEvent, InterEventNum, [Columns])
SELECT @nTab, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV', 0
	, 0, 2, @nTab, 0, 1, 1, 0
	, 1 , 1050, 'A,A,25,A,A,25'

INSERT INTO [RES] (ResId, LANG_00) 
SELECT @nTab, 'Performances de la campagne'
	
	--CHAMPS MODIFIABLES
	 
-- Désabonnés	 
-- 111101
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0, @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Désabonnés'

-- Ouverts
SET @nDescId = @nDescId + 1	-- 111102
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + RIGHT( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Ouverts'

--  Destinataire
SET @nDescId = @nDescId + 1	-- 111103
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled], tooltiptext) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1,'Nombre de destinataires ciblés par l''emailing - Number of recipients targeted by emailing'

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Destinataires'


--   Cliqueurs uniques
SET @nDescId = @nDescId + 1	-- 111104
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Cliqueurs uniques'


-- Nombre Clics lien de visualisation
SET @nDescId = @nDescId + 1	 -- 111105
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Clics lien de visualisation'


-- Nombre Clics liens
SET @nDescId = @nDescId + 1	 -- 111106
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Clics liens'



-- Nombre NB_SENT
SET @nDescId = @nDescId + 1	-- 111107
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

		
INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Envoyés'


-- Messages refusés
SET @nDescId = @nDescId + 1	 -- 111108
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Messages refusés'


-- Adresses invalides
SET @nDescId = @nDescId + 1	 -- 111109
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Adresses invalides'

-- 	Adresses inexistantes
SET @nDescId = @nDescId + 1	-- 111110
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Adresses inexistantes'


--  Communication impossible
SET @nDescId = @nDescId + 1	-- 111111
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Communication impossible'

-- Non joignables
SET @nDescId = @nDescId + 1	-- 111112
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Non joignables'


-- Rejets non catégorisés
SET @nDescId = @nDescId + 1	-- 111113
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Rejets non catégorisés'

-- Marqués en SPAM
SET @nDescId = @nDescId + 1	 -- 111114
INSERT INTO [desc] ([DescId], [File], [Field], 
	[Format], [Length], [disporder],[readonly],[ComputedFieldEnabled]) 
SELECT @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2)
		, 10, 0,  @nDescId % 100, 1, 1

INSERT INTO [res] (resid, lang_00) SELECT @nDescId, 'Marqués en SPAM'

-- titres separateur

-- Titre separateur performance d'ouverture
SET @nDescId = @nDescId + 1	 -- 111115
 
INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
select @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2), 15, 1,1,2

INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 111115, 'Perfomances d''ouverture', '', '', '', ''	

-- Titre separateur Perfomances des liens
SET @nDescId = @nDescId + 1	-- 111116
 
INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
select @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2), 15, 1,1,2


INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nDescId, 'Perfomances des liens', '', '', '', ''		
	
-- Titre separateur Abonnements
SET @nDescId = @nDescId + 1	-- 111117
 
INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
select @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2), 15, 1,1,2


INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nDescId, 'Abonnements', '', '', '', ''			

-- Titre separateur Déliverabilité
SET @nDescId = @nDescId + 1	-- 111118
INSERT INTO[desc] ([DescId], [File], [Field], [Format], [Length], [rowspan], [colspan]) 
select @nDescId, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV' + right( '00' + cast( @nDescId % 100 as varchar(10)),2), 15, 1,1,2

INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select @nDescId, 'Déliverabilité', '', '', '', ''			
	
	
	
	
	
--Champs systèmes
insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111184, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV84', 3, 0, 84 -- Logique

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111184, 'Confidentielle', 'Confidential', 'Vertraulich', 'Vertrouwelijk', 'Confidencial', 'Confidenziale'
	
insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder],[multiple]) 
select 111192, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV92', 8, 1024, 92, 1

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111192, 'Suivi de liens de', 'Campaign Statistik of', 'Track Link von', 'Track Link van', 'Track Link de', 'Track Link di'
	
insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111195, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV95', 2, 0, 95

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111195, 'Créé le','Created on','Geschaffen am','Gecreëerd op','Creado el','Creato il'

insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111196, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV96', 8, 0, 96

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111196, 'Modifié le','Modified on','Geändert am','Gewijzigd op','Modificado el','Modificato il'

insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111197, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV97', 2, 0, 97

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111197, 'Créé par','Created by','Geschaffen von','Gecreëerd door','Creado por','Creato da'

insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111198, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV98', 8, 0, 98

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111198, 'Modifié par','Modified by','Geändert von','Gewijzigd door','Modificado por','Modificato da'

insert into [desc] ([DescId], [File], [Field], [Format], [Length], [disporder]) 
select 111199, 'CAMPAIGNSTATSADV', 'CAMPAIGNSTATSADV99', 8, 0, 99

insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
select 111199, 'Appartient à','Belongs to','Gehört zu','Behoort tot','Pertenece a','Appartiene a'
	
	
-- Maj des disporder
update [desc] set DispOrder=0,Rowspan=1,Colspan=1 where descid like '1111__'

 
update [desc] set Columns='250,A,25' where descid= 111100



update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ) where descid= 111115
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de destinataires ciblés par l''emailing - Number of recipients targeted by emailing'  where descid= 111103
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de destinataires pour lesquels le courriel a été envoyé - Number of recipients for which the email was sent'  where descid= 111107
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de destinataires ayant ouvert le courriel - Number of recipients who opened the email' where descid= 111102


update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )  where descid= 111116
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de destinataires ayant cliqués sur au moins 1 lien - Number of recipients who clicked on at least 1 link' where descid= 111104
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de clics sur le lien de visualisation - Number of clicks on the preview link'  where descid= 111106
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre total de clics sur l''ensemble des liens de l''email - Total number of clicks on email links' where descid= 111105
	

update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ) where descid= 111117
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Nombre de destinataires s''étant désabonnés d''un ou plusieurs types de campagne - Number of recipients who have unsubscribed from one or more types of campaign'  where descid= 111101
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Erreur inconnue, le message de retour du serveur distant n’a pu être catégorisé - Unknown error, the return message from the remote server could not be categorised'  where descid= 111118


update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Erreur temporaire, un problème temporaire de communication entre notre serveur et le serveur distant a empêché la remise de l''email -Temporary error, a temporary communication problem between our server and the remote server prevented the delivery of the email'  where descid= 111111
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Erreur temporaire, le serveur distant existe mais la connexion n’a pu être établie - Temporary error, the remote server exists but the connection could not be established'  where descid= 111112
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Erreur définitive, le domaine de l’adresse de destination n’existe pas - Definitive error, the domain of the destination address does not exist'  where descid= 111109
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Erreur définitive, le domaine de l''adresse email destinataire existe, mais le serveur distant indique que l’utilisateur n’existe pas - Definitive error, the domain  of the recipient email address exists, but the remote server indicates that the user does not exist'  where descid= 111110
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Bloquant, l’adresse email et le domaine existent, mais le serveur distant a refusé le message - Blocking, the email address and domain exist, but the remote server refused the message'  where descid= 111108
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' ),tooltiptext='Bloquant, le courriel a été marqué comme spam par le destinataire - Blocking, the email has been marked as spam by the recipient'  where descid= 111114
update [desc] set DispOrder=(select max(disporder) +1 from [desc] where descid like  '1111__' )	,tooltiptext='Erreur inconnue, le message de retour du serveur distant n’a pu être catégorisé - Unknown error, the return message from the remote server could not be categorised'  where descid= 111113

	
------------------------------------------------------------------------------------------------------------
--TRAIT pas de droit d'ajout, pas de droit de suppr et de modif
		
declare @PRC_RIGHT_ADD numeric(18,0)
	,@PRC_RIGHT_MODIFY numeric(18,0)
	,@PRC_RIGHT_DELETE numeric(18,0)
	,@PRC_RIGHT_ADD_LIST numeric(18,0)
	,@nLastFileId numeric(18,0)

set @PRC_RIGHT_ADD = 1
set @PRC_RIGHT_MODIFY = 2
set @PRC_RIGHT_DELETE = 3
set @PRC_RIGHT_ADD_LIST = 15

INSERT INTO [Trait] ([TraitId],[UserLevel],[TraitLevel]) VALUES ( @nTab ,0,1)
INSERT INTO [Trait] ([TraitId],[UserLevel],[TraitLevel], [Sort]) VALUES ( @nTab + @PRC_RIGHT_ADD ,0,1, @PRC_RIGHT_ADD )
INSERT INTO [Trait] ([TraitId],[UserLevel],[TraitLevel], [Sort]) VALUES ( @nTab + @PRC_RIGHT_MODIFY ,0,1, @PRC_RIGHT_MODIFY+1 )
INSERT INTO [Trait] ([TraitId],[UserLevel],[TraitLevel], [Sort]) VALUES ( @nTab + @PRC_RIGHT_DELETE ,0,1, @PRC_RIGHT_DELETE+1 )
INSERT INTO [Trait] ([TraitId],[UserLevel],[TraitLevel], [Sort]) VALUES ( @nTab + @PRC_RIGHT_ADD_LIST ,0,1, 2 )
		
--AJOUT depuis autre fiche
INSERT INTO [PERMISSION] ([Mode],[Level],[User]) SELECT 1,0,NULL
SELECT @nLastFileId = SCOPE_IDENTITY()
Update TRAIT Set PermId = @nLastFileId Where TraitId=@nTab+@PRC_RIGHT_ADD
		
--AJOUT depuis menu liste ou menu fiche
INSERT INTO [PERMISSION] ([Mode],[Level],[User]) SELECT 1,0,NULL
SELECT @nLastFileId = SCOPE_IDENTITY()
Update TRAIT Set PermId = @nLastFileId Where TraitId=@nTab+@PRC_RIGHT_ADD_LIST
		
--MODIF
INSERT INTO [PERMISSION] ([Mode],[Level],[User]) SELECT 1,0,NULL
SELECT @nLastFileId = SCOPE_IDENTITY()
Update TRAIT Set PermId = @nLastFileId Where TraitId=@nTab+@PRC_RIGHT_MODIFY
		
--SUPPR
INSERT INTO [PERMISSION] ([Mode],[Level],[User]) SELECT 1,0,NULL
SELECT @nLastFileId = SCOPE_IDENTITY()
Update TRAIT Set PermId = @nLastFileId Where TraitId=@nTab+@PRC_RIGHT_DELETE


	
	------------------------------------------------------------------------------------------------------------
--Pref/selections-------------------------------------------------------------------------------------------

delete from pref where tab=@nTab
delete from bkmpref where bkm=@nTab
delete from selections where tab=@nTab

insert into pref (tab,userid, listcol) select @nTab, 0,''
insert into pref (tab,userid, listcol) select  @nTab, userid, '' from [user]


insert into bkmpref (tab,bkm,userid, bkmcol, bkmcolwidth) select 106000,@nTab, 0,'','0;0;0;0'
insert into bkmpref (tab,bkm,userid, bkmcol, bkmcolwidth) select  106000,@nTab, userid, '','' from [user]

insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab,'Vue par défaut', '','', 0
insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab,'Vue par défaut', '','', userid from [user]

update pref set selectid = selections.selectid from [pref]  
inner join selections on selections.userid = [pref].userid 
and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
 