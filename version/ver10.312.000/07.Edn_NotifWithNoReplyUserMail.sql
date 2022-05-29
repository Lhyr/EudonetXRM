UPDATE [User] 
SET [UserMail] = 'noreply@eudonet.com' 
WHERE [UserLogin] = 'EDN_NOTIF' AND ([UserMail] IS NULL OR [UserMail] = 'dev@eudonet.com' OR [UserMail] = 'dev@eudoweb.com')