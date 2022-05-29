/* Pour rester compatible avec les anciennes version de l'ORM, nous ne pouvons pas supprimer la colonne NbExpireDay */
if not exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'PJ' and syscolumns.name like 'NbExpireDay' )
BEGIN
	ALTER TABLE [PJ] ADD NbExpireDay int NULL
	
	exec('UPDATE PJ SET NbExpireDay = -100')
END