CREATE PROC [dbo].[xsp_SendSMS] @PhoneNumber varchar(50), @SMS  varchar(max), @UserId INT = 0, @Tab INT = 0,  @FileId INT = 0
AS
BEGIN
    
	-- codes d'erreur
	DECLARE @SUCCESS INT = 0
	DECLARE @ERROR_MAIL_SENDING INT = 1
	DECLARE @SMS_SENDING_DISABLED INT = 2
	DECLARE @INVALID_CONFIGADV_PARAM INT = 3
	DECLARE @INVALID_PHONE_NUMBER INT = 4
   
   

    declare @SMSServerEnabled as bit = 0
	declare @SMSMailFrom as varchar(50) = ''	
	declare @SMSMailTo as varchar(50) = ''
	declare @SMSClientId as varchar(50) = ''
	declare @DBName as Varchar(100)= ''	
	
	
	Select  @DBName = DB_NAME()

	--pivot pour récuperer la configadv
	SELECT @SMSServerEnabled = ISNULL([SMS_SERVER_ENABLED], 0),
	       @SMSMailFrom = ISNULL([SMS_MAIL_FROM], ''),
           @SMSMailTo = ISNULL([SMS_MAIL_TO], ''),
	       @SMSClientId = ISNULL([SMS_CLIENT_ID], '')
	 from (SELECT [CONFIGADV].[Parameter], [CONFIGADV].[Value] FROM [CONFIGADV]) as T 
                 PIVOT (MAX([value]) FOR [Parameter] 
				 IN ([SMS_SERVER_ENABLED], [SMS_MAIL_FROM],[SMS_MAIL_TO], [SMS_CLIENT_ID])) as T2;
				 			 
  
     -- Pas activé ou  pas de config on exit
    if(@SMSServerEnabled = 0) return (@SMS_SENDING_DISABLED) 
    if(@SMSMailFrom = '' or @SMSMailTo = '') return (@INVALID_CONFIGADV_PARAM)
		
	 -- Formatage de numéro de téléphone sous le format 0612345678	
	SET @PhoneNumber = REPLACE( @PhoneNumber, '.', '' )
	SET @PhoneNumber = REPLACE( @PhoneNumber, ' ', '' )
	SET @PhoneNumber = REPLACE( @PhoneNumber, '-', '' )
	SET @PhoneNumber = REPLACE( @PhoneNumber, '(', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, ')', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, '/', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, '\', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, '_', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, ':', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, ',', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, ';', '')
	SET @PhoneNumber = REPLACE( @PhoneNumber, '*', '')
	
	-- + est remplacé par 00 puis L'index du pays
	if(CHARINDEX(@PhoneNumber, '+') = 0)
		SET @PhoneNumber = REPLACE( @PhoneNumber, '+', '00')   

    -- code d'erreur de num tel invalide
	if(@PhoneNumber = '' or isnumeric(@PhoneNumber) <> 1) return(@INVALID_PHONE_NUMBER)
	
	DECLARE @Subject Varchar(100) = '[' + @DBName +']'  + CONVERT(varchar(18), @UserId) + '_' + CONVERT(varchar(18), @Tab) + '_' + CONVERT(varchar(18), @FileId)
	
	SET @SMSMailTo = REPLACE(@SMSMailTo, '$phone$',  @PhoneNumber)
   
    -- c'est limité à 8 caractères
	-- ajout d'une commande pour indiquer le nom de l'emeteur
    if(@SMSClientId <> '')
		SET @SMS = @SMS + '@@from '+ SUBSTRING(@SMSClientId, 1, 8) +  '@@'
   
    BEGIN TRY
		
		exec xsp_SendMail
			  @From= @SMSMailFrom
			, @To = @SMSMailTo
			, @Subject = @Subject
			, @Body = @SMS
			, @IsHTML = 1
			, @Importance = 1	
			
		return (@SUCCESS)

    END TRY
	BEGIN CATCH 
	   return(@ERROR_MAIL_SENDING) 
	END CATCH
END				
