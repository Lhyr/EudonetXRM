IF NOT EXISTS( SELECT 1 FROM [USER] where UserLogin = 'EDN_PJ')
BEGIN
	INSERT INTO [USER] (UserLogin, UserName, UserLevel, Lang, UserMail, UserDisabled, UserHidden) 
	values ('EDN_PJ','PJ EXT',99,'LANG_00','dev@eudoweb.com',1,1)
END