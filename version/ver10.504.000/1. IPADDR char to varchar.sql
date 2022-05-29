-- MAB - Bug #69 641 - Conversion de IPADDR.IPAddress et IPADDR.SubnetMask en nvarchar(15) au lieu de char(15) pour éviter des espaces superflus à l'enregistrement si la valeur est inférieure à 15 caractères

IF EXISTS (select object_id from sys.objects where name = 'IPADDR' AND TYPE = 'U')
BEGIN
	IF EXISTS (
		select COLUMN_ID from sys.columns where name collate french_ci_ai = 'IPAddress' and object_id = (
		select object_id from sys.objects where name = 'IPADDR' AND TYPE = 'U')
	)
	BEGIN
		ALTER TABLE [IPADDR] ALTER COLUMN [IPAddress] nvarchar(15) not null
	END

	IF EXISTS (
		select COLUMN_ID from sys.columns where name collate french_ci_ai = 'SubnetMask' and object_id = (
		select object_id from sys.objects where name = 'IPADDR' AND TYPE = 'U')
	)
	BEGIN
		ALTER TABLE [IPADDR] ALTER COLUMN [SubnetMask] nvarchar(15) null
	END
END