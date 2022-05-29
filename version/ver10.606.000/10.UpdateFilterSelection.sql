-- Ajout de la colonne 'STATUS' dans la liste des formulaires avancés
UPDATE [selections] SET listcol='113001;113025;113099;113096', ListColWidth=null, ListSort=null,ListOrder=null 
where tab = 113000 and listcol NOT LIKE '%113025%'
