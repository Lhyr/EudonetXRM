var eSpec = {};


///Passe en mode fiche
eSpec.LoadFile = function (nTab, nFileId) {

    top.loadFile(nTab, nFileId);
}




/// <summary>
/// Dans le cas de l'ouverture dans une fenêtre modal, adapte la taille de la fenêtre au contenu
/// </summary>
/// <param name="frmiD">Id de la fenêtre modal. transmide à la spécif en request.form, paramètre 'modalid' </param>
/// <param name="offset">nombre de pixel d'ajustement. La taille calculée en js n'étant pas toujours la bonne</param>
/// <param name="errorCallBack"> fonction de callback eventuel. L'exception JS est transmise</param>
/// <returns></returns>
eSpec.ajustModalToContent = function (frmiD, offset, errorCallBack) {



    try {

        var myMod = top.window['_md']['specif_' + frmiD];
        myMod.adjustModalToContentIframe(offset);


    } catch (e) {
        if (typeof (errorCallBack) == "function")
            errorCallBack(e);
    }
}