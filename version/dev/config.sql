declare @vers as varchar(15)
-- todo : ajouter dans le générateur de branche la modification automatique du num de version
set @vers = '10.703.000'
update [config] set [version] = @vers  where userid = 0  and version > @vers and version not like '7.%'