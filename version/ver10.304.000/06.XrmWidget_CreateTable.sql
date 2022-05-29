
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmWidget]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[XrmWidget] (
		[XrmWidgetId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,  
     
		-- Titre du widget
		[Title] [varchar](1000) NULL,	
		
	    -- Sous-titre du widget
		[SubTitle] [varchar](1000) NULL,	
		
		-- Infobulle du widget
		[Tooltip] [varchar](1000) NULL,	
		
		 -- Relation vers la table parente		
        [Type] int NULL,
		
		 -- Pictogramme du l'onglet	
        [PictoIcon] varchar(50) NULL,
		
		 -- Couleur Pictogramme du l'onglet	
        [PictoColor] varchar(50) NULL,
		
		-- Déplaçable
		[Move] [bit] NULL,
	
	    -- Redémensionnable
		[Resize] [bit] NULL,

		-- Actualisation manuel
		[ManualRefresh] [bit] NULL,
		
		 -- Options d'affichage : Toujours affiché :0, Affiché par défaut : 1, Masqué par défaut : 2
        [DisplayOption] int NULL,	
		
		 -- Position de la widget par defaut		
        [DefaultPosX] int NULL,	
        [DefaultPosY] int NULL,
		
		-- largeur/hauteur pour le propriétaire		
        [DefaultWidth] int NULL,	
        [DefaultHeight] int NULL,		
	    
		-- contenu		
		[ContentSource] [varchar] (max) null,	
        [ContentType] [int] null,			
		[ContentParam][varchar] (1000) null,
		
		-- droits
		[ViewPermId] [numeric](18, 0) NULL,	    

        -- Affichage dans l'onglet
		[ShowHeader] [bit] NULL,		
						
		-- Params systeme
		[XrmWidget84] [bit] NULL,	 -- confidentielle
		[XrmWidget88] [varchar](1000) NULL,
		[XrmWidget92] [varchar](1000) NULL,
		[XrmWidget95] [datetime] NULL,
		[XrmWidget96] [datetime] NULL,
		[XrmWidget97] [numeric](18, 0) NULL,
		[XrmWidget98] [numeric](18, 0) NULL,
		[XrmWidget99] [numeric](18, 0) NULL,
		
	) ON [PRIMARY]	
	ALTER TABLE [dbo].[XrmWidget] WITH NOCHECK ADD CONSTRAINT [PK_XrmWidget] PRIMARY KEY CLUSTERED ( [XrmWidgetId] ) ON [PRIMARY]
END
