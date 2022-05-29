/*supprimer les "Derniers filtres non sauvegardés" qui n'appartiennent à aucun utilisateur*/
DELETE
FROM FILTER
WHERE ISNULL(Libelle, '') = ''
	AND ISNULL(Type, 0) = 0
	AND NOT EXISTS (
		SELECT UserId
		FROM [USER] u
		WHERE u.UserId = FILTER.UserId
		)

/*Supprimer les "Derniers filtres non sauvegardés" qui sont en doublon pour un même utilisateur et un même onglet en conservant le dernier modifié*/
DELETE
FROM FILTER
WHERE ISNULL(Libelle, '') = ''
	AND ISNULL(Type, 0) = 0
	AND EXISTS (
		SELECT FilterId
		FROM FILTER dbl
		WHERE dbl.UserId = FILTER.UserId
			AND dbl.Tab = FILTER.Tab
			AND ISNULL(dbl.Type, 0) = ISNULL(FILTER.Type, 0)
			AND dbl.Libelle = FILTER.Libelle
			AND dbl.DateLastModified > FILTER.DateLastModified
		)
