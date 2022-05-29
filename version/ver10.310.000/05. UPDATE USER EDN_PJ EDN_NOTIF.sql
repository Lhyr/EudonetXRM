BEGIN

	set nocount on
	declare @ui as int

	SELECT @ui= USERID  FROM [USER]  where UserLogin = 'EDN_PJ' 
	exec esp_creapref @ui,0
	
	SELECT @ui= USERID  FROM [USER]  where UserLogin = 'EDN_NOTIF' 
	exec esp_creapref @ui,0
	
	set nocount off	

END