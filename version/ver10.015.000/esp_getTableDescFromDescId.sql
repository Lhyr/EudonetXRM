
/*
*     Retourne la description de la rubrique
*     SPH & HLA - 10/04/09
*/

ALTER PROCEDURE [dbo].[esp_getTableDescFromDescId]
      @Descid numeric,        -- DescId
      @UserId numeric              -- userid
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

-- Libelle de la table
select @sLib = case
      when @sLang='LANG_00' then lang_00
      when @sLang='LANG_01' then lang_01
      when @sLang='LANG_02' then lang_02
      when @sLang='LANG_03' then lang_03
      when @sLang='LANG_04' then lang_04
      when @sLang='LANG_05' then lang_05
      when @sLang='LANG_06' then lang_06
      when @sLang='LANG_07' then lang_07
      when @sLang='LANG_08' then lang_08
      when @sLang='LANG_09' then lang_09
      else lang_00 end
from [res] where [res].[resid] = @Descid
-- Libelle Pj
select @sLibPj = case
      when @sLang='LANG_00' then lang_00
      when @sLang='LANG_01' then lang_01
      when @sLang='LANG_02' then lang_02
      when @sLang='LANG_03' then lang_03
      when @sLang='LANG_04' then lang_04
      when @sLang='LANG_05' then lang_05
      when @sLang='LANG_06' then lang_06
      when @sLang='LANG_07' then lang_07
      when @sLang='LANG_08' then lang_08
      when @sLang='LANG_09' then lang_09
      else lang_00 end
from [res] where [res].[resid] = (@Descid + 91)
-- Libelle du champ description
select @sLibDesc = case
      when @sLang='LANG_00' then lang_00
      when @sLang='LANG_01' then lang_01
      when @sLang='LANG_02' then lang_02
      when @sLang='LANG_03' then lang_03
      when @sLang='LANG_04' then lang_04
      when @sLang='LANG_05' then lang_05
      when @sLang='LANG_06' then lang_06
      when @sLang='LANG_07' then lang_07
      when @sLang='LANG_08' then lang_08
      when @sLang='LANG_09' then lang_09
      else lang_00 end
from [res] where [res].[resid] = (@Descid + 89)
-- Libelle du champ infos
select @sLibInfo = case
      when @sLang='LANG_00' then lang_00
      when @sLang='LANG_01' then lang_01
      when @sLang='LANG_02' then lang_02
      when @sLang='LANG_03' then lang_03
      when @sLang='LANG_04' then lang_04
      when @sLang='LANG_05' then lang_05
      when @sLang='LANG_06' then lang_06
      when @sLang='LANG_07' then lang_07
      when @sLang='LANG_08' then lang_08
      when @sLang='LANG_09' then lang_09
      else lang_00 end
from [res] where [res].[resid] = (@Descid + 93)
-- Libelle du champ notes
select @sLibNote = case
      when @sLang='LANG_00' then lang_00
      when @sLang='LANG_01' then lang_01
      when @sLang='LANG_02' then lang_02
      when @sLang='LANG_03' then lang_03
      when @sLang='LANG_04' then lang_04
      when @sLang='LANG_05' then lang_05
      when @sLang='LANG_06' then lang_06
      when @sLang='LANG_07' then lang_07
      when @sLang='LANG_08' then lang_08
      when @sLang='LANG_09' then lang_09
      else lang_00 end
from [res] where [res].[resid] = (@Descid + 94)

