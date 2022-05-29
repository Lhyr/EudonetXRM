
/*
maj du disporder pour les spécifs onglet web
*/
update [SPECIFS] set DispOrder = ee.ord
from 
 ( select  ROW_NUMBER() over (partition by a.tab  order by  specifid) ord , SpecifId
 from SPECIFS a  where a.SpecifType in (11,12) ) as ee
 where ee.SpecifId=SPECIFS.SpecifId and DispOrder is null

