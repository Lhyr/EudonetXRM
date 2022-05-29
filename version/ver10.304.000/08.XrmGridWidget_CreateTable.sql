
set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmGridWidget]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[XrmGridWidget] (
		
		[Id] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,  
     
		-- Liaison vers un widget
		[XrmWidgetId] [numeric](18, 0) NOT NULL,
	     
		-- Liaison vers la grille si null, (propo : on attache le wiget document.body)
		[XrmGridId] [numeric](18, 0) NULL,
		
		
	) ON [PRIMARY]	
	ALTER TABLE [dbo].[XrmGridWidget] WITH NOCHECK ADD CONSTRAINT [PK_XrmGridWidget] PRIMARY KEY CLUSTERED ( [Id] ) ON [PRIMARY]
END
