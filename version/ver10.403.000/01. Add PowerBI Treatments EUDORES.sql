-- MAB - #63 663 - Création de droits de traitement et restrictions sur les rapports d'export au format Power BI

-- ================================================
-- ETAPE 1 : DECLARATIONS : IDENTIFIANTS DES DROITS
-- ================================================

-- Les ID de droits de traitement sont par tranches de centaines/dizaines, avec les centaines correspondant aux DescID des tables 
-- Soit 1xx : droits pour EVENT, 2xx : droits pour PP, 3xx : droits pour PM, etc.
-- Les tranches ne correspondant pas à des DescID système (ex : 8xx) sont donc utilisables pour de nouveaux droits globaux

DECLARE @PowerBITraitId INT = 820 -- Droits d'export en format Power BI - EXPORT_POWERBI = 820,
DECLARE @PowerBIAddTraitId INT = 821 -- Droit d'ajout d'un rapport Power BI - ADD_EXPORT_POWERBI = 821,
DECLARE @PowerBIUpdateTraitId INT = 822 -- Droit de mise à jour d'un rapport Power BI - UPDATE_EXPORT_POWERBI = 822,
DECLARE @PowerBIDeleteTraitId INT = 823 -- Droit de suppression d'un rapport Power BI - DEL_EXPORT_POWERBI = 823,

-- Pour remettre à zéro si besoin :
-- delete from [permission] where permissionid in (select permid from [trait] where traitid > @PowerBITraitId - 1 and traitid < @PowerBIDeleteTraitId + 1)
-- delete from [trait] where traitid > @PowerBITraitId - 1 and traitid < @PowerBIDeleteTraitId + 1
-- delete from EUDORES..TRAIT where traitid > @PowerBITraitId - 1 and traitid < @PowerBIDeleteTraitId + 1

-- =================================================================================================
-- ETAPE 2 : INSERTION DES LIBELLÉS DES DROITS (RESSOURCES DE LANGUE) DANS LA BASE COMMUNE (EUDORES)
-- =================================================================================================

if not exists (select * from EUDORES..TRAIT where traitid = @PowerBITraitId)
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (@PowerBITraitId, 'Export en format Power BI', 'Export in Power BI format', 'Export in Power BI format')
end
    
if not exists (select * from EUDORES..TRAIT where traitid = @PowerBIAddTraitId)
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (@PowerBIAddTraitId, 'Ajouter un rapport Power BI', 'Add Power BI report', 'Add Power BI report')
end

if not exists (select * from EUDORES..TRAIT where traitid = @PowerBIUpdateTraitId)
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (@PowerBIUpdateTraitId, 'Modifier un rapport Power BI', 'Update Power BI report', 'Update Power BI report')
end

if not exists (select * from EUDORES..TRAIT where traitid = @PowerBIDeleteTraitId)
begin
	insert into EUDORES..TRAIT (traitid,[LANG_00],[LANG_01],[LANG_02]) 
	values (@PowerBIDeleteTraitId, 'Supprimer un rapport Power BI', 'Delete Power BI report', 'Delete Power BI report')
end

-- ================================================================================================================================
-- ETAPE 3 : RÉCUPÉRATION OU CRÉATION DES PERMISSIONS POUR RESTREINDRE CES DROITS AUX ADMINISTRATEURS UNIQUEMENT (CHOIX PAR DÉFAUT)
-- ================================================================================================================================

DECLARE @PowerBIPermId INT = 0
DECLARE @PowerBIAddPermId INT = 0
DECLARE @PowerBIUpdatePermId INT = 0
DECLARE @PowerBIDeletePermId INT = 0

SELECT @PowerBIPermId = ISNULL([PermId], 0) FROM [TRAIT] WHERE TraitId = @PowerBITraitId 
IF (@PowerBIPermId = 0)
BEGIN
 -- 0 : Niveau seulement, 1 : Utilisateur seulement, 2 : Utilisateur ou Niveau, 3 : Utilisateur et Niveau 
 -- Choix effectué ici : administrateurs uniquement
 INSERT INTO [Permission] ([Mode], [Level], [User]) values (0, 99, NULL)
 SELECT @PowerBIPermId = SCOPE_IDENTITY()
END

SELECT @PowerBIAddPermId = ISNULL([PermId], 0) FROM [TRAIT] WHERE TraitId = @PowerBITraitId 
IF (@PowerBIAddPermId = 0)
BEGIN
 -- 0 : Niveau seulement, 1 : Utilisateur seulement, 2 : Utilisateur ou Niveau, 3 : Utilisateur et Niveau 
 -- Choix effectué ici : administrateurs uniquement
 INSERT INTO [Permission] ([Mode], [Level], [User]) values (0, 99, NULL)
 SELECT @PowerBIAddPermId = SCOPE_IDENTITY()
END

SELECT @PowerBIUpdatePermId = ISNULL([PermId], 0) FROM [TRAIT] WHERE TraitId = @PowerBITraitId 
IF (@PowerBIUpdatePermId = 0)
BEGIN
 -- 0 : Niveau seulement, 1 : Utilisateur seulement, 2 : Utilisateur ou Niveau, 3 : Utilisateur et Niveau 
 -- Choix effectué ici : administrateurs uniquement
 INSERT INTO [Permission] ([Mode], [Level], [User]) values (0, 99, NULL)
 SELECT @PowerBIUpdatePermId = SCOPE_IDENTITY()
END

SELECT @PowerBIDeletePermId = ISNULL([PermId], 0) FROM [TRAIT] WHERE TraitId = @PowerBITraitId 
IF (@PowerBIDeletePermId = 0)
BEGIN
 -- 0 : Niveau seulement, 1 : Utilisateur seulement, 2 : Utilisateur ou Niveau, 3 : Utilisateur et Niveau 
 -- Choix effectué ici : administrateurs uniquement
 INSERT INTO [Permission] ([Mode], [Level], [User]) values (0, 99, NULL)
 SELECT @PowerBIDeletePermId = SCOPE_IDENTITY()
END

-- ==========================================================================================================
-- ETAPE 4 : INSERTION DANS LA BASE CLIENT (EUDO_XXXX) AVEC LES PERMISSIONS CRÉÉES OU RÉCUPÉRÉES PRÉCÉDEMMENT
-- ==========================================================================================================
-- 
insert into [trait] ([TraitId], [PermId], [TraitLevel], [sort]) select top 1 @PowerBITraitId, @PowerBIPermId, 0, NULL from [trait] where not exists (select 1 from trait where traitid = @PowerBITraitId)
insert into [trait] ([TraitId], [PermId], [TraitLevel], [sort]) select top 1 @PowerBIAddTraitId, @PowerBIAddPermId, 0, NULL from [trait] where not exists (select 1 from trait where traitid = @PowerBIAddTraitId)
insert into [trait] ([TraitId], [PermId], [TraitLevel], [sort]) select top 1 @PowerBIUpdateTraitId, @PowerBIUpdatePermId, 0, NULL from [trait] where not exists (select 1 from trait where traitid = @PowerBIUpdateTraitId)
insert into [trait] ([TraitId], [PermId], [TraitLevel], [sort]) select top 1 @PowerBIDeleteTraitId, @PowerBIDeletePermId, 0, NULL from [trait] where not exists (select 1 from trait where traitid = @PowerBIDeleteTraitId)