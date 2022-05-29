DECLARE @bEdition2017 BIT = 0

SELECT @bEdition2017 = 1 FROM [CONFIGADV] WHERE Parameter = 'ADMIN_ENABLED' AND Value = '1'

-- Si la base est en édition 2017 et qu'elle n'a pas de clé de licence, on lui fournit une clé de licence par défaut positionnée sur l'offre Standard
IF @bEdition2017 = 1 AND NOT EXISTS(SELECT Id FROM [CONFIGADV] WHERE Parameter = 'LICENSEKEY' AND ISNULL(Value, '') <> '') BEGIN
	-- On supprime la ligne de CONFIGADV si elle existe au cas où
	DELETE FROM [CONFIGADV] WHERE Parameter = 'LICENSEKEY'
	-- On crée la nouvelle
	INSERT INTO [CONFIGADV] (Parameter, Value)  VALUES ('LICENSEKEY', '0Gl9OhP6Dys6MjKKIoq1I/23dOJUECDNRG1RRsa83r8=')
END