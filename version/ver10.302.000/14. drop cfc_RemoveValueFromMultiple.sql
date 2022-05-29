IF EXISTS (
		SELECT [Type]
			,Type_Desc
			,*
		FROM Sys.Objects
		WHERE [name] LIKE 'efc_RemoveValueFromMultiple'
		)
	DROP FUNCTION [dbo].[efc_RemoveValueFromMultiple]