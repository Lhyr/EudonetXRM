set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[NotificationUnsubscriber]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[NotificationUnsubscriber] (		
		[NotificationUnsubscriberId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,		
		[EvtId] [numeric](18, 0) NULL, --clé étrangère sur Notification
		[PpId] [numeric](18, 0) NULL,
		[PmId] [numeric](18, 0) NULL,
		[AdrId] [numeric](18, 0) NULL,
		[NotificationUnsubscriber01] [numeric](18, 0) NULL,
		[Email] [nvarchar](max) NULL,
		[Telephone] [nvarchar](max) NULL,
		[Unsubscribe] [bit] NULL, --permet de savoir si une notification a été lue par un destinataire
		[NotificationUser84] [bit] NULL,
		[NotificationUser92] [varchar](512) NULL,
		[NotificationUser95] [datetime] NULL,		
		[NotificationUser96] [datetime] NULL,
		[NotificationUser97] [numeric](18, 0) NULL,
		[NotificationUser98] [numeric](18, 0) NULL,
		[NotificationUser99] [numeric](18, 0) NULL		 
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[NotificationUnsubscriber] WITH NOCHECK ADD CONSTRAINT [PK_NotificationUnsubscriber] PRIMARY KEY CLUSTERED ( [NotificationUnsubscriberId] ) ON [PRIMARY]
	--ALTER TABLE [dbo].[TriggerAffectation] ADD CONSTRAINT [FK_TriggerAffectation_NotificationTrigger] FOREIGN KEY ( [NotificationTriggerId] ) REFERENCES [dbo].[NotificationTrigger]([NotificationTriggerId])
	
	
END