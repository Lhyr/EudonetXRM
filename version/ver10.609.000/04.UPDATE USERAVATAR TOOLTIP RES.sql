UPDATE RESADV
SET LANG = 'Avatar utilisé pour représenter l’utilisateur dans Eudonet. L''image doit faire 40 pixels de haut et 40 pixels de large et avoir un ratio 1:1.'
WHERE DESCID = 101075 AND ID_LANG = 0

IF EXISTS (SELECT 1 from RESADV WHERE DESCID = 101075 AND ID_LANG = 1)
BEGIN
UPDATE RESADV set LANG = 'Avatar used to represent the user in Eudonet. The image must be at least 40 pixels high and 40 pixels wide and have an aspect ratio of 1:1.' where DESCID = 101075 and ID_LANG = 1
END
ELSE
INSERT INTO RESADV (DESCID, ID_LANG, LANG, [TYPE]) VALUES (101075, 1, 'Avatar used to represent the user in Eudonet. The image must be at least 40 pixels high and 40 pixels wide and have an aspect ratio of 1:1.', 2)