if not exists(select * from EUDORES..TRAIT where traitid = 118 )
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (118, 'Importer en mode onglet', 'Import from tab', 'Import from tab')
end
    
if not exists(select * from EUDORES..TRAIT where traitid = 119 )
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (119, 'Importer en mode signet', 'Import from book mark', 'Import from book mark')
end

