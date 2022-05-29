set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWSTEP]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

	CREATE TABLE [dbo].[WORKFLOWSTEP] (
		[WFSId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id d'�tape
		
		-- champ de liaison n�cessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, pr�sent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, pr�sent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, pr�sent pour des raisons systeme
		
		-- Titre de d�clencheur
		[WFSLabel] [nvarchar](1000) NULL,
	    [WFSDescription] [nvarchar](max) NULL, --description

		[WFSFileID] [int] NULL, -- Id action dans declencheur, action, condition		
		[WFSDescID] [int] NULL, -- Id de table : declencheur, action, condition		

		
		[WFScenarioId] [int] NULL, -- descid de sc�nario

		[WFSNextStep] [int] NULL, -- l'�tape suivante pour une �tape
		[WFSPreviousStep] [int] Null, -- l'�tape pr�c�dente pour une �tape

		-- params systeme
		[WFS74] geography NULL,
		[WFS74_GEO] nvarchar(max) NULL,
		[WFS84] [bit] NULL,	 
		[WFS88] [varchar](512) NULL,
		[WFS91] [int] NULL,
		[WFS95] [datetime] NULL,
		[WFS96] [datetime] NULL,
		[WFS97] [numeric](18, 0) NULL,
		[WFS98] [numeric](18, 0) NULL,
		[WFS99] [numeric](18, 0) NULL,
		
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WORKFLOWSTEP] WITH NOCHECK ADD CONSTRAINT [PK_WORKFLOWSTEP] PRIMARY KEY CLUSTERED ( [WFSId] ) ON [PRIMARY]
END