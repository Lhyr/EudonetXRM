INSERT INTO [DESCADV] (DescId, Parameter, Category, Value)
SELECT DescId, 34, 1, CASE 
		WHEN EXISTS (
				SELECT TOP 1 DescId
				FROM [DESC] Field
				WHERE Field.DescId BETWEEN Tab.DescId + 1
						AND Tab.DescId + 99
					AND (
						X IS NOT NULL
						AND Y IS NOT NULL
						)
				)
			THEN 1
		ELSE 0
		END
FROM [DESC] Tab
WHERE DescId BETWEEN 200
		AND 400
	AND DescId % 100 = 0
	AND NOT EXISTS (
		SELECT TOP 1 DescId
		FROM [DESCADV] Dbl
		WHERE Dbl.DescId = Tab.DescId
			AND Dbl.Parameter = 34
		)