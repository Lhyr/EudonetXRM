set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWACTIVITIES]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

	CREATE TABLE [dbo].[WORKFLOWACTIVITIES] (
		[WFAcId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id de declancheur
		
		-- champ de liaison n�cessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, pr�sent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, pr�sent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, pr�sent pour des raisons systeme

		-- params systeme
		[WFAc74] geography NULL,
		[WFAc74_GEO] nvarchar(max) NULL,
		[WFAc84] [bit] NULL,	 
		[WFAc88] [varchar](512) NULL,
		[WFAc91] [int] NULL,
		[WFAc95] [datetime] NULL,
		[WFAc96] [datetime] NULL,
		[WFAc97] [numeric](18, 0) NULL,
		[WFAc98] [numeric](18, 0) NULL,
		[WFAc99] [numeric](18, 0) NULL,
		
		-- Titre de activit�
		[WFAcLabel] [nvarchar](1000) NULL,
	    [WFAcDescription] [nvarchar](max) NULL, --description d'activit�	
		
		-- descid de sc�nario
		[WFSTEPId] [int] NULL, -- Id de l'�tapes
		[WFAcStatus] [int] NULL, -- ttatus de activit�
		[WFAcDescId] [int] NULL , -- id de la table : destinataire, evenement ...
		[WFAcFileId] [int] NULL, -- id de la ligne dans la table

		-- 
		--[WFAcCreateDate] [datetime] NULL, -- date de cr�ation
		--[WFAcModificationDate] [datetime] NULL, -- date de modification
	
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WORKFLOWACTIVITIES] WITH NOCHECK ADD CONSTRAINT [PK_WORKFLOWACTIVITIES] PRIMARY KEY CLUSTERED ( [WFAcId] ) ON [PRIMARY]
END