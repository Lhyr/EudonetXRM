-- MAB - Backlog #1041 - Volumétrie E-mail
-- Créé le 24/05/2019
-- Modifié le 03/06/2019 pour passer la colonne LastSynchronized en non null (également effectué par un autre script de montée de version pour les bases ayant appliqué la version précédente du script)

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[EDNMSGCMPT]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN	
	CREATE TABLE [dbo].[EDNMSGCMPT](
		[ID] [NUMERIC](18, 0) IDENTITY(1,1) NOT NULL, -- ID
		[CounterType] [VARCHAR](20) NOT NULL, -- Type de donnée
		[CounterDate] [DATETIME] NOT NULL, -- Date correspondant à la donnée - Stockage en DATETIME, mais correspond en réalité au mois correspondant (ex : 05/2019)
		[CounterVolume] [VARCHAR](255) NOT NULL, -- "Volume" = Valeur de la donnée, chiffrée
		[LastUpdated] [DATETIME] NOT NULL, -- Date de dernière mise à jour/création de l'enregistrement dans cette table EDNMSGCMPT
		[LastSynchronized] [DATETIME] NULL, -- Date de dernier envoi des données vers la base de statistiques centralisée
	 CONSTRAINT [PK_EDNMSGCMPT] PRIMARY KEY CLUSTERED 
	(
		[ID] ASC
	) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END