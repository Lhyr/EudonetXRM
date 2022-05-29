set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[PAYMENTTRANSACTION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

	CREATE TABLE [dbo].[PAYMENTTRANSACTION] (
	
		[PAYTRANID] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id  
		
		-- champ de liaison nécessaire
        [PpId] [numeric](18, 0) NULL, 		  
        [PmId] [numeric](18, 0) NULL, 		  
        [ParentEvtId] [numeric](18, 0) NULL,   
		
		
		-- Champs
		/*
		ref eudo => EUDO_XXXXX|<DESCIDFIELD>|<FILEID>
		hostchechoutid
		paymentid
		date
		montant
		statut
		ID Tab
		ID fiche
		
 
		*/
		
		[PAYTRANREFEUDO] nvarchar(40) NOT NULL,  --ref eudo 
		[PAYTRANREFPRESTA] nvarchar(200) NULL,		-- hostchechoutid
		[PAYTRANDATEPAYMENT] datetime NULL,			-- date
		[PAYTRANAMOUNT] numeric(18, 2) NULL,	-- montant
		[PAYTRANSTATUS] numeric(18, 0) NULL,	-- statut
		[PAYTRANCODE] numeric(18, 0) NULL,	-- code
		[PAYTRANCATEGORY] numeric(18, 0) NULL,	-- categorie
		
		[PAYTRANTARGETDESCID] numeric(18, 0) NULL,	-- DescId table destinataire
		[PAYTRANTARGETFILEID] numeric(18, 0) NULL,	-- File Id  table destinataire
		
		[PAYTRANRETURNINFOS] nvarchar(max) NULL,	-- retour presta 
		[PAYTRANRETURNINFOS_HTML] bit NULL,	 
			
		-- params systeme
		[PAYTRAN74] geography NULL,
		[PAYTRAN74_GEO] nvarchar(max) NULL,
		
		[PAYTRAN84] [bit] NULL,	 
		[PAYTRAN88] [varchar](512) NULL,
		[PAYTRAN91] [int] NULL,
		[PAYTRAN95] [datetime] NULL,
		[PAYTRAN96] [datetime] NULL,
		[PAYTRAN97] [numeric](18, 0) NULL,
		[PAYTRAN98] [numeric](18, 0) NULL,
		[PAYTRAN99] [numeric](18, 0) NULL,

 
		
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[PAYMENTTRANSACTION] WITH NOCHECK ADD CONSTRAINT [PK_PAYMENTTRANSACTION] PRIMARY KEY CLUSTERED ( [PAYTRANID] ) ON [PRIMARY]
	
	CREATE UNIQUE NONCLUSTERED INDEX [IX_PAYMENTTRANSACTION_PAYTRANREFEUDO_UNIQUE] ON [dbo].[PAYMENTTRANSACTION]
	(
		[PAYTRANREFEUDO] ASC
	)
	WHERE ([PAYTRANREFEUDO] IS NOT NULL)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

END