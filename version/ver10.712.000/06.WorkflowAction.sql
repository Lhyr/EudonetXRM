set nocount on;

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWACTION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

    -- La table des trigger devant etre affiché par le moteur query, elle doit avoir une structure de tyoe event meme si certains
	-- champs ne sont en pratique pas nécessaire
	CREATE TABLE [dbo].[WORKFLOWACTION] (
		[WFAId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id d'action
		
		-- champ de liaison nécessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, présent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, présent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, présent pour des raisons systeme
			
		-- params systeme
		[WFA74] geography NULL,
		[WFA74_GEO] nvarchar(max) NULL,
		[WFA84] [bit] NULL,	 
		[WFA88] [varchar](512) NULL,
		[WFA91] [int] NULL,
		[WFA95] [datetime] NULL,
		[WFA96] [datetime] NULL,
		[WFA97] [numeric](18, 0) NULL,
		[WFA98] [numeric](18, 0) NULL,
		[WFA99] [numeric](18, 0) NULL,

		
		-- evenements de declenchement		
		[WFAType] [int] NULL, -- email/emailing/sms/attendre x jour ou attendre une date
		[WFACampaignId] [int] NULL, -- id campagne
	    [WFADate] [datetime] NULL, -- date de la notification 
		[WFATimeLimit] [int] NULL, -- nombre de jour pour l'envoie

		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WorkflowAction] WITH NOCHECK ADD CONSTRAINT [PK_WorkflowAction] PRIMARY KEY CLUSTERED ( [WFAId] ) ON [PRIMARY]
END
