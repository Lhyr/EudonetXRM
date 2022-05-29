const FormularType = {
    /// <summary>Formulaire simple</summary>
    ClassicFormular: 0,
    /// <summary>Formulaire avancé</summary>
    AdvancedFormular: 1
};


const FormularState = {
    /// <summary>brouillon</summary>
    NotPublished: 0,
    /// <summary>On valide le formulaire avant de sauvegarder</summary>
    Published: 1
};

const AcknowledgmentSelect = {
    /// <summary>URL de redirection</summary>
    ThankingMessage: "1",
    /// <summary>On valide le formulaire avant de sauvegarder</summary>
    URLRedirection: "2"
};


export { FormularType, FormularState, AcknowledgmentSelect };
