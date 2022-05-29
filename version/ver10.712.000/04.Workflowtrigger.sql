set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWTRIGGER]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

    -- La table des trigger devant etre affiché par le moteur query, elle doit avoir une structure de tyoe event meme si certains
	-- champs ne sont en pratique pas nécessaire
	CREATE TABLE [dbo].[WORKFLOWTRIGGER] (
		[WFTId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id de declancheur
		
		-- champ de liaison nécessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, présent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, présent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, présent pour des raisons systeme
		
		-- Titre de déclencheur
		[WFTLabel] [nvarchar](1000) NULL,
			
		-- evenements de declenchement		
		[WFTType] [int] NULL, -- Ajout/Modification/Suppression : représentation bitwise 
		[WFTContext] [int] NULL, -- Formulaire, application, api... représentation bitwise
		[WFTDescIds] [varchar](1000) NULL, -- liste de descid déclencheur
		[WFTDate] [datetime] NULL, -- date de la notification 
		
		-- conditions de déclenchement
		[WFTFilterId] [numeric](18, 0) NULL, -- condition basé sur un filtre 	
		
		
		-- params systeme
		[WFT74] geography NULL,
		[WFT74_GEO] nvarchar(max) NULL,
		[WFT84] [bit] NULL,	 
		[WFT88] [varchar](512) NULL,
		[WFT91] [int] NULL,
		[WFT95] [datetime] NULL,
		[WFT96] [datetime] NULL,
		[WFT97] [numeric](18, 0) NULL,
		[WFT98] [numeric](18, 0) NULL,
		[WFT99] [numeric](18, 0) NULL,
		
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WorkflowTrigger] WITH NOCHECK ADD CONSTRAINT [PK_WorkflowTrigger] PRIMARY KEY CLUSTERED ( [WFTId] ) ON [PRIMARY]
END