IF NOT EXISTS (
	SELECT * from sys.columns sc 
		inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'STATLOG' collate french_ci_ai
		where sc.name collate french_ci_ai ='TOKEN' collate french_ci_ai
	)	
	ALTER TABLE [STATLOG] ADD  [TOKEN] NVARCHAR(4000)

	
	IF NOT EXISTS (
	SELECT * from sys.columns sc 
		inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = 'STATLOG' collate french_ci_ai
		where sc.name collate french_ci_ai ='PRODUCTNAME' collate french_ci_ai
	)	
	ALTER TABLE [STATLOG] ADD  [PRODUCTNAME] NVARCHAR(500)