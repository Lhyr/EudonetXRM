-- #44174 : Migration - Passage des derniers filtres non sauvegardés V7 en XRM

UPDATE [FILTER]
SET [Type] = 0
WHERE [Type] IS NULL
	AND (
		Libelle IS NULL
		OR Libelle = ''
		)
