
/*
SPH - MAJ le 12/02/2016
Maj de la Function de split en utilisant une cte
 a utiliser avec 		OPTION(MAXRECURSION 0)  si on split +100 lignes
*/
create  FUNCTION [dbo].[efc_splitCTE](@sChaine as varchar(max), @sep as varchar(5)  ) 
	RETURNS table AS 
 
RETURN
(
    WITH CTE_SPLIT(curpos,endpos)
    AS(
        SELECT cast (0 as int) AS curpos, CHARINDEX(@sep,@sChaine) AS endpos --  init : position 0 jusqu'au 1er sep 
        UNION ALL
        SELECT cast(endpos+1 as int), CHARINDEX(@sep,@sChaine,endpos+1) -- partie récurisve : position : dernier sep +1 jusqu'au sep suivant
            FROM CTE_SPLIT
            WHERE endpos > 0
    )

	-- La cte retourne une ligne pour chaque élément de la chaine csv avec 2 colonne : la position de début et celle de fin de l'élément dans la liste
	--  il ne reste plus qu'a retourner : 1 la position de l'élément (rownumbner sur la 1er col, et l'élment, le substring entre la col 1 et la 2)

    SELECT 'N' = ROW_NUMBER() OVER (ORDER BY (SELECT curpos)),
        'ELEMENT' =  SUBSTRING(@sChaine,curpos,COALESCE(NULLIF(endpos,0),LEN(@sChaine)+1)-curpos) 
    FROM CTE_SPLIT  
)
