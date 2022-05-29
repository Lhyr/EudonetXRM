IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'efc_splitxml' )
BEGIN
	DROP FUNCTION [efc_splitxml]
END

 