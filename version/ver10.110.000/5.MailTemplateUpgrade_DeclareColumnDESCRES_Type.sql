/***********************************************************************************************************
MAB le 25/08/2015

D�claration de la colonne [Type] sur la table [MAILTEMPLATE] permettant de distinguer le type de mod�le de mail
	0 : Mod�le d'e-mailing
	1 : Mod�le de mail unitaire
	...
***********************************************************************************************************/
if not exists (select * from [desc] where descid=107011)
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]  ) 	select 107011, 'MAILTEMPLATE', 'Type', 13, 300
Else
	update [desc] set [file]='MAILTEMPLATE',[FIELD]='Type' where descid=107011
	
if not exists (select * from [res] where resid=107011)
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 	select 107011, 'Type', 'Type', 'Type', 'Type', 'Type'
