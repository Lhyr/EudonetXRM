set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[NotificationTriggerRes]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[NotificationTriggerRes] (
		[NotificationTriggerResId] [numeric](18, 0) IDENTITY(1,1) NOT NULL, 
		[NotificationTriggerId] [numeric](18, 0) NOT NULL, -- L'id de declancheur	
        [Lang] [nvarchar] (7) NULL,		
		[ShortTitle] [varbinary](max) NULL, -- Titre simple de la notification (pour le regroupement) (avec champs de fusion – idem mails unitaires)
		[LongTitle] [varbinary](max) NULL, -- Titre long de la notification (avec champs de fusion – idem mails unitaires)
		[ShortDescription] [varbinary](max) NULL,
		[LongDescription] [varbinary](max) NULL,		
		[EmailDescription] [varbinary](max) NULL		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[NotificationTriggerRes] WITH NOCHECK ADD CONSTRAINT [PK_NotificationTriggerRes] PRIMARY KEY CLUSTERED ( [NotificationTriggerResId] ) ON [PRIMARY]
END