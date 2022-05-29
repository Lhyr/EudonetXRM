/*
Function de split
*/
CREATE  FUNCTION [dbo].[efc_split](@sChaine as varchar(max), @sep as varchar(5)  ) RETURNS table AS 
 return 


-- TRANSFORMATION --

SELECT   
	SUBSTRING(@sep + data + @sep,N+1,CHARINDEX(@sep,@sep+data+@sep,N+1)-N-1)  [Element] ,
	row_number() over (order by CHARINDEX(@sep,@sep+data+@sep,N+1)) N
FROM 	(select @sChaine as data ) Datas
INNER JOIN  cfc_getIDS()  ON SUBSTRING(@sep+data+@sep,N,1) =@sep AND  N < LEN(data)+1
GROUP BY SUBSTRING(@sep + data + @sep,N+1,CHARINDEX(@sep,@sep+data+@sep,N+1)-N-1),CHARINDEX(@sep,@sep+data+@sep,N+1)



