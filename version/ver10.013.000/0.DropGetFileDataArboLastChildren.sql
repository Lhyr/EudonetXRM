IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'getFiledataArboLastChildrenDisplayAdv' )
BEGIN
	DROP FUNCTION getFiledataArboLastChildrenDisplayAdv
END
