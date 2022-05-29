/* Recuperation de l'existant */
if exists (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'PJ' and syscolumns.name like 'NbExpireDay' )
BEGIN
	exec ('
	if exists (SELECT 1 FROM [PJ] WHERE NbExpireDay <> -100)
	BEGIN
		-- Comme on ne connait pas la date exacte de l''expiration, on met 1900-01-01

		-- -2 : le lien d''accès à l''annexe a expiré (si date passé => acces interdit)
		--  0 : expiré
		exec(''update [PJ] set ExpireDay = CONVERT(DATETIME, 0) where NbExpireDay in (-2, 0)'')

		-- > 0 : nb de jour de la disponibilité de la PJ par rapport à PJ_DATE de celle-ci
		exec(''update [PJ] set ExpireDay = CONVERT(DATETIME, 0) where NbExpireDay > 0 and cast(DATEADD(DAY, NbExpireDay, PjDate) as date) < cast(GETDATE() as date)'')

		ALTER TABLE [PJ] DROP COLUMN NbExpireDay
	END
	')
END

