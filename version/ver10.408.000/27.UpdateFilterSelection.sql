DELETE FROM [SELECTIONS] WHERE [TAB] IN (104000,105000) AND [USERID] = 0
	AND [selectid] not in ( SELECT ISNULL(pref.SelectId,0) FROM [PREF] )
	AND [selectid] not in ( SELECT ISNULL(config.TabOrderId,0) FROM config)


UPDATE [selections] SET listcol='104001;104008;104007;104006', ListColWidth=null, ListSort=null,ListOrder=null where tab = 104000
