-- Nombre de connexion avec code en echec
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name = 'UserCnxCodeAttempt')
BEGIN            
	ALTER TABLE [USER] ADD [UserCnxCodeAttempt] int default 0
END

-- UPDATE [USER] SET [UserCnxCodeAttempt] = 0 WHERE [UserCnxCodeAttempt] IS NULL