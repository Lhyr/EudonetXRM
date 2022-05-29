set nocount on;
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[NotificationTrigger]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[NotificationTrigger] (
		[NotificationTriggerId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL, -- L'id de declancheur
        [PpId] [numeric](18, 0) NULL, -- PP attache au declencheeur
        [PmId] [numeric](18, 0) NULL, -- PM attache au declencheeur
        [ParentEvtId] [numeric](18, 0) NULL, -- Event parent rattaché	
		
		-- Titre de déclencheur
		[Label] [varchar](1000) NULL,
		
		-- Image du declencheeur		
		[Icon] [varchar](25) NULL, -- url de l'icone
		[Color] [varchar](20) NULL, -- color en string	
		[Image] [numeric](15, 0) NULL, -- descid de champs de type Image ou avatar(75)			
		[ImageSource] [tinyint] NULL, -- source de l'image		
		
		-- evenements de declenchement		
		[TriggerAction] [int] NULL, -- Ajout/Modification/Suppression/fusionn/basée sur temps [catalogue multiple ?
		[TriggerTargetDescid] [int] NULL, -- 200 pour toute la table PP, 201 que pour le PP01
		
		-- conditions de déclenchement
		[ConditionalFilterIdTrigger] [numeric](18, 0) NULL, -- condition basé sur un filtre 	
		[ConditionalSqlTrigger] [varchar](1000) NULL, -- code sql select
			
		-- Action de la notification
	    [NotificationPriority][tinyint] NULL, -- priorité de la notification        
		[NotificationType][tinyint] NULL,	-- A la validation / à un instant donné / incitative 	
		
		-- Proprietes de la notification
		[BroadcastType] [tinyint] NULL, -- XRM et mobile, mail, SMS	
		[TriggerDate] [datetime] NULL, -- date de la notification 
		[ExpirationDate] [datetime] NULL, -- date purge de toutes les notifs issues de ce déclencheeur 
		
		-- Destinataire
		[Subscribers][varchar](512) NULL, -- ids des groubes/utilisateurs abonnés à recevoir la notification issue de ce declencheeur
		[SubscribersTargetDescId][int] NULL, -- ids des groubes/utilisateurs abonnés à recevoir la notification issue de ce declencheeur
		[SubscribersFreeEmailField][varchar](500) NULL, -- si la BroadcastType de type email alors liste des mails séparé par ";" 
		[SubscribersFreeTelField][varchar](500) NULL, -- si la BroadcastType de type sms/mobile alors liste des numéro tel séparé par ";" 	
		
		-- params systeme
		[NotificationTrigger84] [bit] NULL,	 -- confidentielle
		[NotificationTrigger88] [varchar](512) NULL,
		[NotificationTrigger95] [datetime] NULL,
		[NotificationTrigger96] [datetime] NULL,
		[NotificationTrigger97] [numeric](18, 0) NULL,
		[NotificationTrigger98] [numeric](18, 0) NULL,
		[NotificationTrigger99] [numeric](18, 0) NULL,
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[NotificationTrigger] WITH NOCHECK ADD CONSTRAINT [PK_NotificationTrigger] PRIMARY KEY CLUSTERED ( [NotificationTriggerId] ) ON [PRIMARY]
END