
UPDATE PREF
SET DiscCustomFields = Cast((tab + 95) as varchar(10)) + ';' + Cast((tab + 99) as varchar(10)) + ';' + Cast((tab + 94) as varchar(10))
FROM [DESC]
WHERE [DESC].DescId = Pref.Tab
	AND UserId = 0
	AND [DESC].Type = 20
	AND DiscCustomFields IS NULL