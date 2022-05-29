/*
*	Remet à null le userid des filtres de type doublon
*	CNA - 29/07/2016
*/

UPDATE [FILTER] SET [UserId] = null WHERE [Type] = 1
