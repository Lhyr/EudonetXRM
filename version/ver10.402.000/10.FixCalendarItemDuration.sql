-- Correctif sur la colonne CalendarItemDefaultDuration à NULL pour certaines bases, provoquant une erreur de conversion varchar vers numeric
-- cf. demande de perm #61 707
-- On insère la valeur minimale acceptée pour cette colonne (5)

update [PREF] set 
CalendarItemDefaultDuration =   case  
		when 	[PREF].CalendarItemDefaultDuration	  IS NULL  AND PREF.UserId<>0 then NULL
		when ISNULL( [PREF].CalendarItemDefaultDuration,0) < 5 then 5
		else [PREF].CalendarItemDefaultDuration
	end
FROM [PREF]
WHERE [PREF].[Tab] in ( select [descid] from  [desc] where [type] = 1 and  [descid] like '%00')