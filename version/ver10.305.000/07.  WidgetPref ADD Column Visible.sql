

--Ajout de colonne Visible sur la table XrmWidgetPref
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'Visible' and t.name ='XrmWidgetPref')
begin
	alter table [XrmWidgetPref] add [Visible] bit null;
end