
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmHomePage]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[XrmHomePage] (
		[XrmHomePageId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,  
       	
		-- Titre de la page d'accueil
		[Title] [varchar](1000) NULL,	
        
		-- Titre de la page d'accueil
		[Tooltip] [varchar](1000) NULL,			
		
		-- Affectation aux utilisateurs
		[UserAssign] [varchar](1000) NULL,
		
		-- Affectation aux group
		[GroupAssign] [varchar](1000) NULL,
				
		-- Params systeme	
        [XrmHomePage84] [bit] NULL,	
        [XrmHomePage92] [varchar](1000) NULL,		
		[XrmHomePage95] [datetime] NULL,
		[XrmHomePage96] [datetime] NULL,
		[XrmHomePage97] [numeric](18, 0) NULL,
		[XrmHomePage98] [numeric](18, 0) NULL,
		[XrmHomePage99] [numeric](18, 0) NULL,
		
	) ON [PRIMARY]	
	ALTER TABLE [dbo].[XrmHomePage] WITH NOCHECK ADD CONSTRAINT [PK_XrmHomePage] PRIMARY KEY CLUSTERED ( [XrmHomePageId] ) ON [PRIMARY]
END
