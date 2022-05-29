-- SPH - US #4315 - Tâche #7071/7100 - Création de droits de traitement et restrictions sur la personnalisation de la zone Assistant du nouveau mode Fiche Eudonet X
-- Validé et intégré par MAB

-- ================================================
-- ETAPE 1 : DECLARATIONS : IDENTIFIANTS DES DROITS
-- ================================================

-- Les ID de droits de traitement sont par tranches de centaines/dizaines, avec les centaines correspondant aux DescID des tables 
-- Soit 1xx : droits pour EVENT, 2xx : droits pour PP, 3xx : droits pour PM, etc.
-- Les tranches ne correspondant pas à des DescID système (ex : 8xx) sont pour les droits globaux
-- On souhaite ici un droit par onglet, mais non rattaché à une rubrique. Donc pas 8xx, mais dans la continuité des droits existants
-- Généralement, xx1 = Ajout, xx2 = Modification, xx3 = Suppression.
-- Après confirmation de SPH, la série 120 et 130 était réservée auparavant (avant Eudonet XRM) à une série de droits v5/v7 qui ne sont désormais plus utilisés.
-- On choisit donc 122, qui deviendra *22 pour chaque TabID

DECLARE @PERMTYPE INT
SET @PERMTYPE = 22

DECLARE  @T TABLE (PERM INT, TRAIT INT)

-- Droit de mise à jour de la zone Assistant d'Eudonet X - UPDATE_EUDONETX_PROGRESS_AREA dans eLibConst = 122

-- Voir également le droit PRC_RIGHT_UPDATE_EUDONETX_PROGRESS_AREA dans eProgressRights sur EudoQuery = 22

-- =================================================================================================
-- ETAPE 2 : INSERTION DES LIBELLÉS DES DROITS (RESSOURCES DE LANGUE) DANS LA BASE COMMUNE (EUDORES)
-- =================================================================================================

-- Cette MAJ se fait désormais via EudoProcess 1 seule fois par serveur, via son bouton de MAJ de structure qui
-- fait la MAJ à partir d'un fichier ResTrait.json interne

-- ==========================================================================================================
-- ETAPE 3 : INSERTION DANS LA BASE CLIENT (EUDO_XXXX) AVEC LES PERMISSIONS CRÉÉES OU RÉCUPÉRÉES PRÉCÉDEMMENT
-- ==========================================================================================================

INSERT INTO trait(TraitId, UserLevel,TraitLevel)
SELECT 
	[DESC].DescId + @PERMTYPE,0, 1
FROM [DESC] WHERE [DESCID] % 100 = 0 
AND NOT EXISTS (
	SELECT 1 FROM TRAIT  t
	
	WHERE TraitId = [descid] + @PERMTYPE
)

-- ======================================================================================================================================
-- ETAPE 4 : RÉCUPÉRATION OU CRÉATION DES PERMISSIONS POUR RESTREINDRE CES DROITS AUX SUPER-ADMINISTRATEURS UNIQUEMENT (CHOIX PAR DÉFAUT)
-- ======================================================================================================================================

-- Création de la nouvelle permission
MERGE INTO PERMISSION P
USING 	 (
	SELECT T.TraitId FROM TRAIT T
	LEFT JOIN [PERMISSION] P ON T.PermId = P.PermissionId
	WHERE TraitId % 100 = @PERMTYPE AND TraitId NOT IN (@PERMTYPE,500+@PERMTYPE,800+@PERMTYPE)
	AND P.PermissionId IS NULL
) T	
ON
	TraitId % 100 = @PERMTYPE AND TraitId NOT IN (@PERMTYPE,500+@PERMTYPE,800+@PERMTYPE)
		AND P.PermissionId IS NULL

WHEN NOT MATCHED THEN

 INSERT([MODE],[level],[user]) Values(1,100,0)
 OUTPUT inserted.permissionid, T.traitid INTO @T(PERM,TRAIT);

-- MAJ De trait avec la permission en question
UPDATE [TRAIT] SET PermId = z.PERM 
FROM 
[TRAIT] 
INNER JOIN @T z on z.TRAIT = TRAIT.TraitId
 
-- MAJ des Permission IDs existants
UPDATE [PERMISSION] 
	SET
		[User] = 0,
		[Mode] = 0,
		[Level] = 100
WHERE PermissionId IN 
(
	SELECT permid FROM TRAIT 
	WHERE  TraitId % 100 = @PERMTYPE AND TraitId NOT IN (@PERMTYPE,500+@PERMTYPE,800+@PERMTYPE)
)
