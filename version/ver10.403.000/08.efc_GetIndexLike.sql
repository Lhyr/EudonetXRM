/*********************************************************************
	CREATED :	SPH 
	DATE  : 07/03/2018
	DESC  :  retourne le nom des index avec les propriétés fournies
		@tabname : table de l'index
		@colsname : liste des colonnes de l'index séparé par des ';'
		@@includecols : liste des colonnes include séparé par des ';'
 
 select * from [dbo].[efc_GetIndexLike]('user','userid','GroupId;UserLevel;UserLogin')

****************************************************************************/
CREATE FUNCTION [dbo].[efc_GetIndexLike] (
	 @tabname VARCHAR(100)
	,@colsname VARCHAR(max)
	,@includecols VARCHAR(max)
	)
RETURNS TABLE
AS
 
  return 
		SELECT  i.name a
		FROM sys.index_columns ic
		INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
		INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
		INNER JOIN sys.objects o ON c.object_id = o.object_id
		WHERE object_name(ic.object_id) collate french_ci_ai = @tabname collate french_ci_ai
			AND (
				(
					CHARINDEX(';' + c.NAME collate french_ci_ai + ';', ';' + @colsname collate french_ci_ai + ';') > 0
					AND is_included_column = 0
					)
				OR (
					CHARINDEX(';' + c.NAME collate french_ci_ai + ';', ';' + @includecols collate french_ci_ai + ';') > 0
					AND is_included_column = 1
					)
				)
		GROUP BY i.NAME
		HAVING COUNT(1) = (SELECT   COUNT(n) FROM  dbo.efc_split(  @colsname  + coalesce( ';' + nullif(@includecols,'') ,''),';')   where Element<>'')
 
 