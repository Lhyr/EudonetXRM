UPDATE REPORT SET [Param] =
	CASE 
		WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') NOT LIKE '%displaylegend=%'THEN
			CASE 
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') NOT LIKE '%displayx=%'THEN
					ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') + '&displayx=1&lstdisplayx=1&displaylegend=1&lstdisplaylegend=1'
					
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') LIKE '%displayx=0%' THEN
					REPLACE(ISNULL(CAST (PARAM AS VARCHAR(MAX)),''),'displayx=0','displayx=1&displaylegend=1&lstdisplaylegend=1')
					
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') LIKE '%displayx=1%' THEN
					REPLACE(ISNULL(CAST (PARAM AS VARCHAR(MAX)),''),'displayx=1','displayx=1&displaylegend=1&lstdisplaylegend=1')
					
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') LIKE '%displayx=2%'THEN
					REPLACE(ISNULL(CAST (PARAM AS VARCHAR(MAX)),''),'displayx=2','displayx=1&displaylegend=1&lstdisplaylegend=1')
					
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') LIKE '%displayx=3%'THEN
					REPLACE(ISNULL(CAST (PARAM AS VARCHAR(MAX)),''),'displayx=3','displayx=1&displaylegend=1&lstdisplaylegend=1')
					
				WHEN ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') LIKE '%displayx=&%'THEN
					REPLACE(ISNULL(CAST (PARAM AS VARCHAR(MAX)),''),'displayx=','displayx=1&displaylegend=1&lstdisplaylegend=1')
					
				ELSE [Param]
			END
		ELSE [Param]
	END

WHERE [Type] = 6 AND ISNULL(CAST (PARAM AS VARCHAR(MAX)),'') <> ''