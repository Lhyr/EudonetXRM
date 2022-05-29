
-- Nombre de connexion en echec
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'STATLOG' and syscolumns.name = 'FAILED')
BEGIN            
	ALTER TABLE [STATLOG] ADD [FAILED] bit
END
 
