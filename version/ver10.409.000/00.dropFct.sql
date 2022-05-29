IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'efc_getGridXY' )
BEGIN
	DROP FUNCTION efc_getGridXY
END
