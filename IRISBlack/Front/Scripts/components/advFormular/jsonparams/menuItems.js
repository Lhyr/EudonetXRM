export let menuJson = [
    {
        //Paramètres
        ref: "param",
        tabsBarAside: [
            //Paramètres généraux
            {
                ref: "tabGeneralParameters",
                refChildren: "GeneralParameters",
                active: true,
                icon: "fas fa-hands-helping",
                txtHeader: 7179, // Paramètres généraux
                txtSubTitle: 2583, // 'A la validation du formulaire'
                blocks: [
                    {
                        title: 4, // 'Langue'
                        txtSubTitle: 2628, //'Langue du formulaire'
                        ref: "paramLang",
                        refChildren: "ParamLang",
                        active: true,
                    },
                    {
                        //AABBA tache 2 842 création de composant Page de remerciement
                        title: 2605, // 'Page de Remerciements'
                        txtSubTitle: 2622, // 'Choisissez la page à afficher après validation du formulaire'
                        ref: "tabAcknowledgement",
                        refChildren: "Acknowledgement",
                        active: false,
                    },
                ],
            },
            //Planification
            {
                //tache #3 364 creation composant planification
                ref: "tabPlanification",
                refChildren: "Planification",
                active: false,
                icon: "fas fa-calendar-alt",
                txtHeader: 2040, // 'Planification'
                txtSubTitle: 2584, //  'Planifier le formulaire'
                blocks: [
                    {
                        title: 1091, // 'Date de début'
                        txtSubTitle: 2720, // 'Définissez la date et l'heure à partir de laquelle votre formulaire est accessible'
                        ref: "tabStartDate",
                        refChildren: "StartDate",
                        active: false,
                    },
                    {
                        title: 1090, // 'Date de fin'
                        txtSubTitle: 2722, // 'Définissez la date et l'heure à partir de laquelle votre formulaire n'est plus accessible'
                        ref: "tabEndDate",
                        refChildren: "EndDate",
                        active: false,
                    },
                ],
            },
            //Sécurité
            {
                ref: "tabSecurity",
                refChildren: "Security",
                active: false,
                icon: "fas fa-user-shield",
                txtHeader: 206, // Sécurité
                txtSubTitle: 2711, // Sécurité du formulaire
                blocks: [
                    {
                        title: 506, // Droits d'accès
                        txtSubTitle: 2709, // Définissez les droits de visualisation et de modification du formulaire
                        ref: "paramAccessRights",
                        refChildren: "AccessRights",
                        active: true,
                    },
                ],
            },
            {
                ref: "tabPersonnalisation",
                refChildren: "Personnalisation",
                active: false,
                icon: "fas fa-user-shield",
                txtHeader: 207, // Personnalisation
                //txtSubTitle: 2711, // Personnalisation
                blocks: [
                    {
                        title: 2760, // Couleurs par défaut
                        txtSubTitle: 2755, // Définissez les couleurs par défaut du formulaire
                        ref: "paramDefaultColors",
                        refChildren: "DefaultColors",
                        active: true,
                    },
                    {
                        title: 2783, // Couleurs des champs de saisie
                        txtSubTitle: 2785, // Définissez les couleurs des champs de saisie du formulaire
                        ref: "paramFormColors",
                        refChildren: "FormColors",
                        active: true,
                    },
                    {
                        title: 3131, // Couleurs des boutons
                        txtSubTitle: 3132, // Définissez les couleurs des boutons du formulaire
                        ref: "paramButtonColors",
                        refChildren: "ButtonColors",
                        active: true,
                    },
                    {
                        title: 2784, // Police des champs de saisie
                        txtSubTitle: 2786, // Définissez la police des champs de saisie du formulaire
                        ref: "paramFormFont",
                        refChildren: "FormFont",
                        active: false,
                    },
                ],
            }
        ],
    },

    //Publication
    {
        ref: "publi",
        tabsBarAside: [
            //Partage
            {
                ref: "tabFastShare",
                refChildren: "fastShare",
                active: true,
                icon: "fas fa-share-alt",
                txtHeader: 2625, //Publier et partager
                txtSubTitle: 2625, //Publication et partage du formulaire
                blocks: [
                    {
                        title: 2629, //"Publier"
                        txtSubTitle: 2632,// 'Publiez le formulaire en ligne afin de le partager via le lien public, via les réseaux sociaux ou via un emailing'
                        ref: "publish",
                        refChildren: "publish",
                        active: true,
                    },
                    {
                        title: 2626, //"Lien public"
                        txtSubTitle: 2627, //"Donnez accès au formulaire à tous les internautes en partageant ce lien public")
                        ref: "publicLink",
                        refChildren: "publicLink",
                        active: false,
                    },
                    {
                        title: 6906, //Réseaux sociaux
                        txtSubTitle: 2638, //Partagez sur les résaux sociaux
                        ref: "NetworkShare",
                        refChildren: "NetworkShare",
                        active: false,
                    },
                ],
            },

            //Intégration
            {
                ref: "tabIntegrer",
                refChildren: "integrer",
                active: false,
                icon: "fas fa-code",
                txtHeader: 2747, //Intégrer le formulaire
                txtSubTitle: 2748, //Diverses options d'intégration web
                blocks: [
                    {
                        title: 2750, //Formulaire entier
                        txtSubTitle: 2751,// Intégrer le formulaire dans votre site web
                        ref: "paramFullPageForm",
                        refChildren: "FullPageForm",
                        active: true,
                    },
                ],
            },
        ],
    },
];
