
IF EXISTS (SELECT 1 FROM [CONFIG] WHERE [UserId] = 0 AND SMSServerEnabled = 1)
AND NOT EXISTS 
(
      SELECT 1 FROM [CONFIGADV] WHERE [Parameter] in ('SMS_SERVER_ENABLED','SMS_MAIL_FROM', 'SMS_MAIL_TO','SMS_CLIENT_ID')
)
BEGIN
 
    Declare @ClientId varchar(30)
	Declare @MailFrom varchar(100)
	Declare @MailTo varchar(100)  
	
	select @clientId = SMSClientID, @MailFrom = SMSMailFrom, @MailTo=SMSMailTo from [CONFIG] where userid = 0
    
	INSERT INTO [CONFIGADV] ([Parameter], [Value]) values ('SMS_SERVER_ENABLED', '1')
	INSERT INTO [CONFIGADV] ([Parameter], [Value]) values ('SMS_MAIL_FROM', @MailFrom)
	INSERT INTO [CONFIGADV] ([Parameter], [Value]) values ('SMS_MAIL_TO', @MailTo)
	INSERT INTO [CONFIGADV] ([Parameter], [Value]) values ('SMS_CLIENT_ID', @ClientId)
	
END