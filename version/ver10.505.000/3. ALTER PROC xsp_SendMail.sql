/*
	AUTHOR : KHA le 24/10/2014
	ARGUMENTS : 
		@From varchar (255)			Adresse de l'expediteur
		, @ReplyTo varchar(max)		Reply To (facultatif)
		, @To varchar(max)			Adresses des destinataires séparés par un point virgules
		, @CC varchar(max)			Adresses des destinataires en copie séparés par un point virgules
		, @BCC varchar(max)			Adresses des destinataires en copie cachée séparés par un point virgules
		, @Subject nvarchar(255)	Objet du mail
		, @Body nvarchar(255)		Corps 
		, @IsHTML bit = 1			Indique si le mail est en richi ou en texte brut
		, @Importance numeric = 1	Importance du mail (Normal par défaut : 0-Low | 1-Normal | 2-High
		, @PJ nvarchar(max) = null	liste des pièces jointes séparées par des points virgules
		
	EXEMPLE :
		exec xsp_SendMail @From= 'khau@eudoweb.com'
						--, @ReplyTo = 'dev@eudonet.com'
						, @To = 'khau@eudoweb.com;mouarab@eudoweb.com'
						, @CC = 'jasacazes@eudoweb.com;mabbey@eudonet.com'
						, @BCC = 'spham@eudoweb.com;gchenel@eudoweb.com'
						, @Subject ='testxsp_SendMail test 2'
						, @Body = 'Pouvez vous me re-dire si vous recevez ce mail ? (je teste les destinataires multiples en principal, cc et bcc) <br> Karen'
						, @IsHTML = 1
						, @Importance = 1
						, @PJ = 'D:\khau\Documents\annexes potentielles\txtsuperlong.txt;d:\khau\Documents\annexes potentielles\Lafontaine - le corbeau et le renard.txt'	
						
*/

ALTER PROC dbo.xsp_SendMail @From varchar (255)
						, @ReplyTo varchar(max)=null
						, @To varchar(max)
						, @CC varchar(max) = null
						, @BCC varchar(max) = null
						, @Subject nvarchar(255) = null
						, @Body  nvarchar(max) = null
						, @IsHTML bit = 1
						, @Importance numeric = 1
						, @PJ nvarchar(max) = null
AS
BEGIN

	DECLARE @dateObsolete AS DATETIME
		, @dateObsoleteForFailed AS DATETIME

	SELECT @dateObsolete = dateadd(day, - 30, getdate())
		, @dateObsoleteForFailed = dateadd(day, - 90, getdate())

	-- on supprime tous les mails envoyés depuis plus de 3 mois quelque soit leur statut d'envoi.
	EXEC msdb.dbo.sysmail_delete_mailitems_sp @sent_before = @dateObsoleteForFailed

	-- on supprime tous les mails envoyés avec succès depuis plus d'1 mois.
	EXEC msdb.dbo.sysmail_delete_mailitems_sp @sent_before = @dateObsolete
		, @sent_status = 'sent'


	DECLARE @sBodyFormat as varchar(20), @sImportance as varchar(6)

	SELECT @sImportance = CASE @Importance WHEN 0 THEN 'Low'
			WHEN 1 THEN 'Normal'
			WHEN 2 Then 'High'
			END
			
	SELECT @sBodyFormat = CASE WHEN @IsHTML = 1 THEN 'HTML' ELSE 'TEXT' END
	
	DECLARE @nReturnCode int

	EXEC @nReturnCode = msdb.dbo.sp_send_dbmail @profile_name = 'xsp_sendMailProfile'
												, @recipients = @To
												, @copy_recipients = @CC
												, @blind_copy_recipients = @BCC
												, @from_address = @From
												, @reply_to = @ReplyTo
												, @subject = @Subject
												, @body = @Body
												, @body_format = @sBodyFormat
												, @importance = @sImportance
												, @file_attachments = @PJ
	
	/*Incrémentation du compteur*/
	IF @nReturnCode = 0
	BEGIN
		DECLARE @nCounterID AS NUMERIC(18)
		DECLARE @sCounterVolume AS VARCHAR(255)		
		DECLARE @sCounterType AS VARCHAR(20) = 'XSP_SENDMAIL'
		DECLARE @dCounterDate DATETIME

		SET @dCounterDate = getdate()

		SELECT @nCounterID = [ID]
		, @sCounterVolume = [CounterVolume]
		FROM [EDNMSGCMPT]
		WHERE UPPER(LTRIM(RTRIM([CounterType]))) = @sCounterType
		AND YEAR([CounterDate]) = YEAR(@dCounterDate)
        AND MONTH([CounterDate]) = MONTH(@dCounterDate)

		IF ISNULL(@nCounterID, 0) <> 0
		BEGIN
			DECLARE @nCounterVolume AS NUMERIC(18) = 0

			IF ISNUMERIC(@sCounterVolume) = 1
			BEGIN
				SET @nCounterVolume = CAST(@sCounterVolume AS NUMERIC(18))
			END
			
			SET @nCounterVolume = @nCounterVolume + 1
			
			UPDATE [EDNMSGCMPT] SET [CounterType] = @sCounterType
			, [CounterVolume] = CAST(@nCounterVolume AS VARCHAR(255))
			, [LastUpdated] = @dCounterDate
			WHERE [ID] = @nCounterID
		END
		ELSE
		BEGIN
			INSERT INTO [EDNMSGCMPT] ([CounterType], [CounterDate], [CounterVolume], [LastUpdated])
			VALUES (@sCounterType,  @dCounterDate, CAST(1 AS VARCHAR(255)), @dCounterDate)
		END
	END
	
END						