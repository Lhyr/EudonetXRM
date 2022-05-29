/* Ajout d'une nouvelle colonne pour les mappings pour indiquer si on ajoute la valeur dans le catalogue dans le cas des adresses prédictives */
IF NOT EXISTS (SELECT 1
		FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name LIKE 'FILEMAP_PARTNER' AND syscolumns.name LIKE 'CreateCatalogValue' )
BEGIN
	ALTER TABLE [FILEMAP_PARTNER] ADD CreateCatalogValue BIT NULL
END