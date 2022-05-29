--Suppression de la colonne dans XrmGrid
if exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ShowWidgetTitle' and t.name ='XrmGrid')
begin
	alter table [XrmGrid] DROP COLUMN [ShowWidgetTitle];	
end

--Suppression de la ligne dans DESC et RES
DELETE FROM [DESC] where DescId = 115004;
DELETE FROM [RES] where ResId = 115004;

--Insertion de la colonne dans XrmWidget
if not exists(select 1 from sys.tables t inner join sys.columns c on t.object_id = c.object_id  where c.name = 'ShowWidgetTitle' and t.name ='XrmWidget')
begin
	alter table [XrmWidget] add [ShowWidgetTitle] bit null;
end
  
--Insertion de la ligne dans DESC et RES
if not exists(select 1 from [DESC] where DescId = 115120)
begin
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT 115120, 'XrmWidget', 'ShowWidgetTitle', 3, 1	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT 115120, 'Afficher les titres des widgets', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title'
end

