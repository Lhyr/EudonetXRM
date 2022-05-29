
UPDATE BKMPREF
SET bkmcol = REPLACE(REPLACE(';' + bkmcol + ';', ';492;', ';'), ';301;', ';'), bkmcolwidth = '130'
WHERE tab = 200
	AND bkm = 400
	AND bkmcol IS NOT NULL

UPDATE BKMPREF
SET bkmcol = SUBSTRING(bkmcol, 2, len(bkmcol) - 2)
WHERE tab = 200
	AND bkm = 400
	AND bkmcol LIKE ';%;'

UPDATE BKMPREF
SET bkmcol = '301;492;' + isnull(bkmcol, '')
WHERE tab = 200
	AND bkm = 400
	
	
UPDATE BKMPREF
SET bkmcol = REPLACE(';' + bkmcol + ';', ';201;', ';'), bkmcolwidth = '130'
WHERE tab = 300
	AND bkm = 400
	AND bkmcol IS NOT NULL

UPDATE BKMPREF
SET bkmcol = SUBSTRING(bkmcol, 2, len(bkmcol) - 2)
WHERE tab = 300
	AND bkm = 400
	AND bkmcol LIKE ';%;'

UPDATE BKMPREF
SET bkmcol = '201;' + isnull(bkmcol, '')
WHERE tab = 300
	AND bkm = 400