ALTER FUNCTION [dbo].[getAutoBuildNameAdv] (
	@d DATETIME
	,-- Date créer le
	@sPM01 VARCHAR(max)
	,-- Société
	@sPP01 VARCHAR(max)
	,-- Contacts.Nom
	@sPP02 VARCHAR(max)
	,-- Contacts.Prénom
	@sPP03 VARCHAR(max)
	,-- Contacts.Particule
	@sMask VARCHAR(50)
	,-- DisplayMask de la tabel [filedataparam]
	@sEVT01 VARCHAR(max)
	,@nEVTID NUMERIC
	)
RETURNS TABLE
AS
-- 103 : format jj/mm/yyyy CI - CULTURE INFO
RETURN (
		SELECT TOP 1 res
		FROM [dbo].[getAutoBuildNameAdvCI](@d, @sPM01, @sPP01, @sPP02, @sPP03, @sMask, @sEVT01, @nEVTID, 103)
		)
