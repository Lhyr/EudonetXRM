
/* Script de modification du stockage en base de la gestion des expiration de PJ */
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'PJ' and syscolumns.name like 'ExpireDay' )
BEGIN
	ALTER TABLE [PJ] ADD ExpireDay date NULL
END

/* Ajout ou modif des entrée dans desc et res*/
If not exists (select 1 from [desc] where descid = 102013)
begin
   insert into [desc] ([DescId], [File], [Field], [Format], [default]) select 102013, 'PJ', 'ExpireDay', 10, NULL
   insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 102013, 'Durée de vie des liens par défaut','Default link lifetime','Default link lifetime','Default link lifetime','Default link lifetime'
end
else
begin
	update [desc] set [Field] = 'ExpireDay' where descid = 102013
end
