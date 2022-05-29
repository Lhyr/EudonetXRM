IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[SPECIFS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )

/* ************************************************************
 *   KHA/SPH 2014-06-30
 *   TABLE DES SPECIFS
 ************************************************************************* */
BEGIN



 

CREATE TABLE [dbo].SPECIFS(
	[SpecifId] [numeric](18,0) IDENTITY(1,1) NOT NULL,  -- ID CLEE PRIMAIRE
	[SpecifType] [int] NOT NULL,						-- Type de la spécif 	(HomePage, Menu accueil, fiche...)
	[OpenMode] [int] NOT NULL,							-- Mode d'ouverture de la spécif (eModal, eUpdater...)
	[Source] [int] NOT NULL,							-- Source : V7 / XRM ...
	[Tab] [int]  NULL,								-- Table de la spécif si applicable
	[Label] [varchar](2000) NOT NULL,					--- Nom du lien de la spécif
	[URL] [varchar](2048) NOT NULL,						--- URL a ouvrir
	[URLParam] [varchar](2048) NOT NULL,					--- paramètres additionnels pour l'URL a ouvrir
	[ViewPermId] [numeric](18, 0)   NULL				-- Permission de visu
	
 CONSTRAINT [PK_SPECIFS] PRIMARY KEY CLUSTERED 
(
	[SpecifId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE NONCLUSTERED INDEX [IX_INDEX] ON [dbo].[SPECIFS] 
(
	[SpecifType] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
 

 
END
