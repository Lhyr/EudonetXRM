-- Création de PP75 - AVATAR
if not exists (select * from [desc] where descid=275)
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]  ) 	select 275, 'PP', 'PP75', 13, 100
	
if not exists (select * from [res] where resid=275)
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 	select 275, 'Avatar', 'Avatar', 'Avatar', 'Avatar', 'Avatar'	
	
if not exists (select * from sys.columns sc 
inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PP' collate french_ci_ai
where sc.name collate french_ci_ai ='PP75' collate french_ci_ai)
	alter table [PP] add  [PP75] nvarchar(200) null

if not exists (select * from sys.columns sc 
inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PP' collate french_ci_ai
where sc.name collate french_ci_ai ='PP75_NAME' collate french_ci_ai)
	alter table [PP] add  [PP75_NAME] nvarchar(512) null	


if not exists (select * from sys.columns sc 
inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PP' collate french_ci_ai
where sc.name collate french_ci_ai ='PP75_TYPE' collate french_ci_ai)
	alter table [PP] add  [PP75_TYPE] nvarchar(100) null	
	 

-- Création de PM75 - AVATAR	
if not exists (select * from [desc] where descid=375)
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]  ) 	select 375, 'PM', 'PM75', 13, 100
Else
	update [desc] set [file]='PM',[FIELD]='PM75' where descid=375	
	
if not exists (select * from [res] where resid=375)
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 	select 375, 'Avatar', 'Avatar', 'Avatar', 'Avatar', 'Avatar'

if not exists (select * from sys.columns sc 
	inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PM' collate french_ci_ai
	where sc.name collate french_ci_ai ='PM75' collate french_ci_ai)
	alter table [PM] add  [PM75] nvarchar(200) null


if not exists (select * from sys.columns sc 
inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PM' collate french_ci_ai
where sc.name collate french_ci_ai ='PM75_NAME' collate french_ci_ai)
	alter table [PM] add  [PM75_NAME] nvarchar(512) null	


	if not exists (select * from sys.columns sc 
inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'PM' collate french_ci_ai
where sc.name collate french_ci_ai ='PM75_TYPE' collate french_ci_ai)
	alter table [PM] add  [PM75_TYPE] nvarchar(100) null		
	
