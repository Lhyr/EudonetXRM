
create FUNCTION [dbo].[cfc_getIDs]() RETURNS table AS 
 return 
 SELECT (a3.id +a2.id + a1.id + a0.id ) N -- Compteur (1 - 10.000) 
 FROM 
(        
           SELECT 0 id UNION ALL 
            SELECT 1 UNION ALL 
            SELECT 2 UNION ALL 
            SELECT 3 UNION ALL 
            SELECT 4 UNION ALL 
            SELECT 5 UNION ALL 
            SELECT 6 UNION ALL 
            SELECT 7 UNION ALL 
            SELECT 8 UNION ALL 
           SELECT 9 
) a0 
CROSS JOIN 
( 
            SELECT 0 id UNION ALL 
            SELECT 10 UNION ALL 
            SELECT 20 UNION ALL 
            SELECT 30 UNION ALL 
            SELECT 40 UNION ALL 
            SELECT 50 UNION ALL 
            SELECT 60 UNION ALL 
            SELECT 70 UNION ALL 
            SELECT 80 UNION ALL 
            SELECT 90 
) a1 
CROSS JOIN 
( 
            SELECT 0 id UNION ALL  
            SELECT 100 UNION ALL 
            SELECT 200 UNION ALL 
            SELECT 300 UNION ALL 
            SELECT 400 UNION ALL 
            SELECT 500 UNION ALL 
            SELECT 600 UNION ALL 
            SELECT 700 UNION ALL 
            SELECT 800 UNION ALL 
           SELECT 900 
 ) a2 
 CROSS JOIN 
 ( 
 SELECT 0 id UNION ALL 
 SELECT 1000 UNION ALL 
 SELECT 2000 UNION ALL 
 SELECT 3000 UNION ALL 
 SELECT 4000 UNION ALL 
 SELECT 5000 UNION ALL 
 SELECT 6000 UNION ALL 
 SELECT 7000 UNION ALL 
 SELECT 8000 UNION ALL 
 SELECT 9000 
) a3
