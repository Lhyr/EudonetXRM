/*
SELECT

	AA.descid, 
	bb.descid, 
	
	AA.POINT, BB.POINT


 
 , BB.POINT.STIntersection(AA.Point).STGeometryType ()
  FROM [dbo].[efc_getGridXY] (400)  AA
	CROSS JOIN  [dbo].[efc_getGridXY](400)  BB
	WHERE AA.DESCID <> BB.DESCID
	AND  BB.POINT.STIntersects (AA.Point)  = 1 -- A des points de contacts
	AND  BB.POINT.STIntersection(AA.Point).STGeometryType () not in( 'Point','LineString') -- L'intersection est "plus" qu'un point (angle) ou une frontière
*/
create FUNCTION [dbo].[efc_getGridXY](@descidTab int) RETURNS table AS 
return 
SELECT 
	
	GRID.Y YStart,
	GRID.X XStart,

	GRID.Y + Rowspan - 1 YEnd,
	GRID.X + Colspan  - 1 Xend,

	DESCID,
	DISPORDER,
	ROWSPAN,
	COLSPAN, 

	[LANG_00],
	 geometry::STGeomFromText('POLYGON((
			' + CAST( GRID.X as varchar(5)) +  ' -'  + CAST( GRID.Y as varchar(5)) +  ',
			' + CAST( GRID.X + Colspan  as varchar(5)) +  ' -'  + CAST( GRID.Y as varchar(5)) +  ',
			' + CAST( GRID.X  + Colspan  as varchar(5)) +  ' -'  + CAST( GRID.Y  + Rowspan  as varchar(5)) +  ',
			' + CAST( GRID.X  as varchar(5)) +  ' -'  + CAST( GRID.Y  + Rowspan  as varchar(5)) +  ',
			' + CAST( GRID.X as varchar(5)) +  ' -'  + CAST( GRID.Y as varchar(5)) +  '
			 ))'
			 
			 ,0) POINT  

FROM	
	 ( 
		SELECT 	Y,X,DESCID,DISPORDER,ROWSPAN,COLSPAN, [LANG_00]
		FROM [DESC]
		INNER JOIN [RES] ON [RESID] = [DESCID]
		WHERE [DESCID] - [DESCID] % 100 = @descidTab AND X IS NOT NULL
	) as  GRID 
--ORDER BY GRID.Y,GRID.X



 