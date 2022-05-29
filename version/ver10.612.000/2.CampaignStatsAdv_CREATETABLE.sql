/*
US:2579 - tache 3692
SPHAM/ASTAN
28/10/2020
*/
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[CAMPAIGNSTATSADV]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[CAMPAIGNSTATSADV] (
	
	
		[CAMPAIGNSTATSADVID] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,	--PK		
		
		-- Champ de statistiques
		[CAMPAIGNSTATSADV01] [numeric](18, 0) ,  -- Nb Unsub
		[CAMPAIGNSTATSADV02] [numeric](18, 0) ,  -- NB_VIEW
		[CAMPAIGNSTATSADV03] [numeric](18, 0) ,  -- NB_TOTAL_DEST
		[CAMPAIGNSTATSADV04] [numeric](18, 0) ,  -- NB_SINGLE_CLICK
		[CAMPAIGNSTATSADV05] [numeric](18, 0) ,  -- NB_CLICK_VIEW
		[CAMPAIGNSTATSADV06] [numeric](18, 0) ,  -- NB_CLICK
		[CAMPAIGNSTATSADV07] [numeric](18, 0) ,  -- NB_SENT
		[CAMPAIGNSTATSADV08] [numeric](18, 0) ,  -- 2_BLOCKED
		[CAMPAIGNSTATSADV09] [numeric](18, 0) ,  -- 2_INVALID_ADDRESS
		[CAMPAIGNSTATSADV10] [numeric](18, 0) ,  -- 2_REJECTED
		[CAMPAIGNSTATSADV11] [numeric](18, 0) ,  -- 2_TEMP_COMMUNICATION_FAILURE
		[CAMPAIGNSTATSADV12] [numeric](18, 0) ,  -- 2_TRANSIENT
		[CAMPAIGNSTATSADV13] [numeric](18, 0) ,  -- 2_UNKNOWN
		[CAMPAIGNSTATSADV14] [numeric](18, 0) ,  -- 2_COMPLAINT
		
		              


     
		
		--Champs systèmes
		[EvtId] [numeric](18,0) NULL,	--Liaison avec Campaign
		
		[CAMPAIGNSTATSADV84] [bit] NULL DEFAULT(0),	--Confidentiel
		[CAMPAIGNSTATSADV92] [varchar](1024) NULL,	--  de
		[CAMPAIGNSTATSADV95] [datetime] NULL,	--Créé le
		[CAMPAIGNSTATSADV97] [varchar](10) NULL,	--Créée par
		[CAMPAIGNSTATSADV96] [datetime] NULL,	--Modifié le
		[CAMPAIGNSTATSADV98] [varchar](10) NULL,	--Modifié par
		[CAMPAIGNSTATSADV99] [numeric](18, 0) NULL	--Appartient à
		
	) ON [PRIMARY];


	-- PK
	ALTER TABLE [dbo].[CAMPAIGNSTATSADV] 
	WITH NOCHECK ADD CONSTRAINT [PK_CAMPAIGNSTATSADV]
	PRIMARY KEY CLUSTERED ( [CAMPAIGNSTATSADVID] ) ON [PRIMARY];
	
	--Contrainte vers Campaign
	ALTER TABLE [dbo].[CAMPAIGNSTATSADV] WITH NOCHECK ADD CONSTRAINT [FK_CAMPAIGNSTATSADV_Campaign] FOREIGN KEY ( [EvtId] ) REFERENCES [dbo].[Campaign] ( [CampaignId] );
	
	--INDEX sur table CAMPAIGNSTATS
	IF NOT EXISTS(SELECT * FROM sys.indexes AS i INNER JOIN sys.index_columns AS ic 
	ON i.OBJECT_ID = ic.OBJECT_ID AND i.index_id = ic.index_id	
	WHERE ic.OBJECT_ID = OBJECT_ID('[CAMPAIGNSTATSADV]')		and i.name like 'IX_CAMPAIGNSTATS' )
	begin
		CREATE NONCLUSTERED INDEX [IX_CAMPAIGNSTATSADV] ON [dbo].[CAMPAIGNSTATSADV]
		( 
			[EvtId] ASC
		)
		WITH (STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	end


END

-- Titre separateur performance d'ouverture
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'CAMPAIGNSTATSADV' and syscolumns.name like 'CAMPAIGNSTATSADV15')
BEGIN            
	ALTER TABLE [CAMPAIGNSTATSADV] ADD [CAMPAIGNSTATSADV15] bit
END		


-- Titre separateur performance des liens
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'CAMPAIGNSTATSADV' and syscolumns.name like 'CAMPAIGNSTATSADV16')
BEGIN            
	ALTER TABLE [CAMPAIGNSTATSADV] ADD [CAMPAIGNSTATSADV16] bit
END		


-- Titre Abonement
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'CAMPAIGNSTATSADV' and syscolumns.name like 'CAMPAIGNSTATSADV17')
BEGIN            
	ALTER TABLE [CAMPAIGNSTATSADV] ADD [CAMPAIGNSTATSADV17] bit
END		


-- Titre separateur delivrabilité
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'CAMPAIGNSTATSADV' and syscolumns.name like 'CAMPAIGNSTATSADV18')
BEGIN            
	ALTER TABLE [CAMPAIGNSTATSADV] ADD [CAMPAIGNSTATSADV18] bit
END		
