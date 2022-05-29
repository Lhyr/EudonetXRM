IF (SELECT COUNT(*) FROM sysobjects where name='efc_getFieldDependancyAll' and xtype='IF')=1
	drop  function efc_getFieldDependancyAll