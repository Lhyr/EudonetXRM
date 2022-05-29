-- Pour pouvoir afficher les champs dans l'admin
update [DESC] set disporder = descid % 100 where descid > 102000 and descid < 102070