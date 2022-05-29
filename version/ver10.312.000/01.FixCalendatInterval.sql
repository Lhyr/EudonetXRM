  update [PREF] set 
	CalendarMinutesInterval =   case  
			when 	[PREF].CalendarMinutesInterval	  IS NULL  AND PREF.UserId<>0 then NULL
			when ISNULL( [PREF].CalendarMinutesInterval,0) < 5 then 5
			else [PREF].CalendarMinutesInterval
		END,
	CalendarMonthIntervalDuration =    case  
			when 	[PREF].CalendarMonthIntervalDuration	  IS NULL  AND PREF.UserId<>0 then NULL
			when isnull([PREF].CalendarMonthIntervalDuration,0) < 30 then 30
			else [PREF].CalendarMonthIntervalDuration
		END

,
	CalendarWorkingDays =    case  
			when isnull([PREF].CalendarWorkingDays,'') = '' AND PREF.UserId<>0 then NULL
			when isnull([PREF].CalendarWorkingDays,'') = '' and pref.userid = 0 then '2;3;4;5;6'
			else [PREF].CalendarWorkingDays
		END


	FROM [PREF]
  WHERE [PREF].[Tab] in ( select [descid] from  [desc] where [type] = 1 and  [descid] like '%00')