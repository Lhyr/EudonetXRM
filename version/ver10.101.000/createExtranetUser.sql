
set nocount on;
IF (SELECT COUNT(1) FROM [USER] WHERE [USER].[USERLOGIN] = 'EDN_EXTRA_00') = 0
BEGIN

	INSERT INTO [USER] ([USERLOGIN], [PASSWORD], [USERNAME], [USERLEVEL], [USERHIDDEN], [USERDISABLED], [LANG], [USERMAIL]) 
		VALUES ('EDN_EXTRA_00', null, 'Extranet FR', '99', '1', '1', 'LANG_00', 'dev@eudoweb.com')
		
	DECLARE @userid as int
	SELECT @userid = userid FROM [USER] WHERE [USERLOGIN] = 'EDN_EXTRA_00'
	exec esp_creaPref @userid, 0

END
