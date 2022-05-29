
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmWidgetPref]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[XrmWidgetPref] (
	     -- id de la pref
		[XrmWidgetPrefId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,  
		
		-- référence à la laision
		[XrmGridWidgetId] [numeric](18, 0) NOT NULL,
		
		-- Widget pour un utilisateur
		[UserId] [numeric](18, 0) NULL,
		
		-- Position du widget pour le user	
        [PosX] int NULL,	
        [PosY] int NULL,
		
		-- largeur/hauteur pour le user en cours	
        [Width] int NULL,	
        [Height] int NULL 
		
	) ON [PRIMARY]	
	ALTER TABLE [dbo].[XrmWidgetPref] WITH NOCHECK ADD CONSTRAINT [PK_XrmWidgetPref] PRIMARY KEY CLUSTERED ( [XrmWidgetPrefId] ) ON [PRIMARY]
	ALTER TABLE [dbo].[XrmWidgetPref] WITH NOCHECK ADD CONSTRAINT [FK_XrmWidgetPref] FOREIGN KEY ( [XrmGridWidgetId] ) 
	REFERENCES [XrmGridWidget]( [Id] ) 
END
