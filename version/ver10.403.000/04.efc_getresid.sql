-- =============================================
-- Author:		Simon PHAM
-- Create date: 01/03/2018
-- Description:	Retourne la ressource d'un resid en fonction de la langue fournie
-- =============================================
CREATE FUNCTION dbo.efc_getResid
(		
	 @sLang as varchar(10),
	 @descid as numeric(18,0) --type de la colonne res
)
RETURNS TABLE 
AS
RETURN 
(

 SELECT RES FROM  res UNPIVOT (RES for LG in (lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10)) as u
 where LG = @sLang and ResId  = @descid


)
