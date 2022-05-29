const efakeDialogAdmin = {
    content: [
        { name: 'header', type: 'layout', content: 'Configurer les zones résumés et assistants' },
        {
            name: 'summaryFields',
            title: 'Zone résumé',
            type: 'fields',
            divider: false,
            content: [
                {
                    label: 'Titre',
                    value: { text: 'Contact.Prénom', val: 202 },
                    col: 12,
                    values: [
                        { text: 'Contact.Nom', val: 201 },
                        { text: 'Contact.Prénom', val: 202 },
                        { text: 'Contact.Téléphone', val: 206 }
                    ]
                },
                {
                    label: 'Sous-titre',
                    value: { text: 'Contact.Prénom', val: 202 },
                    col: 12,
                    values: [
                        { text: 'Contact.Nom', val: 201 },
                        { text: 'Contact.Prénom', val: 202 },
                        { text: 'Contact.Téléphone', val: 206 }
                    ]
                },
                {
                    label: 'Avatar',
                    value: { text: 'Contact.Image', val: 215 },
                    col: 12,
                    values: [
                        { text: 'Contact.Image', val: 215 },
                        { text: 'Contact.Avatar', val: 214 }
                    ]
                }
            ]
        },
        {
            name: 'additionnalFields',
            title: 'Rubriques Complémentaires',
            type: 'fields',
            divider: true,
            content: [
                {
                    label: 'Titre',
                    value: { text: 'Contact.Prénom', val: 202 },
                    col: 12,
                    values: [
                        { text: 'Contact.Nom', val: 201 },
                        { text: 'Contact.Prénom', val: 202 },
                        { text: 'Contact.Téléphone', val: 206 }
                    ]
                },
                {
                    label: 'Sous-titre',
                    value: { text: 'Contact.Prénom', val: 202 },
                    col: 12,
                    values: [
                        { text: 'Contact.Nom', val: 201 },
                        { text: 'Contact.Prénom', val: 202 },
                        { text: 'Contact.Téléphone', val: 206 }
                    ]
                },
                {
                    label: 'Avatar',
                    value: { text: 'Contact.Image', val: 215 },
                    col: 12,
                    values: [
                        { text: 'Contact.Image', val: 215 },
                        { text: 'Contact.Avatar', val: 214 }
                    ]
                }
            ]
        },
        {
            name: 'stepBarFields',
            title: 'Barre d\'\étapes',
            type: 'fields',
            divider: true,
            content: [
                {
                    label: 'Titre',
                    value: { text: 'Contact.Prénom', val: 202 },
                    col: 12,
                    values: [
                        { text: 'Contact.Nom', val: 201 },
                        { text: 'Contact.Prénom', val: 202 },
                        { text: 'Contact.Téléphone', val: 206 }
                    ]
                },
            ]
        },
        {
            name: 'footer',
            type: 'layout',
            content: [
                {
                    title: 'Fermer',
                    action: 'close',
                    color: 'grey darken-1'
                },
                {
                    title: 'Enregistrer',
                    action: 'save',
                    color: 'green darken-1'
                }
            ]
        }
    ]
}

export { efakeDialogAdmin };