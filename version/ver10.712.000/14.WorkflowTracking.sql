set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWTRACKING]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

	CREATE TABLE [dbo].[WORKFLOWTRACKING] (
		[WFTrId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id d'étape
		
		-- champ de liaison nécessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, présent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, présent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, présent pour des raisons systeme		
		
		-- params systeme
		[WFTr74] geography NULL,
		[WFTr74_GEO] nvarchar(max) NULL,
		[WFTr84] [bit] NULL,	 
		[WFTr88] [varchar](512) NULL,
		[WFTr91] [int] NULL,
		[WFTr95] [datetime] NULL,
		[WFTr96] [datetime] NULL,
		[WFTr97] [numeric](18, 0) NULL,
		[WFTr98] [numeric](18, 0) NULL,
		[WFTr99] [numeric](18, 0) NULL,

		[WFTrLabel] [nvarchar](1000) NULL, -- Label
	    [WFTrDescription] [nvarchar](max) NULL, -- description

		-- descid de scénario
		[WFTrStepId] [int] NULL, -- condition basé sur un filtre 
		[WFTrStatus] [int] NULL, -- status de activité
		[WFTrDescId] [int] NULL , -- id de la table : destinataire, evenement ...
		[WFTrFileId] [int] NULL, -- id de la ligne dans la table
		[WFTrCreateDate] [datetime] NULL, -- date de création
		[WFTrModificationDate] [datetime] NULL, -- date de modification
		
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WORKFLOWTRACKING] WITH NOCHECK ADD CONSTRAINT [PK_WORKFLOWTRACKING] PRIMARY KEY CLUSTERED ( [WFTrId] ) ON [PRIMARY]
END