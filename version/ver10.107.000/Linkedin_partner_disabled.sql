IF NOT EXISTS (
		SELECT 1
		FROM [dbo].syscolumns
		WHERE NAME = 'LinkedInDisabled'
			AND id = object_id('[CONFIG]')
		)
	ALTER TABLE CONFIG ADD LinkedInDisabled BIT DEFAULT 0

IF NOT EXISTS (
		SELECT 1
		FROM [dbo].syscolumns
		WHERE NAME = 'PartnerLinkDisabled'
			AND id = object_id('[CONFIG]')
		)
	ALTER TABLE CONFIG ADD PartnerLinkDisabled BIT DEFAULT 0
