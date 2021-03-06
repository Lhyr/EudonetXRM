/**
*	Récupère les permissions 
*	SPH 
*	18/12/2014 : GCH - si permmode null : on a les droits de visu. (bug #35938)
select * from [dbo].[cfc_getPermInfo](29,99,1 ) where [permissionid] in(319,	320,	321,	322)
*	CRU 10/05/2015 : Si le mode est � NONE, on a les droits �galement
 
*/
CREATE FUNCTION [dbo].[xfc_getPermInfo] (
	@userid AS NUMERIC
	,@userlevel AS NUMERIC
	,@groupid AS NUMERIC
	,@permid AS NUMERIC
	)
RETURNS INT
AS
BEGIN
	DECLARE @res INT

	SELECT @res = CASE 
			WHEN P.mode IS NULL
				THEN 1
			WHEN P.mode = - 1
				THEN 1
			WHEN P.mode = 0
				THEN CASE 
						WHEN @userlevel >= P.LEVEL
							THEN 1
						ELSE 0
						END
			WHEN P.mode = 1
				THEN CASE 
						WHEN charindex(';' + cast(@userid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
							OR charindex(';G' + cast(@groupid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
							THEN 1
						ELSE 0
						END
			WHEN P.mode = 3
				THEN CASE 
						WHEN (@userlevel >= P.LEVEL)
							AND (
								charindex(';' + cast(@userid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
								OR charindex(';G' + cast(@groupid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
								)
							THEN 1
						ELSE 0
						END
			WHEN P.mode = 2
				THEN CASE 
						WHEN (@userlevel >= P.LEVEL)
							OR (
								charindex(';' + cast(@userid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
								OR charindex(';G' + cast(@groupid AS VARCHAR(50)) + ';', ';' + P.[user] + ';') > 0
								)
							THEN 1
						ELSE 0
						END
			ELSE 99
			END
	FROM [permission] P
	WHERE p.PermissionId = @permid

	RETURN @res
END
