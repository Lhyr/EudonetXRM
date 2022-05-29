set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[WORKFLOWSCENARIO]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN

    -- La table des trigger devant etre affiché par le moteur query, elle doit avoir une structure de tyoe event meme si certains
	-- champs ne sont en pratique pas nécessaire
	CREATE TABLE [dbo].[WORKFLOWSCENARIO] (
		[WFSCId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id de declancheur
		
		-- champ de liaison nécessaire
        [PpId] [numeric](18, 0) NULL, 		 -- PP - non applicable, présent pour des raisons systeme
        [PmId] [numeric](18, 0) NULL, 		 -- PM - non applicable, présent pour des raisons systeme
        [ParentEvtId] [numeric](18, 0) NULL, -- Event - non applicable, présent pour des raisons systeme
				
		[WFSCLabel] [nvarchar](1000) NULL, -- titre
		[WFSCDescription] [nvarchar](max) NULL, --description
		[WFSCDescription_html] bit null, --description
		[WFSCEnabled] [bit] NULL default 0, --description
		
			
		-- params systeme
		[WFSC84] [bit] NULL,	 
		[WFSC88] [varchar](512) NULL,
		[WFSC91] [int] NULL,
		[WFSC74] geography NULL,
		[WFSC74_GEO] nvarchar(max) NULL,
		[WFSC95] [datetime] NULL,
		[WFSC96] [datetime] NULL,
		[WFSC97] [numeric](18, 0) NULL,
		[WFSC98] [numeric](18, 0) NULL,
		[WFSC99] [numeric](18, 0) NULL,
		
		
		-- champ fonctionnel sans présence dans [desc]
		
		-- descid et fileid de la fiche event
		[WFTEVENTDESCID] [int] NULL,
		[WFTEVENTFILEID] [int] NULL,
		
		-- descid fichier des destinataires
		[WFTTARGETDESCID] [int] NULL,
		
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[WorkflowScenario] WITH NOCHECK ADD CONSTRAINT [PK_WorkflowScenario] PRIMARY KEY CLUSTERED ( [WFSCId] ) ON [PRIMARY]
END