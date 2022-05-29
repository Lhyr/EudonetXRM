-- US #2144 - Tâche #3126 - Demandes #66 928 et #80 251 - Les libellés des droits globaux "Export" (811) et "Emailing" (812) sont renommés pour être plus explicites sur la fonctionnalité associée.
-- Ils étaient précédemment utilisés comme libellés pour le module d'administration des seuils en v7, module qui n'a pas été repris ensuite. Ces libellés étaient affichés à côté de champs de saisie permettant d'indiquer les seuils à appliquer.
-- On renomme donc les libellés en "Ignorer les seuils - Export" et "Ignorer les seuils - Emailing" pour un usage plus explicite en XRM/E17/..., au détriment de leur cohérence en v7

UPDATE EUDORES..TRAIT SET
[LANG_00] = 'Ignorer les seuils - Export'
,[LANG_01] = 'Ignore thresholds - Export'
,[LANG_02] = 'Ignore thresholds - Export'
,[LANG_03] = 'Ignore thresholds - Export'
,[LANG_04] = ''
,[LANG_05] = 'Ignore thresholds - Export'
,[LANG_06] = ''
,[LANG_07] = ''
,[LANG_08] = ''
,[LANG_09] = ''
,[LANG_10] = ''
WHERE [TraitId] = 811

UPDATE EUDORES..TRAIT SET
[LANG_00] = 'Ignorer les seuils - Emailing'
,[LANG_01] = 'Ignore thresholds - Emailing'
,[LANG_02] = 'Ignore thresholds - Emailing'
,[LANG_03] = 'Ignore thresholds - Emailing'
,[LANG_04] = ''
,[LANG_05] = 'Ignore thresholds - Emailing'
,[LANG_06] = ''
,[LANG_07] = ''
,[LANG_08] = ''
,[LANG_09] = ''
,[LANG_10] = ''
WHERE [TraitId] = 812