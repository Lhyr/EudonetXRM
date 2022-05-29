
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmGrid]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[XrmGrid] (
		[XrmGridId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,  

        -- Relation vers la table parente		
        [ParentTab] [numeric](18, 0) NULL,
		
		-- Relation vers la fiche parente		
        [ParentFileId] [numeric](18, 0) NULL,
		
		-- Titre de la page d'accueil
		[Title] [varchar](1000) NULL,	
		
		-- Infobulle de la page d'accueil
		[Tooltip] [varchar](1000) NULL,	
		
		-- Ordre d'affichage dans l'onglet
		[DisplayOrder] [int] NULL,

		-- Ordre d'affichage dans l'onglet
		[ShowWidgetTitle] [bit] NULL,
			
        -- Permissions
    	[ViewPermId] [numeric](18, 0) Null,       
		[UpdatePermId] [numeric](18, 0) Null,
		
		-- Params systeme
		[XrmGrid84] [bit] NULL,	 -- confidentielle
		[XrmGrid88] [varchar](1000) NULL,
		[XrmGrid92] [varchar](1000) NULL,
		[XrmGrid95] [datetime] NULL,
		[XrmGrid96] [datetime] NULL,
		[XrmGrid97] [numeric](18, 0) NULL,
		[XrmGrid98] [numeric](18, 0) NULL,
		[XrmGrid99] [numeric](18, 0) NULL,
		
	) ON [PRIMARY]	
	ALTER TABLE [dbo].[XrmGrid] WITH NOCHECK ADD CONSTRAINT [PK_XrmGrid] PRIMARY KEY CLUSTERED ( [XrmGridId] ) ON [PRIMARY]
END
