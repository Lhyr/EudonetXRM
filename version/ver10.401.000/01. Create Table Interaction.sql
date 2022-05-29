IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[INTERACTION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	-- Création de la table RGPDTreatmentsLogs si elle n'existe pas
	
	CREATE TABLE [dbo].[INTERACTION] (
			--Champs Systèmes
			[TPLID] [numeric](18, 0) IDENTITY(1,1) NOT NULL			
			,[EvtId] [numeric](18, 0) NULL
			,[PpId] [numeric](18, 0) NULL
			,[PmId] [numeric](18, 0) NULL
			,[AdrId] [numeric](18, 0) NULL
			,[TPL74_GEO] [geography] NULL
			,[TPL74] AS ([TPL74_GEO].[STAsText]())
			,[TPL76] [varchar](50) NULL
			,[TPL77] [numeric](18, 0) NULL
			,[TPL78] [datetime] NULL
			,[TPL79] [bit] NULL
			,[TPL80] [varchar](50) NULL
			,[TPL81] [varchar](512) NULL
			,[TPL82] [numeric](18, 0) NULL
			,[TPL83] [numeric](18, 0) NULL
			,[TPL84] [bit] NULL
			,[TPL89] [datetime] NULL
			,[TPL92] [varchar](512) NULL
			,[TPL93] [varchar](max) NULL
			,[TPL93_HTML] [bit] NULL
			,[TPL94] [varchar](max) NULL
			,[TPL94_HTML] [bit] NULL
			,[TPL95] [datetime] NULL
			,[TPL96] [datetime] NULL
			,[TPL97] [varchar](512) NULL
			,[TPL98] [varchar](512) NULL
			,[TPL99] [numeric](18, 0) NULL
			
			--Rubriques
			,[TPL02] [bit] NULL
			,[TPL03] [bit] NULL
			,[TPL04] [bit] NULL
			,[TPL05] [datetime] NULL
			,[TPL06] [varchar](100) NULL
			,[TPL07] [bit] NULL
			,[TPL08] [varchar](100) NULL
			,[TPL09] [varchar](100) NULL
			,[TPL10] [varchar](512) NULL
			,[TPL11] [varchar](100) NULL
			,[TPL12] [varchar](512) NULL
			,[TPL13] [varchar](100) NULL
			,[TPL14] [varchar](100) NULL
			,[TPL15] [varchar](max) NULL
			,[TPL15_HTML] [bit] NULL
			,[TPL16] [varchar](max) NULL
			,[TPL16_HTML] [bit] NULL
		) ON [PRIMARY]
	
	--Clé primaire
	ALTER TABLE [dbo].[INTERACTION] WITH NOCHECK ADD CONSTRAINT [PK_INTERACTION] PRIMARY KEY CLUSTERED ( [TPLID] ) ON [PRIMARY]
	
	--Clés étrangères
	ALTER TABLE [dbo].[INTERACTION] ADD CONSTRAINT [FK_INTERACTION_PP] FOREIGN KEY ( [PpId] ) REFERENCES [dbo].[PP]( [PpId] )
	ALTER TABLE [dbo].[INTERACTION] ADD CONSTRAINT [FK_INTERACTION_PM] FOREIGN KEY ( [PmId] ) REFERENCES [dbo].[PM]( [PmId] )
	ALTER TABLE [dbo].[INTERACTION] ADD CONSTRAINT [FK_INTERACTION_ADDRESS] FOREIGN KEY ( [AdrId] ) REFERENCES [dbo].[ADDRESS]( [AdrId] )
	
END