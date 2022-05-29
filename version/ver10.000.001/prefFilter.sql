
delete from pref where tab=104000
delete from selections where tab=104000


insert into pref (tab,userid, listcol) select 104000, 0,'104007;104006'
insert into pref (tab,userid, listcol) select  104000, userid, '104007;104006' from [user]


insert into selections (tab,label,listcol, listcolwidth,userid) select 104000,'Vue par défaut', '104007;104006','0;0;0', 0
insert into selections (tab,label,listcol, listcolwidth,userid) select 104000,'Vue par défaut', '104007;104006','0;0;0', userid from [user]


update pref set selectid = selections.selectid from [pref]  inner join selections on selections.userid = [pref].userid and  [selections].[tab] = [pref].[tab] where [pref].[tab] = 104000




