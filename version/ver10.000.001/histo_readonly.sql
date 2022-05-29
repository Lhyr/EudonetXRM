
/*Demande #22809 : [Mode Fiche] - Historique - Actuellement on peut modifier les champs systèmes et à l'inverses les champs custom ne le sont jamais.*/
update [DESC] set [DESC].[ReadOnly] = 1 from [DESC] where [DESC].[DescId] between 100001 and 100011