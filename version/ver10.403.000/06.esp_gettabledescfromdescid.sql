 
/*
*     Retourne la description de la rubrique
*     SPH & HLA - 10/04/09
--[esp_getTableDescFromDescId] 200,4
*/

alter PROCEDURE [dbo].[esp_getTableDescFromDescId]
      @Descid numeric,      -- DescId
      @UserId numeric       -- userid
AS 

declare @ulevel int
declare @groupid int 
declare @sLang varchar(20)

declare @sLib nvarchar(100)

declare @sLibPj nvarchar(100)
declare @sLibDesc nvarchar(100)
declare @sLibInfo nvarchar(100)
declare @sLibNote nvarchar(100)


select @ulevel = userlevel, @groupid = groupid, @sLang = upper(lang) from [user] where userid = @UserId



select @sLib = RES from dbo.efc_getResid(@sLang ,@Descid)
select @sLibPj = RES from dbo.efc_getResid(@sLang ,@Descid + 91)
select @sLibDesc = RES from dbo.efc_getResid(@sLang ,@Descid + 89)
select @sLibInfo = RES from dbo.efc_getResid(@sLang ,@Descid + 93)
select @sLibNote = RES from dbo.efc_getResid(@sLang ,@Descid + 94)
 
 
 

select @sLib as libelle, [desc].[ToolTipText], [desc].[ObligatReadOnly], [desc].[ProspectEnabled],
      @sLibPj as pjLib, [pj_desc].[icon] as pjIcon,
      @sLibDesc as descLib, [DESCRIPTION_DESC].[icon] as descIcon,
      @sLibInfo as infoLib, [info_desc].[icon] as infoIcon,
      @sLibNote as noteLib, [note_desc].[icon] as noteIcon,
      [desc].[TreatmentMaxRows], [desc].[rowspan], [desc].[colspan], [desc].[Disabled], 
      /* Droits de visu */
     ISNULL( P_VIEW.P,1)  VIEW_P,
      /* Droits d ajout */
     ISNULL(P_ADD.P,1) ADD_P,
      /* Droits d ajout mode liste */
	  ISNULL(P_ADD_LIST.P,1)  ADD_LIST_P,
      /* Droits de modif */
      ISNULL(P_MODIF.P,1) MODIF_P,
      /* Droits de suppression */
	  ISNULL(P_DEL.P,1)   DEL_P,
      /* Droits sur l historique */
	  ISNULL(P_HISTO.P,1)  HISTO_P,
      /* Droits sur duplication */
	  ISNULL(P_DUPLI.P,1)  DUPLI_P,
      /* Droits de visu de la pj */
	  ISNULL(PJ_VIEW.P,1)  VIEW_PJ,      
      /* Traitements droits sur PJ */      
	  ISNULL(P_ADD_PJ.P,1)  ADD_PJ,      
      /* Traitements droits de modif sur PJ */
	  ISNULL(P_MODIF_PJ.P,1)  MODIF_PJ,      
      /* Traitements droits de Suppression sur PJ */
	  ISNULL(P_DEL_PJ.P,1) DEL_PJ,            
      /* Traitements droits d ajout */
	  ISNULL(P_ADD_MULTI.P,1) ADD_MULTI_P,      
      /* Traitements droits de modif */
	  ISNULL(P_MODIF_MULTI.P,1) MODIF_MULTI_P,
      /* Traitements droits de suppression */
	  ISNULL(P_DEL_MULTI.P,1) DEL_MULTI_P,
      /* Traitements droits sur duplication */
	  ISNULL(P_DUPLI_MULTI.P,1)  DUPLI_MULTI_P,      
      /* Traitements d ajout suppression depuis un filtre */
	  ISNULL(P_MULTI_FROMFILTER.P,1)  MULTI_FROMFILTER_P,
      
	  ISNULL(P_LEFT_MENU.P,1) ACTION_MENU_P,
      ISNULL(P_GLOBAL_LINK.P,1) GLOBAL_LINK_P,	  	
      
	  -- Traitements d'import en masse onglet
	  ISNULL(P_IMPORT_TAB.P,1)  IMPORT_TAB_P,

	  -- Traitements d'import en masse depuis un signet
	  ISNULL(P_IMPORT_BKM.P,1) IMPORT_BKM_P,

      [DESC].[InterPpNeeded], [DESC].[InterPmNeeded], [DESC].[InterEventNeeded], [DESC].[InterEventHidden],
      [DESC].[NoDefaultLink_100], [DESC].[NoDefaultLink_200], [DESC].[NoDefaultLink_300],
      [DESC].[NoCascadePMPP], [DESC].[NoCascadePPPM],
	  [DESC].[AutoCompletion],
	   ISNULL(COMPLETENAMEFORMAT,'0') AS CompleteNameFormat
	  
