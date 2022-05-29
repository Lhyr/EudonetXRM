
/* Ajout d'une nouvelle colonne pour les emailing XRM pour dédoublonner les adresses mail*/
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'RemoveDoubles' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD RemoveDoubles BIT NULL DEFAULT(1)
END

/* Ajout des entrée dans desc et res*/
If  not exists(	select 1 from [desc] where descid = 106032)
begin
   insert into [desc] ([DescId], [File], [Field], [Format], [Length], [default], [disporder]) select 106032, 'CAMPAIGN', 'RemoveDoubles', 3, 1, 1, 99
   insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 106032, 'Dédoublonner les adresses mail','Remove doubled mail adresses','Remove doubled mail adresse','Remove doubled mail adresse','Remove doubled mail adresse'
    
end


