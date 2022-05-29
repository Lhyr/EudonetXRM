/* Par défaut le dédoublonnage est actif*/
if exists (SELECT 1 FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'RemoveDoubles' )
BEGIN
   UPDATE [CAMPAIGN] SET RemoveDoubles = 1 WHERE RemoveDoubles IS NULL	
END

