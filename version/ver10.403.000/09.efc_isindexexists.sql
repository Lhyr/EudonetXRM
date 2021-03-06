/*********************************************************************
	CREATED :	SPH 
	DATE  : 07/03/2018
	DESC  :  Indique si le nombre  d'index comprenant les colonnes spécifiées
		@tabname : table de l'index
		@colsname : liste des colonnes de l'index séparé par des ';'
		@@includecols : liste des colonnes include séparé par des ';'
 
 select [dbo].[efc_IsIndexExists]('user','userid','GroupId;UserLevel;UserLogin')
 

****************************************************************************/
CREATE FUNCTION [dbo].[efc_IsIndexExists] (
	 @tabname VARCHAR(100)
	,@colsname VARCHAR(max)
	,@includecols VARCHAR(max)
	)
RETURNS int
AS
BEGIN
	 
	DECLARE @res AS INT
	select @res= COUNT(1)  from [dbo].[efc_GetIndexLike](@tabname,@colsname ,@includecols)
	return @res
END
