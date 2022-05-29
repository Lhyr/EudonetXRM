set nocount on;
if (Not EXISTS ( select * from [user] where userlogin like 'EDN_NOTIF' ) )
begin
INSERT INTO [USER] ([UserLogin], [UserName], [UserLevel], [Lang] , [UserMail], [UserDisabled], [UserHidden])
VALUES ('EDN_NOTIF', 'Notification', 1, 'LANG_00', 'dev@eudoweb.com', 1, 1)
end