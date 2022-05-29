
/*la rubrique "onglet" de la table pj passe en catalogue de type DESC*/
update [DESC] set Popup = 5 where DescId = 102011

/*la rubrique "Emplacement" (Type : Serveur Local, fichier, url, etc) de la table pj passe en catalogue de type ENUM*/
update [DESC] set Popup = 6 where DescId = 102009

/*le champ date d'expiration passe en champ date*/
update [DESC] set Format = 2 where DescId = 102013



