
-- Nombre de connexion en echec
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name = 'UserCnxAttempt')
BEGIN            
	ALTER TABLE [USER] ADD [UserCnxAttempt] int  default 0
END
 



-- Nombre de connexion en echec
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name = 'UserLocked')
BEGIN            
	ALTER TABLE [USER] ADD [UserLocked] bit
END
 




IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name = 'UserDateLocked')
BEGIN            
	ALTER TABLE [USER] ADD [UserDateLocked] datetime
END
 