select @sLib as libelle, [desc].[ToolTipText], [desc].[ObligatReadOnly], [desc].[ProspectEnabled],
      @sLibPj as pjLib, [pj_desc].[icon] as pjIcon,
      @sLibDesc as descLib, [DESCRIPTION_DESC].[icon] as descIcon,
      @sLibInfo as infoLib, [info_desc].[icon] as infoIcon,
      @sLibNote as noteLib, [note_desc].[icon] as noteIcon,
      [desc].[TreatmentMaxRows], [desc].[rowspan], [desc].[colspan], [desc].[Disabled], 
      /* Droits de visu */
      case 
                  when P_VIEW.mode = 0 then case when @ulevel >= P_VIEW.level then 1 else 0 end
                  when P_VIEW.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 then 1 else 0 end
                  when P_VIEW.mode = 3 then case when (@ulevel >= P_VIEW.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
                  when P_VIEW.mode = 2 then case when (@ulevel >= P_VIEW.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_VIEW.[user] +';') > 0) then 1 else 0 end
                  else 1
      end VIEW_P,
      /* Droits d ajout */
      case 
                  when P_ADD.mode = 0 then case when @ulevel >= P_ADD.level then 1 else 0 end
                  when P_ADD.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0 then 1 else 0 end
                  when P_ADD.mode = 3 then case when (@ulevel >= P_ADD.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0) then 1 else 0 end
                  when P_ADD.mode = 2 then case when (@ulevel >= P_ADD.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD.[user] +';') > 0) then 1 else 0 end
                  else 1
      end ADD_P,
      /* Droits d ajout mode liste */
      case 
                  when P_ADD_LIST.mode = 0 then case when @ulevel >= P_ADD_LIST.level then 1 else 0 end
                  when P_ADD_LIST.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0 then 1 else 0 end
                  when P_ADD_LIST.mode = 3 then case when (@ulevel >= P_ADD_LIST.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0) then 1 else 0 end
                  when P_ADD_LIST.mode = 2 then case when (@ulevel >= P_ADD_LIST.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_LIST.[user] +';') > 0) then 1 else 0 end
                  else 1
      end ADD_LIST_P,
      /* Droits de modif */
      case 
                  when P_MODIF.mode = 0 then case when @ulevel >= P_MODIF.level then 1 else 0 end
                  when P_MODIF.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0 then 1 else 0 end
                  when P_MODIF.mode = 3 then case when (@ulevel >= P_MODIF.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0) then 1 else 0 end
                  when P_MODIF.mode = 2 then case when (@ulevel >= P_MODIF.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF.[user] +';') > 0) then 1 else 0 end
                  else 1
      end MODIF_P,
      /* Droits de suppression */
      case 
                  when P_DEL.mode = 0 then case when @ulevel >= P_DEL.level then 1 else 0 end
                  when P_DEL.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0 then 1 else 0 end
                  when P_DEL.mode = 3 then case when (@ulevel >= P_DEL.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0) then 1 else 0 end
                  when P_DEL.mode = 2 then case when (@ulevel >= P_DEL.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL.[user] +';') > 0) then 1 else 0 end
                  else 1
      end DEL_P,
      /* Droits sur l historique */
      case 
                  when P_HISTO.mode = 0 then case when @ulevel >= P_HISTO.level then 1 else 0 end
                  when P_HISTO.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0 then 1 else 0 end
                  when P_HISTO.mode = 3 then case when (@ulevel >= P_HISTO.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0) then 1 else 0 end
                  when P_HISTO.mode = 2 then case when (@ulevel >= P_HISTO.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_HISTO.[user] +';') > 0) then 1 else 0 end
                  else 1
      end HISTO_P,
      /* Droits sur duplication */
      case 
                  when P_DUPLI.mode = 0 then case when @ulevel >= P_DUPLI.level then 1 else 0 end
                  when P_DUPLI.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0 then 1 else 0 end
                  when P_DUPLI.mode = 3 then case when (@ulevel >= P_DUPLI.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0) then 1 else 0 end
                  when P_DUPLI.mode = 2 then case when (@ulevel >= P_DUPLI.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI.[user] +';') > 0) then 1 else 0 end
                  else 1
      end DUPLI_P,
      /* Droits de visu de la pj */
      case 
                  when PJ_VIEW.mode = 0 then case when @ulevel >= PJ_VIEW.level then 1 else 0 end
                  when PJ_VIEW.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0 then 1 else 0 end
                  when PJ_VIEW.mode = 3 then case when (@ulevel >= PJ_VIEW.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0) then 1 else 0 end
                  when PJ_VIEW.mode = 2 then case when (@ulevel >= PJ_VIEW.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ PJ_VIEW.[user] +';') > 0) then 1 else 0 end
                  else 1
      end VIEW_PJ,
      
      /* Traitements droits sur PJ */
      case 
                  when P_ADD_PJ.mode = 0 then case when @ulevel >= P_ADD_PJ.level then 1 else 0 end
                  when P_ADD_PJ.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0 then 1 else 0 end
                  when P_ADD_PJ.mode = 3 then case when (@ulevel >= P_ADD_PJ.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0) then 1 else 0 end
                  when P_ADD_PJ.mode = 2 then case when (@ulevel >= P_ADD_PJ.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_PJ.[user] +';') > 0) then 1 else 0 end
                  else 1
      end ADD_PJ,
      
      /* Traitements droits de modif sur PJ */
      case 
                  when P_MODIF_PJ.mode = 0 then case when @ulevel >= P_MODIF_PJ.level then 1 else 0 end
                  when P_MODIF_PJ.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0 then 1 else 0 end
                  when P_MODIF_PJ.mode = 3 then case when (@ulevel >= P_MODIF_PJ.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0) then 1 else 0 end
                  when P_MODIF_PJ.mode = 2 then case when (@ulevel >= P_MODIF_PJ.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_PJ.[user] +';') > 0) then 1 else 0 end
                  else 1
      end MODIF_PJ,
      
      /* Traitements droits de Suppression sur PJ */
      case 
                  when P_DEL_PJ.mode = 0 then case when @ulevel >= P_DEL_PJ.level then 1 else 0 end
                  when P_DEL_PJ.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0 then 1 else 0 end
                  when P_DEL_PJ.mode = 3 then case when (@ulevel >= P_DEL_PJ.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0) then 1 else 0 end
                  when P_DEL_PJ.mode = 2 then case when (@ulevel >= P_DEL_PJ.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL_PJ.[user] +';') > 0) then 1 else 0 end
                  else 1
      end DEL_PJ,      
      
      /* Traitements droits d ajout */
      case 
                  when P_ADD_MULTI.mode = 0 then case when @ulevel >= P_ADD_MULTI.level then 1 else 0 end
                  when P_ADD_MULTI.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0 then 1 else 0 end
                  when P_ADD_MULTI.mode = 3 then case when (@ulevel >= P_ADD_MULTI.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0) then 1 else 0 end
                  when P_ADD_MULTI.mode = 2 then case when (@ulevel >= P_ADD_MULTI.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_ADD_MULTI.[user] +';') > 0) then 1 else 0 end
                  else 1
      end ADD_MULTI_P,
      
      /* Traitements droits de modif */
      case 
                  when P_MODIF_MULTI.mode = 0 then case when @ulevel >= P_MODIF_MULTI.level then 1 else 0 end
                  when P_MODIF_MULTI.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0 then 1 else 0 end
                  when P_MODIF_MULTI.mode = 3 then case when (@ulevel >= P_MODIF_MULTI.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0) then 1 else 0 end
                  when P_MODIF_MULTI.mode = 2 then case when (@ulevel >= P_MODIF_MULTI.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MODIF_MULTI.[user] +';') > 0) then 1 else 0 end
                  else 1
      end MODIF_MULTI_P,
      /* Traitements droits de suppression */
      case 
                  when P_DEL_MULTI.mode = 0 then case when @ulevel >= P_DEL_MULTI.level then 1 else 0 end
                  when P_DEL_MULTI.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0 then 1 else 0 end
                  when P_DEL_MULTI.mode = 3 then case when (@ulevel >= P_DEL_MULTI.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0) then 1 else 0 end
                  when P_DEL_MULTI.mode = 2 then case when (@ulevel >= P_DEL_MULTI.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DEL_MULTI.[user] +';') > 0) then 1 else 0 end
                  else 1
      end DEL_MULTI_P,
      /* Traitements droits sur duplication */
      case 
                  when P_DUPLI_MULTI.mode = 0 then case when @ulevel >= P_DUPLI_MULTI.level then 1 else 0 end
                  when P_DUPLI_MULTI.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0 then 1 else 0 end
                  when P_DUPLI_MULTI.mode = 3 then case when (@ulevel >= P_DUPLI_MULTI.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0) then 1 else 0 end
                  when P_DUPLI_MULTI.mode = 2 then case when (@ulevel >= P_DUPLI_MULTI.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_DUPLI_MULTI.[user] +';') > 0) then 1 else 0 end
                  else 1
      end DUPLI_MULTI_P,
      
      /* Traitements d ajout suppression depuis un filtre */
      case 
                  when P_MULTI_FROMFILTER.mode = 0 then case when @ulevel >= P_MULTI_FROMFILTER.level then 1 else 0 end
                  when P_MULTI_FROMFILTER.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0 then 1 else 0 end
                  when P_MULTI_FROMFILTER.mode = 3 then case when (@ulevel >= P_MULTI_FROMFILTER.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0) then 1 else 0 end
                  when P_MULTI_FROMFILTER.mode = 2 then case when (@ulevel >= P_MULTI_FROMFILTER.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_MULTI_FROMFILTER.[user] +';') > 0) then 1 else 0 end
                  else 1
      end MULTI_FROMFILTER_P,
      
      case 
                  when P_LEFT_MENU.mode = 0 then case when @ulevel >= P_LEFT_MENU.level then 1 else 0 end
                  when P_LEFT_MENU.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0 then 1 else 0 end
                  when P_LEFT_MENU.mode = 3 then case when (@ulevel >= P_LEFT_MENU.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0) then 1 else 0 end
                  when P_LEFT_MENU.mode = 2 then case when (@ulevel >= P_LEFT_MENU.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_LEFT_MENU.[user] +';') > 0) then 1 else 0 end
                  else 1
      end ACTION_MENU_P ,
      
      case 
                  when P_GLOBAL_LINK.mode = 0 then case when @ulevel >= P_GLOBAL_LINK.level then 1 else 0 end
                  when P_GLOBAL_LINK.mode = 1 then case when charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0 or charindex(';G'+cast( @groupid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0 then 1 else 0 end
                  when P_GLOBAL_LINK.mode = 3 then case when (@ulevel >= P_GLOBAL_LINK.level) And ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0) then 1 else 0 end
                  when P_GLOBAL_LINK.mode = 2 then case when (@ulevel >= P_GLOBAL_LINK.level) or ( charindex(';'+ cast(@userid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0 or charindex(';G'+ cast( @groupid as varchar(50)) + ';',';'+ P_GLOBAL_LINK.[user] +';') > 0) then 1 else 0 end
                  else 1
      end GLOBAL_LINK_P,
      [DESC].[InterPpNeeded], [DESC].[InterPmNeeded], [DESC].[InterEventNeeded], [DESC].[InterEventHidden],
      [DESC].[NoDefaultLink_100], [DESC].[NoDefaultLink_200], [DESC].[NoDefaultLink_300]
      
from [desc] inner join [res] 
            on [desc].[descid] = [res].[resid] 
            LEFT JOIN [PERMISSION] AS P_VIEW  ON [desc].[viewpermid] = P_VIEW.[permissionid] 
            LEFT JOIN [TRAIT] AS T_ADD ON [desc].[descid] + 1 = T_ADD.[traitid]
            LEFT JOIN [PERMISSION] AS P_ADD ON T_ADD.[permid] = P_ADD.[permissionid]
            LEFT JOIN [TRAIT] AS T_ADD_LIST ON [desc].[descid] + 15 = T_ADD_LIST.[traitid]
            LEFT JOIN [PERMISSION] AS P_ADD_LIST ON T_ADD_LIST.[permid] = P_ADD_LIST.[permissionid]
            LEFT JOIN [TRAIT] AS T_MODIF ON [desc].[descid] + 2 = T_MODIF.[traitid]
            LEFT JOIN [PERMISSION] AS P_MODIF ON T_MODIF.[permid] = P_MODIF.[permissionid]
            LEFT JOIN [TRAIT] AS T_DEL ON [desc].[descid] + 3 = T_DEL.[traitid]
            LEFT JOIN [PERMISSION] AS P_DEL ON T_DEL.[permid] = P_DEL.[permissionid]
            LEFT JOIN [TRAIT] AS T_HISTO ON [desc].[descid] + 4 = T_HISTO.[traitid]
            LEFT JOIN [PERMISSION] AS P_HISTO ON T_HISTO.[permid] = P_HISTO.[permissionid]
            LEFT JOIN [TRAIT] AS T_DUPLI ON [desc].[descid] + 5 = T_DUPLI.[traitid]
            LEFT JOIN [PERMISSION] AS P_DUPLI ON T_DUPLI.[permid] = P_DUPLI.[permissionid]
            /* ICON PJ */
            LEFT JOIN [desc] AS PJ_DESC ON [desc].[file] = [pj_desc].[file] and [pj_desc].[descid] = (@Descid + 91)
            /* DROIT TREATEMENT PJ */
            LEFT JOIN [PERMISSION] AS PJ_VIEW ON [pj_desc].[viewpermid] = [pj_view].[permissionid]
            /* ICON Description */
            LEFT JOIN [desc] AS DESCRIPTION_DESC ON [desc].[file] = [DESCRIPTION_DESC].[file] and [DESCRIPTION_DESC].[descid] = (@Descid + 89)
            /* ICON INFO */
            LEFT JOIN [desc] AS INFO_DESC ON [desc].[file] = [info_desc].[file] and [info_desc].[descid] = (@Descid + 93)
            /* ICON NOTE */
            LEFT JOIN [desc] AS NOTE_DESC ON [desc].[file] = [note_desc].[file] and [note_desc].[descid] = (@Descid + 94)
            /* TRAITEMENTS */
            LEFT JOIN [TRAIT] AS T_ADD_MULTI ON [desc].[descid] + 6 = T_ADD_MULTI.[traitid]
            LEFT JOIN [PERMISSION] AS P_ADD_MULTI ON T_ADD_MULTI.[permid] = P_ADD_MULTI.[permissionid]
            LEFT JOIN [TRAIT] AS T_MODIF_MULTI ON [desc].[descid] + 7 = T_MODIF_MULTI.[traitid]
            LEFT JOIN [PERMISSION] AS P_MODIF_MULTI ON T_MODIF_MULTI.[permid] = P_MODIF_MULTI.[permissionid]
            LEFT JOIN [TRAIT] AS T_DEL_MULTI ON [desc].[descid] + 8 = T_DEL_MULTI.[traitid]
            LEFT JOIN [PERMISSION] AS P_DEL_MULTI ON T_DEL_MULTI.[permid] = P_DEL_MULTI.[permissionid]
            LEFT JOIN [TRAIT] AS T_DUPLI_MULTI ON [desc].[descid] + 9 = T_DUPLI_MULTI.[traitid]
            LEFT JOIN [PERMISSION] AS P_DUPLI_MULTI ON T_DUPLI_MULTI.[permid] = P_DUPLI_MULTI.[permissionid]
            
            LEFT JOIN [TRAIT] AS T_ADD_PJ ON [desc].[descid] + 11 = T_ADD_PJ.[traitid]
            LEFT JOIN [PERMISSION] AS P_ADD_PJ ON T_ADD_PJ.[permid] = P_ADD_PJ.[permissionid]

            LEFT JOIN [TRAIT] AS T_MODIF_PJ ON [desc].[descid] + 12 = T_MODIF_PJ.[traitid]
            LEFT JOIN [PERMISSION] AS P_MODIF_PJ ON T_MODIF_PJ.[permid] = P_MODIF_PJ.[permissionid]

            LEFT JOIN [TRAIT] AS T_DEL_PJ ON [desc].[descid] + 13 = T_DEL_PJ.[traitid]
            LEFT JOIN [PERMISSION] AS P_DEL_PJ ON T_DEL_PJ.[permid] = P_DEL_PJ.[permissionid]            

            LEFT JOIN [TRAIT] AS T_MULTI_FROMFILTER ON [desc].[descid] + 17 = T_MULTI_FROMFILTER.[traitid]
            LEFT JOIN [PERMISSION] AS P_MULTI_FROMFILTER ON T_MULTI_FROMFILTER.[permid] = P_MULTI_FROMFILTER.[permissionid]        
            
            LEFT JOIN [PERMISSION] AS P_LEFT_MENU on P_LEFT_MENU.[PermissionId]   = [DESC].[LeftMenuLinksPermId]
            
                  
            LEFT JOIN [TRAIT] AS T_GLOBAL_LINK ON [desc].[descid] + 16 = T_GLOBAL_LINK.[traitid]
            LEFT JOIN [PERMISSION] AS P_GLOBAL_LINK ON T_GLOBAL_LINK.[permid] = P_GLOBAL_LINK.[permissionid]       
            
            
            
            
where [desc].[descid] = @Descid order by libelle
