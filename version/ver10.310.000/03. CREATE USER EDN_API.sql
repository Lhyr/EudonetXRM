IF NOT EXISTS( SELECT 1 FROM [USER] where UserLogin = 'EDN_API')
BEGIN

set nocount on
	--DECLARE  @T TABLE (R INT)
	declare @ui as int

	INSERT INTO [USER] (UserLogin, UserName, UserLevel, Lang, UserMail, UserDisabled, UserHidden, [Password], [EncryptPassword] ) 
	values ('EDN_API','USER API',1,'LANG_00','dev@eudoweb.com',1,1, LOWER( dbo.efc_GetHash( NEWID() ,'MD5',0,0,0)),LOWER( dbo.efc_GetHash( NEWID(),'MD5',0,0,0)))

	
	select  @ui= SCOPE_IDENTITY()

	exec esp_creapref @ui,0
	
	set nocount off	

END

 