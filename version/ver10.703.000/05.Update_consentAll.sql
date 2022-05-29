/*
*	Archive les consentements de meme type pour ne garder que le plus recent
*/

ALTER PROCEDURE [dbo].[esp_archiveConsentAll]
 		 
AS 



-- 1) interaction avec date la plus r�cente
UPDATE [INTERACTION] 
	SET TPL04 = 1
FROM INTERACTION  ii
INNER JOIN (
SELECT AdrId,PpId, TPL06,TPL17,TPL09,  MAX(tpl05) dt, MAX(tpl96) dtModif, MAX(tplid) tplid from INTERACTION  
WHERE  ISNULL(TPL04,0) <>1
GROUP BY  AdrId,PpId, TPL06,TPL17,TPL09  having COUNT(1) > 1 ) as dbl
ON		isnull(ii.AdrId,0) = isnull(dbl.AdrId,0) 
	AND isnull(ii.PpId,0) = dbl.PpId
	AND isnull(ii.TPL06,0) = dbl.TPL06  -- type interaction
	AND isnull(ii.TPL17,0) = dbl.TPL17  -- type de media
	AND isnull(ii.TPL09,0) = dbl.TPL09  -- type de campagne
WHERE  ISNULL(TPL04,0) <>1
AND TPL05 < dbl.dt
AND ii.TPL17 IS NOT NULL
AND ii.TPL06 in (SELECT ISNULL(dataid,-1) FROM FILEDATA WHERE Data = 'consent' AND DESCID = 118006)
AND ii.TPL09 IS NOT NULL
 
-- 2) interaction avec cr�e/modifi� la plus r�cente
UPDATE [INTERACTION] 
	SET TPL04 = 1
FROM INTERACTION  ii
INNER JOIN (
SELECT AdrId,PpId, TPL06,TPL17,TPL09,  MAX(tpl05) dt, MAX(isnuLL(tpl96, tpl95)) dtSys, MAX(tplid) tplid from INTERACTION  
WHERE  ISNULL(TPL04,0) <>1
GROUP BY  AdrId,PpId, TPL06,TPL17,TPL09  having COUNT(1) > 1 ) as dbl
ON		isnull(ii.AdrId,0) = isnull(dbl.AdrId,0) 
	AND isnull(ii.PpId,0) = dbl.PpId
	AND isnull(ii.TPL06,0) = dbl.TPL06  -- type interaction
	AND isnull(ii.TPL17,0) = dbl.TPL17  -- type de media
	AND isnull(ii.TPL09,0) = dbl.TPL09  -- type de campagne
WHERE  ISNULL(TPL04,0) <>1
AND ISNULL(tpl96,tpl95) < dtSys
AND ii.TPL17 IS NOT NULL
AND ii.TPL06 in (SELECT ISNULL(dataid,-1) FROM FILEDATA WHERE Data = 'consent' AND DESCID =118006)
AND ii.TPL09 IS NOT NULL
 
-- 3) interaction avec id le plus �lev�
UPDATE [INTERACTION] 
	SET TPL04 = 1
FROM INTERACTION  ii
INNER JOIN (
SELECT AdrId,PpId, TPL06,TPL17,TPL09,  MAX(tpl05) dt, MAX(tpl96) dtModif, MAX(tpl95) dtCrea, MAX(tplid) maxtplid from INTERACTION  
WHERE  ISNULL(TPL04,0) <>1
GROUP BY  AdrId,PpId, TPL06,TPL17,TPL09  having COUNT(1) > 1 ) as dbl
ON		isnull(ii.AdrId,0) = isnull(dbl.AdrId,0) 
	AND isnull(ii.PpId,0) = dbl.PpId
	AND isnull(ii.TPL06,0) = dbl.TPL06  -- type interaction
	AND isnull(ii.TPL17,0) = dbl.TPL17  -- type de media
	AND isnull(ii.TPL09,0) = dbl.TPL09  -- type de campagne
WHERE  ISNULL(TPL04,0) <>1
AND tplid  < maxtplid 
AND ii.TPL17 IS NOT NULL
AND ii.TPL06 in (SELECT ISNULL(dataid,-1) FROM FILEDATA WHERE Data = 'consent' AND DESCID =118006)
AND ii.TPL09 IS NOT NULL
 