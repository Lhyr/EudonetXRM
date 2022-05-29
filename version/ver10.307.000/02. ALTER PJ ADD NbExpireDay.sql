
/* Ajout d'une nouvelle colonne pour les emailing XRM pour stocker le nom de domaine partenaire*/
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'PJ' and syscolumns.name like 'NbExpireDay' )
BEGIN
	ALTER TABLE [PJ] ADD NbExpireDay int NULL
END

/* Ajout des entrée dans desc et res*/
If  not exists(	select 1 from [desc] where descid = 102013)
begin
   insert into [desc] ([DescId], [File], [Field], [Format], [default]) select 102013, 'PJ', 'NbExpireDay', 10, NULL
   insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 102013, 'Durée de vie des liens par défaut','Default link lifetime','Default link lifetime','Default link lifetime','Default link lifetime'
end