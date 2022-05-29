
/* Ajout d'une nouvelle colonne pour les emailing XRM pour d�doublonner les adresses mail*/
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'RatingDetail' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD RatingDetail nvarchar(1000) 
END

 