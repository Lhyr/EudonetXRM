
/* Ajout d'une nouvelle colonne pour les emailing XRM pour dédoublonner les adresses mail*/
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'MailTesterInfos' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD MailTesterInfos nvarchar(1000) 
END

 