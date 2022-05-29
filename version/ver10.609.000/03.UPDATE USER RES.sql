UPDATE [RES] set 
	LANG_00 = [0],
	LANG_01 = [1],
	LANG_02 = [2],
	LANG_03 = [3],
	LANG_04 = [4],
	LANG_05 = [5],
	LANG_06 = [6],
	LANG_07 = [7],
	LANG_08 = [8],
	LANG_09 = [9],
	LANG_10 = [10]

from
(
SELECT * FROM (SELECT [RESID],[ID_LANG], [LANG] FROM EUDORES..[RESAPP] where resid=2679) a PIVOT
( MAX( [LANG] ) FOR [id_lang] IN ([0],[1],[2],[3],[4],[5],[6],[7],[8],[9],[10])) as m -- order by resid
) z
where [RES].[RESid] = 101018

DELETE FROM [RESADV] WHERE [TYPE] = 2 AND [DESCID] = 101018

INSERT INTO RESADV (DESCID, ID_LANG, LANG, [TYPE]) SELECT 101018,ML.LANG_ID, [LANG] ,2 FROM EUDORES..[RESAPP] RA
INNER JOIN MAPLANG ML ON ML.LANG_SYSID = RA.Id_Lang 
where resid=2680 AND isnull(ML.disabled,0) = 0
