
/* Ajout d'une nouvelle colonne pour les emailing XRM pour stocker le nom de domaine partenaire*/
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'SenderAliasDomain' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD SenderAliasDomain NVARCHAR(255) NULL
END

/* Ajout des entrée dans desc et res */
/* CNA - N'est plus renférencé par EudoQuery */
/*
If  not exists(	select 1 from [desc] where descid = 106033)
begin
   insert into [desc] ([DescId], [File], [Field], [Format], [Length], [default], [disporder]) select 106033, 'CAMPAIGN', 'SenderAliasDomain', 1, 255, NULL, 0
   insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 106033, 'Domaine partenaire','Partner domain','Partner domain','Partner domain','Partner domain'
    
end
*/