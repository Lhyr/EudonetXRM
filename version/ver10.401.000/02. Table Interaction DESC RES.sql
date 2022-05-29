declare @nTab INT = 118000
if not exists(select * from [DESC] where [descid] = @nTab )
begin
	/* Nettoyage Permission*/
	declare @permissionToDeleteIds as varchar(max)

	select @permissionToDeleteIds = isnull(@permissionToDeleteIds, '') + ';' + isnull(cast([AddPermission] as varchar), '') + ';' + isnull(cast([UpdatePermission] as varchar), '') + ';' + isnull(cast([DeletePermission] as varchar), '')
	from [FILEDATAPARAM]
	where [DescId] >= @nTab and [DescId] < @nTab + 100

	update [FILEDATAPARAM] set 
	[AddPermission] = null
	,[UpdatePermission] = null
	,[DeletePermission] = null
	where [DescId] >= @nTab and [DescId] < @nTab + 100

	select @permissionToDeleteIds = isnull(@permissionToDeleteIds, '') + ';' + isnull(cast([ViewPermId] as varchar), '') + ';' + isnull(cast([UpdatePermId] as varchar), '')
	from [FILTER]
	where [Tab] = @nTab

	update [FILTER] set 
	[ViewPermId] = null
	,[UpdatePermId] = null
	where [Tab] = @nTab

	delete from [PERMISSION]
	where charindex(';' + cast([PermissionId] as varchar) + ';', ';' + @permissionToDeleteIds + ';') > 0

	/* Nettoyage */
	delete [RES] where [ResId] >=@nTab and [ResId] < @nTab + 100
	delete [FileData] where [DescId] >=@nTab and [DescId] < @nTab + 100
	delete [FILEDATAPARAM] where [DescId] >=@nTab and [DescId] < @nTab + 100
	delete [FILTER] where [Tab] = @nTab
	delete [UserValue] where [DescId] >=@nTab and [DescId] < @nTab + 100
	delete [DESCADV] where [DescId] >=@nTab and [DescId] < @nTab + 100
	delete [DESC] where [DescId] >=@nTab and [DescId] < @nTab + 100

	/* CREATION DES DESC */
	--DESC Table INTERACTION
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length], [type], [Disporder]) select @nTab, 'INTERACTION', 'TPL', 0, 0, 2, @nTab

	update [DESC] set
	[InterPpNeeded] = 1
	,[InterPmNeeded] = 0
	,[InterEventNeeded] = 0
	,[InterEventHidden] = 0
	,[NoDefaultLink_100] = 0
	,[NoDefaultLink_200] = 0
	,[NoDefaultLink_300] = 0
	,[DefaultLink_100] = 0
	,[DefaultLink_200] = 0
	,[DefaultLink_300] = 0
	,[AutoSelectEnabled] = 0
	,[AutoSelectValue] = 0
	,[Columns] = '125,A,25,125,A,25'
	where [DescId] = @nTab

	--DESC Champs Systèmes
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 74, 'INTERACTION', 'TPL74', 28, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 76, 'INTERACTION', 'TPL76', 1, 50
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 77, 'INTERACTION', 'TPL77', 10, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 78, 'INTERACTION', 'TPL78', 2, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 79, 'INTERACTION', 'TPL79', 3, 0
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 80, 'INTERACTION', 'TPL80', 1, 50
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 81, 'INTERACTION', 'TPL81', 8, 512
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 82, 'INTERACTION', 'TPL82', 10, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 83, 'INTERACTION', 'TPL83', 10, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 84, 'INTERACTION', 'TPL84', 3, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 89, 'INTERACTION', 'TPL89', 2, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 91, 'INTERACTION', 'TPL91', 0, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 92, 'INTERACTION', 'TPL92', 8, 512
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 93, 'INTERACTION', 'TPL93', 9, 8000
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 94, 'INTERACTION', 'TPL94', 9, 8000
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 95, 'INTERACTION', 'TPL95', 2, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 96, 'INTERACTION', 'TPL96', 2, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 97, 'INTERACTION', 'TPL97', 8, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 98, 'INTERACTION', 'TPL98', 8, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 99, 'INTERACTION', 'TPL99', 8, 100

	--DESC Rubriques
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 2, 'INTERACTION', 'TPL02', 29, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 3, 'INTERACTION', 'TPL03', 29, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 4, 'INTERACTION', 'TPL04', 3, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 5, 'INTERACTION', 'TPL05', 2, 0
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 6, 'INTERACTION', 'TPL06', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 7, 'INTERACTION', 'TPL07', 15, 1
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 8, 'INTERACTION', 'TPL08', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 9, 'INTERACTION', 'TPL09', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 10, 'INTERACTION', 'TPL10', 1, 512
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 11, 'INTERACTION', 'TPL11', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 12, 'INTERACTION', 'TPL12', 8, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 13, 'INTERACTION', 'TPL13', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 14, 'INTERACTION', 'TPL14', 1, 100
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 15, 'INTERACTION', 'TPL15', 9, 8000
	insert into [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 16, 'INTERACTION', 'TPL16', 9, 8000

	update [DESC] set
	[Case] = 0
	,[Historic] = 0
	,[Obligat] = 0
	,[Multiple] = 0
	,[Popup] = 0
	,[ActiveTab] = 0
	,[ActiveBkmPP] = 1
	,[ActiveBkmPM] = 1
	,[ActiveBkmEvent] = 1
	,[GetLevel] = 1
	,[ViewLevel] = 1
	,[UserLevel] = 0
	,[InterPP] = 0
	,[InterPM] = 0
	,[InterEvent] = 0
	,[TabIndex] = 0
	,[Bold] = 0
	,[Italic] = 0
	,[Underline] = 0
	,[Flat] = 0
	,[Disabled] = 0
	,[Unicode] = 0
	,[NbrBkmInline] = 0
	,[TreatmentMaxRows] = 0
	,[TreeViewUserList] = 0
	,[FullUserList] = 0
	,[BreakLine] = 0
	,[NoCascadePPPM] = 0
	,[NoCascadePMPP] = 0
	,[AutoCompletion] = 0
	where [DescId] >=@nTab and [DescId] < @nTab + 100

	update [DESC] set
	[PopupDescId] = 0
	,[BoundDescId] = 0
	,[DefaultFormat] = 0
	,[ReadOnly] = 0
	,[Html] = 0
	,[Relation] = 0
	,[Storage] = 0
	,[NoDefaultClone] = 0
	,[ComputedFieldEnabled] = 0
	where [DescId] >=@nTab and [DescId] < @nTab + 100
	and [DescId] not in (@nTab, @nTab + 91)

	update [DESC] set
	[Multiple] = 1
	where [DescId] in (@nTab + 80, @nTab + 92, @nTab + 10)

	update [DESC] set
	[Popup] = 3
	,[PopupDescId] = [DescId]
	where [DescId] in (@nTab + 6, @nTab + 8, @nTab + 9, @nTab + 10, @nTab + 11, @nTab + 13, @nTab + 14)

	update [DESC] set
	[Icon] = 'TrombOut.jpg'
	where [DescId] in (@nTab + 91)

	update [DESC] set
	[Html] = 1
	where [DescId] in (@nTab + 94)

	update [DESC] set
	[FullUserList] = 1
	where [DescId] in (@nTab + 81, @nTab + 92, @nTab + 97, @nTab + 98, @nTab + 99, @nTab + 12)

	update [DESC] set
	[ActiveTab] = 1
	,[InterPP] = 1
	,[BreakLine] = 7
	where [DescId] = @nTab

	--Mise en page Champs Systèmes
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 74 where [DescId] = @nTab + 74
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 76 where [DescId] = @nTab + 76
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 77 where [DescId] = @nTab + 77
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 78 where [DescId] = @nTab + 78
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 79 where [DescId] = @nTab + 79
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 80 where [DescId] = @nTab + 80
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 81 where [DescId] = @nTab + 81
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 82 where [DescId] = @nTab + 82
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 83 where [DescId] = @nTab + 83
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 84 where [DescId] = @nTab + 84
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 89 where [DescId] = @nTab + 89
	update [DESC] set [Rowspan] = 0, [Colspan] = 0, [DispOrder] = 91 where [DescId] = @nTab + 91
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 92 where [DescId] = @nTab + 92
	update [DESC] set [Rowspan] = 4, [Colspan] = 2, [DispOrder] = 93 where [DescId] = @nTab + 93
	update [DESC] set [Rowspan] = 4, [Colspan] = 2, [DispOrder] = 16 where [DescId] = @nTab + 94
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 95 where [DescId] = @nTab + 95
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 96 where [DescId] = @nTab + 96
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 97 where [DescId] = @nTab + 97
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 98 where [DescId] = @nTab + 98
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 99 where [DescId] = @nTab + 99

	--Mise en page Rubriques
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 5, [x] = 0, [y] = 2 where [DescId] = @nTab + 2
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 6, [x] = 1, [y] = 2 where [DescId] = @nTab + 3
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 1, [x] = 0, [y] = 0 where [DescId] = @nTab + 4
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 4, [x] = 1, [y] = 1 where [DescId] = @nTab + 5
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 3, [x] = 0, [y] = 1 where [DescId] = @nTab + 6
	update [DESC] set [Rowspan] = 1, [Colspan] = 2, [DispOrder] = 7, [x] = 0, [y] = 3 where [DescId] = @nTab + 7
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 9, [x] = 1, [y] = 4 where [DescId] = @nTab + 8
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 8, [x] = 0, [y] = 4 where [DescId] = @nTab + 9
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 10, [x] = 0, [y] = 5 where [DescId] = @nTab + 10
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 11, [x] = 1, [y] = 5 where [DescId] = @nTab + 11
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 12, [x] = 0, [y] = 6 where [DescId] = @nTab + 12
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 13, [x] = 1, [y] = 6 where [DescId] = @nTab + 13
	update [DESC] set [Rowspan] = 1, [Colspan] = 1, [DispOrder] = 14, [x] = 0, [y] = 7 where [DescId] = @nTab + 14
	update [DESC] set [Rowspan] = 4, [Colspan] = 2, [DispOrder] = 16, [x] = 0, [y] = 8 where [DescId] = @nTab + 15
	update [DESC] set [Rowspan] = 4, [Colspan] = 2, [DispOrder] = 17, [x] = 0, [y] = 12 where [DescId] = @nTab + 16


	/* DESCADV */
	insert into [DESCADV] ([DescId], [Parameter], [Value], [Category]) select @nTab, 16, 3, 0
	insert into [DESCADV] ([DescId], [Parameter], [Value], [Category]) select @nTab, 34, 1, 1
	insert into [DESCADV] ([DescId], [Parameter], [Value], [Category]) select @nTab + 2, 19, 200, 0
	insert into [DESCADV] ([DescId], [Parameter], [Value], [Category]) select @nTab + 3, 19, 400, 0
	insert into [DESCADV] ([DescId], [Parameter], [Value], [Category]) select [DESC].[DescId], 2, 0, 0 from [DESC] where [DESC].[DescId] > @nTab and [DESC].[DescId] < @nTab + 100


	/* CREATION DES RES */
	--RES Champs Sytèmes
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab, 'Interaction', 'Interaction', 'Interaction', 'Interaction', 'Interaction', 'Interaction'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 74, 'Géolocalisation', 'Geolocation', 'Geolocation', 'Geolocation', 'Geolocation', 'Geolocation'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 76, 'Alertes - Son', 'Alerts - Sound', 'Warnungen - Ton', 'Alarmen - Geluid', 'Alertas - Sonido', 'Allarmi - Suono'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 77, 'Alertes - Heure', 'Alerts - Hour', 'Warnungen - Stunde', 'Alarmen - Uur', 'Alertas - Hora', 'Allarmi - Ora'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 78, 'Alertes - Date', 'Alerts - Date', 'Warnungen - Datum', 'Alarmen - Datum', 'Alertas - Fecha', 'Allarmi - Data'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 79, 'Alertes', 'Alerts', 'Warnungen', 'Alarmen', 'Alertas', 'Allarmi'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 80, 'Couleurs', 'Colours', 'Farben', 'Kleuren', 'Colores', 'Colori'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 81, 'Couleurs', 'Colours', 'Farben', 'Kleuren', 'Colores', 'Colori'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 82, 'Périodicité', 'Recurrence', 'Periodizität', 'Periodiciteit', 'Periodicidad', 'Programmazione'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 83, 'Type', 'Type', 'Typ', 'Type', 'Tipo', 'Tipo'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 84, 'Confidentiel', 'Confidential', 'Vertraulich', 'Vertrouwelijk', 'Confidencial', 'Confidenziale'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 89, 'Fin', 'End', 'Ende', 'Einde', 'Fin', 'Fine'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 91, 'Annexes', 'Attachments', 'Anlagen', 'Bijlagen', 'Anexos', 'Allegati'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 92, 'test template 2 de', 'test template 2 of', 'test template 2 von', 'test template 2 van', 'test template 2 de', 'test template 2 di'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 93, 'Informations', 'Information', 'Informationen', 'Informatie', 'Información', 'Informazioni'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 94, 'Notes', 'Notes', 'Vermerke', 'Aantekeningen', 'Notas', 'Note'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 95, 'Créé le', 'Date created', 'Geschaffen am', 'Gecreëerd op', 'Creado el', 'Creato il'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 96, 'Modifié le', 'Date modified', 'Geändert am', 'Gewijzigd op', 'Modificado el', 'Modificato il'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 97, 'Créé par', 'Created by', 'Geschaffen von', 'Gecreëerd door', 'Creado por', 'Creato da'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 98, 'Modifié par', 'Modified by', 'Geändert von', 'Gewijzigd door', 'Modificado por', 'Modificato da'
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @nTab + 99, 'Appartient à', 'Belongs to', 'Gehört zu', 'Behoort tot', 'Pertenece a', 'Appartiene a'


	--RES Rubriques
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05], [LANG_06], [LANG_07], [LANG_08], [LANG_09], [LANG_10]) 
	select @nTab + 2, [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05], [LANG_06], [LANG_07], [LANG_08], [LANG_09], [LANG_10] from [RES] where [ResId] = 200
	insert into [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05], [LANG_06], [LANG_07], [LANG_08], [LANG_09], [LANG_10]) 
	select @nTab + 3, [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05], [LANG_06], [LANG_07], [LANG_08], [LANG_09], [LANG_10] from [RES] where [ResId] = 400

	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 4, 'Archiver', 'Archive'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 5, 'Date', 'Date'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 6, 'Type d''interaction', 'Interaction type'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 7, 'Information Consentement', 'Details of consent'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 8, 'Statut du consentement', 'Consent status'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 9, 'Type de campagne', 'Campaign type'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 10, 'Motif du traitement', 'Type of processing'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 11, 'Condition d''obtention', 'Consent obtained by'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 12, 'Responsable du traitement', 'Processing manager'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 13, 'Contact mineur', 'Minor'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 14, 'Consentement autorisé par', 'Consent authorised by'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 15, 'Coordonnées', 'Contact details'
	insert into [RES] ([ResId], [LANG_00], [LANG_01]) select @nTab + 16, 'Autres infos consentement', 'Other consent information'

	update [RES] set 
	[LANG_02] = [LANG_01]
	,[LANG_03] = [LANG_01]
	,[LANG_04] = [LANG_01]
	,[LANG_05] = [LANG_01]
	,[LANG_06] = [LANG_01]
	,[LANG_07] = [LANG_01]
	,[LANG_08] = [LANG_01]
	,[LANG_09] = [LANG_01]
	,[LANG_10] = [LANG_01]
	where [ResId] >= @nTab + 4 and [ResId] <= @nTab + 69


	/* SELECTIONS */
	delete from [SELECTIONS] where [Tab] = @nTab

	/* PREF */
	delete from [PREF] where [Tab] = @nTab
	insert into [PREF] ([Tab], [UserId]) select @nTab, 0

	update [PREF] set 
	[AdrJoin] = 1
	,[AutoLinkEnabled] = 0
	,[AutoLinkCreation] = 0
	,[ActiveBkm] = 0
	where [Tab] = @nTab and [UserId] = 0

	insert into [PREF] ([Tab], [UserId], [DefaultOwner], [ActiveBkm]) select @nTab, [UserId], [UserId], 0 from [USER]


	/* Paramètres Catalogues */
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 6, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 8, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 9, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 10, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 11, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 13, 1, '0;1', '[TEXT]', '[TEXT]'
	insert into [FILEDATAPARAM] ([DescId], [DataEnabled], [LangUsed], [DisplayMask], [SortBy]) select @nTab + 14, 1, '0;1', '[TEXT]', '[TEXT]'

	/* Valeurs Catalogues */
	--TPL06
	declare @nTypeConsentCatValueId numeric(18) = null
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 6, 'consent', 'Consentement', 'Consent'
	select @nTypeConsentCatValueId = scope_identity()

	--TPL08
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 8, 'optin', 'Opt-in', 'Opt-in'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 8, 'optout', 'Opt-out', 'Opt-out'

	--TPL09
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'mail-news', 'Emailing newsletter', 'Email newsletter'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'mail-comm', 'Emailing commercial', 'Commercial emails'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'mail-promo', 'Emailing promotionnel', 'Promotional emails'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'sms-prosp', 'SMS prospection', 'SMS prospection'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'sms-info', 'SMS informations', 'SMS information'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 9, 'sms-promo', 'SMS promotionnel', 'SMS promotions'

	--TPL11
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'phone', 'Oral téléphonique', 'Over the phone'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'face', 'Oral face à face', 'Face to face'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'email', 'Email', 'Email'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'webform', 'Formulaire web', 'Web form'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'written', 'Ecrit', 'Written'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 11, 'sms', 'SMS', 'SMS'

	--TPL13
	declare @nContactMineurCatValueId numeric(18) = null
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 13, 'yes', 'Oui', 'Yes'
	select @nContactMineurCatValueId = scope_identity()
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 13, 'no', 'Non', 'No'

	--TPL14
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 14, 'father', 'Représentant légal - Père', 'Legal guardian - Father'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 14, 'mother', 'Représentant légal - Mère', 'Legal guardian - Mother'
	insert into [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01]) select @nTab + 14, 'guardian', 'Représentant légal - Tuteur', 'Legal guardian'

	update [FILEDATA] set 
	[Tip_Lang_00_Format] = 0
	,[Tip_Lang_01_Format] = 0
	,[Tip_Lang_02_Format] = 0
	,[Tip_Lang_03_Format] = 0
	,[Tip_Lang_04_Format] = 0
	,[Tip_Lang_05_Format] = 0
	,[Tip_Lang_06_Format] = 0
	,[Tip_Lang_07_Format] = 0
	,[Tip_Lang_08_Format] = 0
	,[Tip_Lang_09_Format] = 0
	,[Tip_Lang_10_Format] = 0
	where [DescId] >= @nTab + 1 and [DescId] <= @nTab + 69

	/* Permissions catalogues */
	DECLARE @nAddPermissionId numeric(18)
	DECLARE @nUpdatePermissionId numeric(18)
	DECLARE @nDeletePermissionId numeric(18)

	--TPL06
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 6


	--TPL08
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 8


	--TPL09
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (0, 99, '0')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (0, 99, '0')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (0, 99, '0')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 9


	--TPL11
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 11


	--TPL13
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 13


	--TPL14
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @nTab + 14


	/* Règles Affichage, Obligatoire */
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()

	declare @ruleConsentId numeric(18)
	insert into [FILTER] ([Tab], [Libelle], [Type], [UpdatePermId], [DateLastModified], [Param]) select @nTab, 'Type interaction = consentement', 2, @nUpdatePermissionId, getdate(), '&file_0=' + cast(@nTab as varchar) + '&field_0_0=' + cast(@nTab + 6 as varchar) + '&op_0_0=0&value_0_0=' + cast(@nTypeConsentCatValueId as varchar) + '&question_0_0=0'
	select @ruleConsentId = scope_identity()

	update [DESC] set 
	[ViewRulesId] = '$' + cast(@ruleConsentId as varchar) + '$'
	where [Descid] in (@nTab + 7, @nTab + 8, @nTab + 9, @nTab + 10, @nTab + 11, @nTab + 12, @nTab + 13, @nTab + 14, @nTab + 15, @nTab + 16)

	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()

	declare @ruleContactMineurId numeric(18)
	insert into [FILTER] ([Tab], [Libelle], [Type], [UpdatePermId], [DateLastModified], [Param]) select @nTab, 'Contact mineur = oui', 2, @nUpdatePermissionId, getdate(), '&file_0=' + cast(@nTab as varchar) + '&field_0_0=' + cast(@nTab + 13 as varchar) + '&op_0_0=0&value_0_0=' + cast(@nContactMineurCatValueId as varchar) + '&question_0_0=0'
	select @ruleContactMineurId = scope_identity()

	update [DESC] set 
	[ObligatRulesId] = '$' + cast(@ruleContactMineurId as varchar) + '$'
	where [Descid] in (@nTab + 14, @nTab + 15)


	/* Valeurs par defaut */
	update [desc] set [default] = '<DATE>' where [descid] = @nTab + 5
	update [desc] set [default] = cast(@nTypeConsentCatValueId as varchar) where [descid] = @nTab + 6
END