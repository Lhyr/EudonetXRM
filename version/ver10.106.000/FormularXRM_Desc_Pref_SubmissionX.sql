/* ************************************************************
 *   MAB/HLA/JAS/KHA 2015-05-21
 *   Correctif de champs dans DESC ayant pu conserver un ancien libellé
 *   lorsque la base a été montée en 10.006 avec une version non finalisée d'XRM
 *   cf. échanges de mails de ce jour (demande orale)
 ************************************************************************* */
update [DESC] set [Field] = 'SubmissionBody' where DescId = 113009 
update [DESC] set [Field] = 'SubmissionBodyCss' where DescId = 113010 
update [DESC] set [Field] = 'SubmissionRedirectUrl' where DescId = 113011
