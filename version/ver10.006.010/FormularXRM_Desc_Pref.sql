/* ************************************************************
 *   MOU 26-06-2014 
 *   Insertion des descriptions des champs de la table FormularXRM dans Desc
 *   cf. 24897
 ************************************************************************* */
if (  EXISTS ( select * from [desc] where descid >= 113000 and DescId < 113100 ))
begin
	delete [res] where resid >=113000 AND resid < 113100
	delete [desc] where descid >=113000 AND descid < 113100
end
 
 if ( Not EXISTS ( select * from [desc] where descid >= 113000 and DescId < 113100 ))
begin
	delete [res] where resid >=113000 AND resid < 113100
	delete [desc] where descid >=113000 AND descid < 113100
	
	-- Description de la table
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [type]) select 113000, 'FORMULARXRM', 'FORMULARXRM', 0, 0, 18
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113000, 'Formulaire XRM', 'XRM Formular', 'XRM Formular', 'XRM Formular', 'XRM Formular'

	-- Nom du formulaire
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113001, 'FORMULARXRM', 'Label', 1, 255
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) 	select 113001, 'Nom du formulaire', 'Label', 'Label', 'Label', 'Label'

	-- Date d'expiration du formulaire
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113002, 'FORMULARXRM', 'ExpirationDate', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113002, 'Date limite','Expiration date','Expiration date','Expiration date','Expiration date'

	-- Type de soumission (unique 0/1)
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113003, 'FORMULARXRM', 'IsUniqueSubmit', 3, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113003, 'Soumission unique', 'Unique submission', 'Unique submission', 'Unique submission', 'Unique submission'
	
	-- Les permissions de visus/modifs
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113004, 'FORMULARXRM', 'ViewPermId', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113004, 'Permission de visualisation','View permission','View permission','View permission','View permission'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113005, 'FORMULARXRM', 'UpdatePermId', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113005, 'Permission de modification','Update permission','Update permission','Update permission','Update permission'

	-- Descid ++/Cible Etendu
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113006, 'FORMULARXRM', 'Tab', 10, 18
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113006, 'Table', 'Tab', 'Tab', 'Tab', 'Tab'	

	-- Corps de formulaire
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select 113007, 'FORMULARXRM', 'Body', 9, 8000, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113007, 'Corps du formulaire', 'Formular body', 'Formular body', 'Formular body', 'Formular body'

	-- Corps de formulaire
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select 113008, 'FORMULARXRM', 'BodyCss', 9, 8000, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113008, 'Feuille de style du formulaire', 'Formular stylesheet', 'Formular stylesheet', 'Formular stylesheet', 'Formular stylesheet'
		
	-- Corps de message de confirmation de la soumission
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select 113009, 'FORMULARXRM', 'SubmissionBody', 9, 8000, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113009, 'Corps de la page de confirmation après soumission', 'Submission confirmation page body', 'Submission confirmation page body', 'Submission confirmation page body', 'Submission confirmation page body'

	-- Feuille de message de confirmation de la soumission
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [html]) select 113010, 'FORMULARXRM', 'SubmissionBodyCss', 9, 8000, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113010, 'Feuille de style de la page de confirmation après soumission', 'Submission confirmation page stylesheet', 'Submission confirmation page stylesheet', 'Submission confirmation page stylesheet', 'Submission confirmation page stylesheet'
	
	-- Redirection de l'URL
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113011, 'FORMULARXRM', 'SubmissionRedirectUrl', 1, 300
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113011, 'URL de redirection après soumission', 'Redirect URL on submission', 'Redirect URL on submission', 'Redirect URL on submission', 'Redirect URL on submission'

	-- Messages :  "formulaire déjà soumis" et "la date d'expiration est dépassée"
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113012, 'FORMULARXRM', 'LabelExpired', 1, 600
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113012, 'Message à afficher si le formulaire a expiré', 'Message displayed on form expiration', 'Message displayed on form expiration', 'Message displayed on form expiration', 'Message displayed on form expiration'
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113013, 'FORMULARXRM', 'LabelAlreadySubmitted', 1, 600
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113013, 'Message à afficher si le formulaire a déjà été soumis', 'Message displayed when form has been already submitted', 'Message displayed when form has been already submitted', 'Message displayed when form has been already submitted', 'Message displayed when form has been already submitted'
	
	-- CHAMPS SYSTEMES
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113095, 'FORMULARXRM', 'CreatedOn', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113095, 'Crée le','Created on','Created on','Created on','Created on'

	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113096, 'FORMULARXRM', 'ModifiedOn', 2, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113096, 'Modifié le','Modified on','Modified on','Modified on','Modified on'
	
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113097, 'FORMULARXRM', 'CreatedBy', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113097, 'Crée par','Created by','Created by','Created by','Created by'

	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113098, 'FORMULARXRM', 'ModifiedBy', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113098, 'Modifié par','Modified by','Modified by','Modified by','Modified by'
		
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select 113099, 'FORMULARXRM', 'UserId', 8, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113099, 'Appartient à','Owned by','Owned by','Owned by','Owned by'

	--Pref/selections
	delete from pref where tab=113000
	delete from selections where tab=113000
	
	insert into pref (tab,userid, listcol) select 113000, 0,'113001;113096;113095'
	insert into pref (tab,userid, listcol) select  113000, userid, '113001;113096;113095' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select 113000,'Vue par défaut', '113001;113096;113095','0;0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select 113000,'Vue par défaut', '113001;113096;113095','0;0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = 113000
END


