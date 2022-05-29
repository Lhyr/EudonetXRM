set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[NOTIFICATION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[Notification] (
		[NotificationId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,
		[ParentEvtId] [numeric](18, 0) NULL, --clé étrangère sur NotificationTrigger		
		[PpId] [numeric](18, 0) NULL,
		[PmId] [numeric](18, 0) NULL,		
		[Title] [nvarchar](2000) NULL,
		[TitleLong] [nvarchar](2000) NULL,
		[Description] [nvarchar](2000) NULL,
		[DescriptionLong] [nvarchar](max) NULL,
		[DescriptionLong_HTML] [bit] NULL,
		[EmailBody] [nvarchar](max) NULL,			
		[EmailBody_HTML] [bit] NULL,
		[NotificationType] [tinyint] NULL,
		[Icon] [nvarchar](25) NULL,
		[Image] [nvarchar](512) NULL,
		[Image_NAME] [nvarchar](512) NULL,
		[Image_TYPE] [nvarchar](100) NULL,		
		[ExpirationDate] [datetime] NULL, --date purge
		[NotificationDate] [datetime] NULL, --date de notification
		[ParentTab] [numeric](18, 0) NULL, --descid de la table planning parente
		[ParentFileId] [numeric](18, 0) NULL, --id de la fiche planning parente
		[Email] [nvarchar](max) NULL,
		[Telephone] [nvarchar](max) NULL,
		[Read] [bit] NULL, --permet de savoir si une notification a été lue par un destinataire
		[ToastedDate] [datetime] NULL, --date d'affichage sous forme de toaster		
		[NotificationStatus] [tinyint] NULL, --status : en attente de fusion/prête
		[BroadcastType] [tinyint] NULL, -- XRM et mobile, mail, SMS
		[Notification80] [nvarchar](20) NULL, --couleur
		[Notification84] [bit] NULL,
		--[Notification92] [varchar](512) NULL,
		[Notification88] [varchar](512) NULL,
		[Notification95] [datetime] NULL,		
		[Notification96] [datetime] NULL,
		[Notification97] [numeric](18, 0) NULL,
		[Notification98] [numeric](18, 0) NULL,
		[Notification99] [numeric](18, 0) NULL,
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[Notification] WITH NOCHECK ADD CONSTRAINT [PK_Notification] PRIMARY KEY CLUSTERED ( [NotificationId] ) ON [PRIMARY]
END