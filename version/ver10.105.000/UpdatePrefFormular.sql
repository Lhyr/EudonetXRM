
--On affiche appartient à et modifié le comme pour les filtres et rapports.
if exists (
select 1 from selections where tab = 113000 and ListCol <> '113001;113099;113096'
)
begin
	delete from pref where tab=113000
	delete from selections where tab=113000
	insert into pref (tab,userid, listcol) select 113000, 0,'113001;113099;113096'
	insert into pref (tab,userid, listcol) select  113000, userid, '113001;113099;113096' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select 113000,'Vue par défaut', '113001;113099;113096','0;0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select 113000,'Vue par défaut', '113001;113099;113096','0;0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = 113000
end