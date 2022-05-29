
--On affiche appartient à et modifié le comme pour les filtres et rapports. 
-- GCH : Ne semble pas avoir été joué sur certaines base, donc recopie du script de la 7.602
if exists (
select 1 from selections where tab = 107000 and ListCol <> '107001;107099;107096'
)
begin
	delete from pref where tab=107000
	delete from selections where tab=107000
	insert into pref (tab,userid, listcol) select 107000, 0,'107001;107099;107096'
	insert into pref (tab,userid, listcol) select  107000, userid, '107001;107099;107096' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select 107000,'Vue par défaut', '107001;107099;107096','0;0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select 107000,'Vue par défaut', '107001;107099;107096','0;0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = 107000
end