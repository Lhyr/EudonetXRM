/*********************************************************************
	CREATED :	SPH 
	DATE  : 07/03/2018
	DESC  :  Retourne les colonnes utilisé par un index
		@tabname : table de l'index
		@idname : nom de l'index


****************************************************************************/
CREATE FUNCTION [dbo].[efc_GetIndexCols] (
	@tabname VARCHAR(100)
	,@idname VARCHAR(max)
	
	)
RETURNS @columns  table
(
	colsIdx varchar(max),
	colInc varchar(max)
)	
AS
BEGIN


	declare @col as varchar(max)
	declare @colin as varchar(max)

	set @colin=''
	set @col=''
	
	SELECT 

 
		
		@col = @col +
			case when  
				ic.is_included_column   = 0 then  + 
					case when len(@col) > 0 then ';'  else '' end
					+ 	
					c.name
			 else ''
			end ,
					@colin = @colin +
		case when  
			ic.is_included_column   = 1 then  +
			case when len(@colin) >0 then ';'  else '' end
			 + c.name
			 else ''
			end 


	FROM sys.index_columns ic
	INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
	INNER JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
	INNER JOIN sys.objects o ON c.object_id = o.object_id

	where 	OBJECT_NAME(i.object_id) =  @tabname and i.name = @idname

	order by i.name
	
	insert into @columns select @col, @colin

	return
	
End