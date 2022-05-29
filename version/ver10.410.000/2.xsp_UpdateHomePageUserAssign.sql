
/*
*	SPH 22/10/2015
*	Met à jour 'UserAssign' sur les pages d'accueil
*    -> retire les userassign de la page fourni en param des autres pages
*
*/

create PROCEDURE [dbo].[xsp_UpdateHomePageUserAssign]
	@hpgid numeric,		-- Id Accueil
	@content varchar(max)
AS 


	UPDATE XrmHomePage
	SET UserAssign = NameValues
	FROM (
		SELECT XrmHomePageid
			,STUFF((
					SELECT ';' + element
					FROM XrmHomePage
					CROSS APPLY efc_split(UserAssign, ';')
					WHERE Element NOT IN (SELECT element from efc_split(@content, ';'))
							 
						AND XrmHomePageId <> @hpgid
						AND aa.XrmHomePageId = XrmHomePageId
					FOR XML PATH('')
						,TYPE
					).value('(./text())[1]', 'VARCHAR(MAX)'), 1, 1, '') AS NameValues
		FROM XrmHomePage aa
		WHERE aa.XrmHomePageid <> @hpgid
		GROUP BY XrmHomePageid
			--  order by XrmHomePageid
		) dedupUser
	WHERE dedupUser.XrmHomePageId = XrmHomePage.XrmHomePageid
		AND XrmHomePage.XrmHomePageid <> @hpgid
