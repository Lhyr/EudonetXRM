DELETE FROM [RESADV] WHERE [TYPE] = 2 AND [DESCID] LIKE '1010__'

 INSERT INTO RESADV ( [DESCID], [ID_LANG], [TYPE], [LANG])
 SELECT 101075, 0, 2, 'Avatar utilisé pour représenter l’utilisateur dans Eudonet. L''image doit faire xx pixel de hauteur et yy pixel de largeur'
UNION SELECT 101001, 0, 2, 'Nom de l’utilisateur permettant la connexion à Eudonet et affiché dans les listes'
UNION SELECT 101002, 0, 2, 'Nom complet de l''utilisateur utilisé par les fonctions de fusion publipostage d''Eudonet'
UNION SELECT 101011, 0, 2, 'Cocher pour désactiver cet utilisateur et lui interdire toute connexion à Eudonet'
UNION SELECT 101012, 0, 2, 'Cocher pour ne pas afficher cet utilisateur dans les listes '
UNION SELECT 101016, 0, 2, 'Groupe de rattachement hiérarchique de l''utilisateur'
UNION SELECT 101017, 0, 2, 'Niveau d''habilitation de l’utilisateur'
UNION SELECT 101027, 0, 2, 'Groupe de rattachement hiérarchique de l''utilisateur'


UPDATE [RES] SET LANG_00 = 'Groupe' WHERE [RESID] = 101027