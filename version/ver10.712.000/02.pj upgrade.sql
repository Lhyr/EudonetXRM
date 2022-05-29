UPDATE DESCADV set Value = 0 WHERE PARAMETER = 8 AND DescId = 102000 

IF EXISTS (SELECT 1 FROM [DESC] WHERE DescId BETWEEN 102001 AND 102099 AND (X IS NULL OR Y IS NULL))
BEGIN
	UPDATE [DESC] SET Columns = '125,A,25,125,A,25', BreakLine = 0 WHERE DescId = 102000
	UPDATE [DESC] SET DispOrder = 1, X = 0, Y = 0, Colspan = 2 WHERE DescId = 102001
	UPDATE [DESC] SET DispOrder = 2, X = 0, Y = 1, Colspan = 2  WHERE DescId = 102002
	UPDATE [DESC] SET DispOrder = 3, X = 0, Y = 2  WHERE DescId = 102004
	UPDATE [DESC] SET DispOrder = 4, X = 1, Y = 2  WHERE DescId = 102005
	UPDATE [DESC] SET DispOrder = 5, X = 0, Y = 3  WHERE DescId = 102006
	UPDATE [DESC] SET DispOrder = 6, X = 1, Y = 3  WHERE DescId = 102007
	UPDATE [DESC] SET DispOrder = 7, X = 0, Y = 4  WHERE DescId = 102008
	UPDATE [DESC] SET DispOrder = 8, X = 1, Y = 4  WHERE DescId = 102009
	UPDATE [DESC] SET DispOrder = 9, X = 0, Y = 5  WHERE DescId = 102010
	UPDATE [DESC] SET DispOrder = 10, X = 1, Y = 5  WHERE DescId = 102011
	UPDATE [DESC] SET DispOrder = 11, X = 0, Y = 6  WHERE DescId = 102012
	UPDATE [DESC] SET DispOrder = 12, X = 1, Y = 6  WHERE DescId = 102013
END
