If Not Exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'DESC' and syscolumns.name like 'BkmViewPermId_400' )
BEGIN
	ALTER TABLE [DESC] ADD BkmViewPermId_400 VARCHAR(500)
END
  
If Not Exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'DESC' and syscolumns.name like 'BkmViewRulesId_400' )
BEGIN
	ALTER TABLE [DESC] ADD BkmViewRulesId_400 VARCHAR(500)
END
  
If Not Exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'DESC' and syscolumns.name like 'BkmAddRulesId_400' )
BEGIN
	ALTER TABLE [DESC] ADD BkmAddRulesId_400 VARCHAR(500)
END
  
  