from [desc] inner join [res] 
            on [desc].[descid] = [res].[resid] 
            
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid]

            LEFT JOIN [TRAIT] AS T_ADD ON [desc].[descid] + 1 = T_ADD.[traitid]            
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_ADD ON T_ADD.[permid] = P_ADD.[permissionid]

            LEFT JOIN [TRAIT] AS T_ADD_LIST ON [desc].[descid] + 15 = T_ADD_LIST.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_ADD_LIST ON T_ADD_LIST.[permid] = P_ADD_LIST.[permissionid]

            LEFT JOIN [TRAIT] AS T_MODIF ON [desc].[descid] + 2 = T_MODIF.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_MODIF ON T_MODIF.[permid] = P_MODIF.[permissionid]

            LEFT JOIN [TRAIT] AS T_DEL ON [desc].[descid] + 3 = T_DEL.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid) AS P_DEL ON T_DEL.[permid] = P_DEL.[permissionid]

            LEFT JOIN [TRAIT] AS T_HISTO ON [desc].[descid] + 4 = T_HISTO.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid) AS P_HISTO ON T_HISTO.[permid] = P_HISTO.[permissionid]

            LEFT JOIN [TRAIT] AS T_DUPLI ON [desc].[descid] + 5 = T_DUPLI.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_DUPLI ON T_DUPLI.[permid] = P_DUPLI.[permissionid]

            /* ICON PJ */
            LEFT JOIN [desc] AS PJ_DESC ON [desc].[file] = [pj_desc].[file] and [pj_desc].[descid] = (@Descid + 91)
            /* DROIT TREATEMENT PJ */
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid) AS PJ_VIEW ON [pj_desc].[viewpermid] = [pj_view].[permissionid]

            /* ICON Description */
            LEFT JOIN [desc] AS DESCRIPTION_DESC ON [desc].[file] = [DESCRIPTION_DESC].[file] and [DESCRIPTION_DESC].[descid] = (@Descid + 89)
            /* ICON INFO */
            LEFT JOIN [desc] AS INFO_DESC ON [desc].[file] = [info_desc].[file] and [info_desc].[descid] = (@Descid + 93)
            /* ICON NOTE */
            LEFT JOIN [desc] AS NOTE_DESC ON [desc].[file] = [note_desc].[file] and [note_desc].[descid] = (@Descid + 94)
            
			/* TRAITEMENTS */
            LEFT JOIN [TRAIT] AS T_ADD_MULTI ON [desc].[descid] + 6 = T_ADD_MULTI.[traitid]             
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid) AS P_ADD_MULTI ON T_ADD_MULTI.[permid] = P_ADD_MULTI.[permissionid]

			
			LEFT JOIN [TRAIT] AS T_MODIF_MULTI ON [desc].[descid] + 7 = T_MODIF_MULTI.[traitid]
			LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid) AS P_MODIF_MULTI ON T_MODIF_MULTI.[permid] = P_MODIF_MULTI.[permissionid]

            LEFT JOIN [TRAIT] AS T_DEL_MULTI ON [desc].[descid] + 8 = T_DEL_MULTI.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_DEL_MULTI ON T_DEL_MULTI.[permid] = P_DEL_MULTI.[permissionid]


            LEFT JOIN [TRAIT] AS T_DUPLI_MULTI ON [desc].[descid] + 9 = T_DUPLI_MULTI.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_DUPLI_MULTI ON T_DUPLI_MULTI.[permid] = P_DUPLI_MULTI.[permissionid]
            
            LEFT JOIN [TRAIT] AS T_ADD_PJ ON [desc].[descid] + 11 = T_ADD_PJ.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_ADD_PJ ON T_ADD_PJ.[permid] = P_ADD_PJ.[permissionid]

            LEFT JOIN [TRAIT] AS T_MODIF_PJ ON [desc].[descid] + 12 = T_MODIF_PJ.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_MODIF_PJ ON T_MODIF_PJ.[permid] = P_MODIF_PJ.[permissionid]

            LEFT JOIN [TRAIT] AS T_DEL_PJ ON [desc].[descid] + 13 = T_DEL_PJ.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_DEL_PJ ON T_DEL_PJ.[permid] = P_DEL_PJ.[permissionid]            

            LEFT JOIN [TRAIT] AS T_MULTI_FROMFILTER ON [desc].[descid] + 17 = T_MULTI_FROMFILTER.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_MULTI_FROMFILTER ON T_MULTI_FROMFILTER.[permid] = P_MULTI_FROMFILTER.[permissionid] 
			
			-- Import en masse sur un onglet
			LEFT JOIN [TRAIT] AS T_IMPORT_TAB ON [desc].[descid] + 18 = T_IMPORT_TAB.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_IMPORT_TAB ON T_IMPORT_TAB.[permid] = P_IMPORT_TAB.[permissionid]        
			
			-- Import en masse depuis un signet
			LEFT JOIN [TRAIT] AS T_IMPORT_BKM ON [desc].[descid] + 19 = T_IMPORT_BKM.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_IMPORT_BKM ON T_IMPORT_BKM.[permid] = P_IMPORT_BKM.[permissionid]               
            

            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_LEFT_MENU on P_LEFT_MENU.[PermissionId]   = [DESC].[LeftMenuLinksPermId]
            
                  
            LEFT JOIN [TRAIT] AS T_GLOBAL_LINK ON [desc].[descid] + 16 = T_GLOBAL_LINK.[traitid]
            LEFT JOIN dbo.cfc_getPermInfo(@UserId, @ulevel, @groupid)  AS P_GLOBAL_LINK ON T_GLOBAL_LINK.[permid] = P_GLOBAL_LINK.[permissionid]      
			
			 
            LEFT JOIN (
				SELECT [DESCADVPIVOT].[DESCID], [9] AS 'COMPLETENAMEFORMAT'
				FROM [DESC]
				LEFT JOIN (
					SELECT [DESCID],[PARAMETER],[VALUE]
					FROM [DESCADV]
					WHERE [DESCID] = @descid
					) R
				PIVOT(max(value) FOR parameter IN ([9])) AS [DESCADVPIVOT] ON [DESCADVPIVOT].DescId = [desc].DescId
				WHERE [DESC].[DESCID] = @descid
			) AS DESCP ON DESCP.DESCID = [DESC].[DESCID]
		

where [desc].[descid] = @Descid order by libelle
