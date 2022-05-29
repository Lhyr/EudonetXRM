
/* Ajout d'une nouvelle colonne le score des campagne mail*/
IF NOT EXISTS (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'Rating' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD Rating numeric(18,0) 
END

DELETE FROM [DESC] WHERE [DESCID] = 106051
DELETE FROM [RES] WHERE [RESID] = 106051

INSERT into [desc] ([DescId], [File], [Field], [Format], [Length], [default], [disporder]) select 106051, 'CAMPAIGN', 'Rating', 10, 0, 0, max(disporder) +1 FROM [DESC] where [File] = 'CAMPAIGN'
		AND descid % 100 > 0 and descid % 100 < 69 and DispOrder <> 99

		
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 106051, 'Score','Rating','Rating','Rating','Rating'
    
UPDATE [DESC] SET [ReadOnly] = 1 WHERE [DESCID] = 106051

