using System;
using System.Collections.Generic;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.mgr
{
    /************************************************************************/
    /*   Page contrôleur pour  gestion des fiches marquées                  */
    /* DESCRIPTION  : Page appelé via ajax pour gérer les fiches marquéees  */
    /* CREATION     : SPH LE 09/10/2011                                     */
    /************************************************************************/

    /// <summary>
    /// Classe contrôleur pour  gestion des fiches marquées
    /// Appellé via ajax (eUpdater pour la gestion des fiches marquées)
    /// </summary>
    public class eMarkedFilesManager : eEudoManager
    {

        XmlNode _baseResultNode;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            #region PREPARATION XML RETOUR

            // BASE DU XML DE RETOUR            
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);

            // Content
            XmlNode xmlContentNode = xmlResult.CreateElement("Content");
            _baseResultNode.AppendChild(xmlContentNode);

            //Document MarkedFile        
            XmlNode baseMarkedFileNode;
            baseMarkedFileNode = xmlResult.CreateElement("markedfile");
            xmlContentNode.AppendChild(baseMarkedFileNode);

            #endregion

            #region Variables de session
            // Type d'action sur sélection
            Int32 nType = 0;
            if (!String.IsNullOrEmpty(_context.Request.Form["type"]))
                Int32.TryParse(_context.Request.Form["type"].ToString(), out nType);

            //Id de la sélection
            Int32 nMarkedFileId = 0;
            if (!String.IsNullOrEmpty(_context.Request.Form["markedfileid"]))
                Int32.TryParse(_context.Request.Form["markedfileid"].ToString(), out nMarkedFileId);

            //Id de la fiche
            Int32 nFileId = 0;
            if (!String.IsNullOrEmpty(_context.Request.Form["fileid"]))
                Int32.TryParse(_context.Request.Form["fileid"].ToString(), out nFileId);

            //Label de la sélection
            String sLabelSelection = String.Empty;
            if (!String.IsNullOrEmpty(_context.Request.Form["label"]))
                sLabelSelection = _context.Request.Form["label"].ToString();

            //Flag de suppression
            Boolean bAdd = false;
            if (!String.IsNullOrEmpty(_context.Request.Form["add"]))
                bAdd = (_context.Request.Form["add"].ToString() == "1");

            //Flag de suppression
            Boolean bConfirm = false;
            if (!String.IsNullOrEmpty(_context.Request.Form["confirm"]))
                bConfirm = (_context.Request.Form["confirm"].ToString() == "1");


            // Table de la sélection
            Int32 nTab = _pref.Context.Paging.Tab;

            #endregion

            MarkedFilesSelection currentSelection = null;

            #region Traitement de la demande
            switch (nType)
            {
                case 0:
                    #region Afficher tout
                    _pref.Context.MarkedFiles.TryGetValue(nTab, out currentSelection);
                    if (currentSelection != null)
                    {
                        currentSelection.Enabled = false;
                        nMarkedFileId = currentSelection.Id;
                    }
                    //Set Pref
                    setMarkedFileDisplay(false);

                    break;
                    #endregion

                case 1:
                    #region Afficher seulement la sélection
                    _pref.Context.MarkedFiles.TryGetValue(nTab, out currentSelection);

                    if (currentSelection != null)
                    {

                        currentSelection.Enabled = true;
                        nMarkedFileId = currentSelection.Id;

                        //Set Pref
                        setMarkedFileDisplay(true);
                    }
                    else
                    {

                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6548)));  

                    }
                    break;
                    #endregion
                
                case 2:
                    #region "Enregistrer sous" la sélection en cours
                    _pref.Context.MarkedFiles.TryGetValue(nTab, out currentSelection);
                    if (currentSelection == null || String.IsNullOrEmpty(sLabelSelection))
                    {
                        if (currentSelection == null)
                            this.ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6549),
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6550));   
                        else
                            this.ErrorContainer = eErrorContainer.GetUserError(
                                eLibConst.MSG_TYPE.EXCLAMATION,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6551),
                                eResApp.GetRes(_pref, 72));  
                        break;
                    }

                    String errorMsg = string.Empty;
                    //recherche d'une sélection ayant le même nom et différente de la sélection courrante :
                    // dans ce cas, besoin d'une confirmation de suppression
                    MarkedFilesSelection mfNamed = eMarkedFiles.LoadSelectionByName(_pref, sLabelSelection, out errorMsg);

                    if (!String.IsNullOrEmpty(errorMsg))
                    {
                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), errorMsg));
                    }

                    if (mfNamed != null && mfNamed.Id == currentSelection.Id)
                        mfNamed = null;

                    if ((mfNamed == null || bConfirm))
                    {

                        //Supression de la sélection portant le même nom
                        if (bConfirm && mfNamed != null)
                            eMarkedFiles.DeleteSelection(_pref, mfNamed.Id, out errorMsg);
                        if (!String.IsNullOrEmpty(errorMsg))
                        {
                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72), errorMsg));
                        }


                        currentSelection = saveSelection(currentSelection.Id, sLabelSelection);
                        if (currentSelection != null)
                        {
                            _pref.Context.MarkedFiles.AddOrUpdateValue(currentSelection.Tab, currentSelection, true);
                            nMarkedFileId = currentSelection.Id;
                        }

                    }
                    else if (currentSelection != null && !String.IsNullOrEmpty(sLabelSelection))
                    {
                        // Une sélection existe avec ce nom, confirmation nécessaire
                        XmlNode _blocConfirmSaveAs = xmlResult.CreateElement("needconfirm");
                        _blocConfirmSaveAs.AppendChild(xmlResult.CreateTextNode("1"));
                        baseMarkedFileNode.AppendChild(_blocConfirmSaveAs);
                    }
                    else
                    {
                        if (!this.ErrorContainer.IsSet)
                        {
                            this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                            this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                        }
                        this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6548);
                        this.ErrorContainer.IsSet = true;
                    }
                    break;

                    #endregion
              
                case 3: 
                    #region Coche/décoche
                    if (nFileId == 0)
                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Concat("Paramètre invalide pour la sélection/désélection d'une fiche."), eResApp.GetRes(_pref, 72), string.Concat("Pas d'identifiant de fiches à sélectionner.")));


                    currentSelection = modSelection(nFileId, bAdd);
                    if (currentSelection != null)
                    {
                        _pref.Context.MarkedFiles.AddOrUpdateValue(currentSelection.Tab, currentSelection, true);
                        nMarkedFileId = currentSelection.Id;
                      
                        // MOU Reset le pagging 
                        // Si on déselection une ligne, la liste est raffraichie, le compteur est mis a jour avec un appel asynchrone de eCountOnDemande
                        // Il est necessaire de re-initiliser le pagging ici.
                        if(currentSelection.Enabled)
                             setMarkedFileDisplay(true);
                    }
                    else
                    {
                        // Normalement, s'il n'y a pas de sélection en cours, une sélection vide est créée
                        // ce bloc ne *devrait*  donc jamais être atteint
                        if (!this.ErrorContainer.IsSet)
                        {
                            this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                            this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                        }
                        this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6548);  
                        this.ErrorContainer.IsSet = true;
                        break;
                    }

                    break;

                   #endregion
               
                case 4:
                    #region Ajout/supprime la page de la selection de fiches marquées

                    //S'il n'y a pas de fiche marquées, on retourne rien 
                    if (_pref.Context.Paging.LstIdPage.Count == 0)
                        break;
                        //LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref.Lang, 72), string.Concat("Paramètre invalide pour l'ajout/suppression de fiche marquées."), eResApp.GetRes(_pref.Lang, 72), string.Concat("Pas de numéro de page sélectionnée.")));

                    currentSelection = modSelection(_pref.Context.Paging.LstIdPage, bAdd);

                    if (currentSelection != null)
                    {
                        _pref.Context.MarkedFiles.AddOrUpdateValue(currentSelection.Tab, currentSelection, true);
                        nMarkedFileId = currentSelection.Id;


                        XmlNode _CheckAll = xmlResult.CreateElement("checkall");
                        _CheckAll.AppendChild(xmlResult.CreateTextNode("1"));
                        baseMarkedFileNode.AppendChild(_CheckAll);

                    }
                    else
                    {
                        // Normalement, s'il n'y a pas de sélection en cours, une sélection vide est créée
                        // ce bloc ne *devrait*  donc jamais être atteint
                        if (!this.ErrorContainer.IsSet)
                        {
                            this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                            this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                        }
                        this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6552);   
                        this.ErrorContainer.IsSet = true;
                        break;
                    }


                    break;
                    #endregion
               
                case 5: 
                    #region Supprimer la sélection
                    if (DelMarkedFile(nMarkedFileId))
                    {
                        _pref.Context.MarkedFiles.TryGetValue(nTab, out currentSelection);
                        if (currentSelection != null && currentSelection.Id == nMarkedFileId)
                        {
                            _pref.Context.MarkedFiles.Remove(_pref.Context.Paging.Tab);
                            XmlNode _Reload = xmlResult.CreateElement("reload");
                            _Reload.AppendChild(xmlResult.CreateTextNode("1"));
                            baseMarkedFileNode.AppendChild(_Reload);
                        }


                        XmlNode _Deleted = xmlResult.CreateElement("deleted");
                        _Deleted.AppendChild(xmlResult.CreateTextNode(nMarkedFileId.ToString()));
                        baseMarkedFileNode.AppendChild(_Deleted);

                    }


                    break;
                 #endregion

                case 6: 
                    #region Rennomer la sélection
                    if (!RenameMarkedFile(nMarkedFileId, sLabelSelection))
                    {
                        if (!this.ErrorContainer.IsSet)
                        {
                            this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                            this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                        }
                        this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6553);
                        this.ErrorContainer.IsSet = true;
                        break;
                    }
                    break;

                  #endregion

                case 7: 
                    #region Charger la sélection
                    if (nMarkedFileId == 0)
                        LaunchError(eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6554),
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6555)));

                    currentSelection = loadSelection(nMarkedFileId);
                    if (currentSelection != null)
                    {
                        _pref.Context.MarkedFiles.AddOrUpdateValue(currentSelection.Tab, currentSelection, true);

                        _pref.Context.MarkedFiles.TryGetValue(nTab, out currentSelection);


                        currentSelection.Enabled = true;
                        nMarkedFileId = currentSelection.Id;

                        //Set Pref
                        setMarkedFileDisplay(true);


                    }
                    else
                    {
                        if (!this.ErrorContainer.IsSet)
                        {
                            this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                            this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                        }
                        this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6556);   
                        this.ErrorContainer.IsSet = true;
                        break;
                    }
                    break;
                  #endregion
                
                default:
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Concat("Action non reconnue."), eResApp.GetRes(_pref, 72), string.Concat("Action demandée n° : ", nType)));
                    break;
            }

            // Gestion Erreur            
            LaunchError();


            //Création du flux de sortie


            //Table
            XmlNode _blocTab = xmlResult.CreateElement("tab");
            _blocTab.AppendChild(xmlResult.CreateTextNode(nTab.ToString()));
            baseMarkedFileNode.AppendChild(_blocTab);


            //Type
            XmlNode _blocType = xmlResult.CreateElement("actiontype");
            _blocType.AppendChild(xmlResult.CreateTextNode(nType.ToString()));
            baseMarkedFileNode.AppendChild(_blocType);

            XmlNode _blocHasSelect = xmlResult.CreateElement("hasSelect");
            _blocHasSelect.AppendChild(xmlResult.CreateTextNode((currentSelection != null) ? "1" : "0"));
            baseMarkedFileNode.AppendChild(_blocHasSelect);

            //MOU 30/04/2014 Pour savoir si la selection est activée
            XmlNode _bSelectionEnabled = xmlResult.CreateElement("bSelectionEnabled");
            _bSelectionEnabled.AppendChild(xmlResult.CreateTextNode((currentSelection != null && currentSelection.Enabled) ? "1" : "0"));
            baseMarkedFileNode.AppendChild(_bSelectionEnabled);
            
            if (nType == 6) // transmission des informations de renommage
            {
                //nom marked file
                XmlNode _blocMarkedFileName = xmlResult.CreateElement("markedfilename");
                _blocMarkedFileName.AppendChild(xmlResult.CreateTextNode(sLabelSelection));
                baseMarkedFileNode.AppendChild(_blocMarkedFileName);

                //Id marked file
                XmlNode _blocMarkedFileId = xmlResult.CreateElement("markedfileid");
                _blocMarkedFileId.AppendChild(xmlResult.CreateTextNode(nMarkedFileId.ToString()));
                baseMarkedFileNode.AppendChild(_blocMarkedFileId);
            }
            else
            {
                //Propriété de la sélection en cours
                if (currentSelection != null)
                {
                    //nom marked file
                    XmlNode _blocMarkedFileName = xmlResult.CreateElement("markedfilename");
                    _blocMarkedFileName.AppendChild(xmlResult.CreateTextNode(currentSelection.Name));
                    baseMarkedFileNode.AppendChild(_blocMarkedFileName);

                    //Nb de files
                    XmlNode _nbCountMarked = xmlResult.CreateElement("nbmakedfiles");
                    _nbCountMarked.AppendChild(xmlResult.CreateTextNode(currentSelection.NbFiles.ToString()));
                    baseMarkedFileNode.AppendChild(_nbCountMarked);

                    //Id marked file
                    XmlNode _blocMarkedFileId = xmlResult.CreateElement("markedfileid");
                    _blocMarkedFileId.AppendChild(xmlResult.CreateTextNode(currentSelection.Id.ToString()));
                    baseMarkedFileNode.AppendChild(_blocMarkedFileId);
                }
            }


            /*Les erreurs sont maintenant gérées par launcherror donc renvoi toujours Succès.*/
            XmlNode _xmlNodeSuccess = xmlResult.CreateElement("success");
            _xmlNodeSuccess.InnerText = "1";
            _baseResultNode.AppendChild(_xmlNodeSuccess);

            // retour du xml
            //RenderResultXML(_xmlResult);
            RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });

            #endregion
        }


        #region Méthode métiers de gestion - Appel à la classe eMarkedFiles

        /// <summary>
        /// Met à jour les préférences d'affichages des fiches marquées
        /// </summary>
        /// <param name="bOnlyMarked">Vrai : affiche uniquement les fiches marquées</param>
        private void setMarkedFileDisplay(Boolean bOnlyMarked)
        {
            eMarkedFiles eMF = new eMarkedFiles(_pref);
            String sError = String.Empty;
            eMF.setMarkedFileDisplay(bOnlyMarked, true, out sError);
            if (!String.IsNullOrEmpty(sError))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6557);   
                }
                this.ErrorContainer.AppendDebug = sError;
                this.ErrorContainer.IsSet = true;
            }
        }

        /// <summary>
        /// Modifie une sélection
        /// </summary>
        /// <param name="nTab">Table de la sélection de marked files. 0 si nouvelle sélection</param>
        /// <param name="nSelectId">Id de la sélection de marked files</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="add">ajout/suppression</param>
        /// <returns>Id de la sélection de marked files</returns>
        private MarkedFilesSelection modSelection(Int32 nFileId, Boolean add)
        {
            List<Int32> _LstFileId = new List<Int32>();
            _LstFileId.Add(nFileId);
            return modSelection(_LstFileId, add);
        }


        /// <summary>
        /// Modifie une sélection
        /// </summary>
        /// <param name="_lstFileId">Liste des Id des fiches</param>
        /// <param name="add">ajout/suppression</param>
        /// <returns>Id de la sélection de marked files</returns>
        private MarkedFilesSelection modSelection(List<Int32> _lstFileId, Boolean add)
        {
            //Pas de sélectid => création/recherche de la sélection "dernière sélection  non sauvegardée
            eMarkedFiles eMF = new eMarkedFiles(_pref);
            String errorMsg = string.Empty;
            MarkedFilesSelection mfSel = eMF.chgSel(_lstFileId, add, out errorMsg);
            if (!String.IsNullOrEmpty(errorMsg))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                }
                this.ErrorContainer.AppendDetail = String.Concat(eResApp.GetRes(_pref, 6548), ".");   
                this.ErrorContainer.AppendDebug = errorMsg;
                this.ErrorContainer.IsSet = true;
            }
            return mfSel;
        }





        /// <summary>
        /// Charge la sélection passé en param
        /// </summary>
        /// <param name="nMarkedFileId">Id de la sélection à charger</param>
        /// <returns></returns>
        private MarkedFilesSelection loadSelection(Int32 nMarkedFileId)
        {
            String errorMsg = string.Empty;
            MarkedFilesSelection mfSel = eMarkedFiles.LoadSelectionById(_pref, nMarkedFileId, out errorMsg);
            if (!String.IsNullOrEmpty(errorMsg))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6554); 
                }
                this.ErrorContainer.AppendDebug = errorMsg;
                this.ErrorContainer.IsSet = true;
            }
            return mfSel;
        }



        /// <summary>
        /// Sauvegarde la sélection sous le nom "label"
        /// </summary>
        /// <param name="nSelectid">Id de la fiche marquée</param>
        /// <param name="sLabel">Nom de la sélection</param>
        /// <returns></returns>
        private MarkedFilesSelection saveSelection(Int32 nSelectid, String sLabel)
        {
            String errorMsg = string.Empty;
            MarkedFilesSelection mfSel = eMarkedFiles.SaveAsSelection(_pref, nSelectid, sLabel, out errorMsg);
            if (!String.IsNullOrEmpty(errorMsg))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6558);
                }
                this.ErrorContainer.AppendDebug = errorMsg;
                this.ErrorContainer.IsSet = true;
            }
            return mfSel;
        }


        /// <summary>
        /// Supprime la sélection dont l'id est passée en paramètre.
        /// </summary>
        /// <param name="nMarkedFileId">Id de la fiche marquée</param>
        /// <returns></returns>
        private Boolean DelMarkedFile(Int32 nMarkedFileId)
        {
            String errorMsg = string.Empty;
            Boolean bSucces = eMarkedFiles.DeleteSelection(_pref, nMarkedFileId, out errorMsg);
            if (!String.IsNullOrEmpty(errorMsg))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6553);
                }
                this.ErrorContainer.AppendDebug = errorMsg;
                this.ErrorContainer.IsSet = true;
            }
            return bSucces;
        }


        /// <summary>
        /// Renomme la sélection dont l'id est spécifié par le libelle est spécifié en paramètre.
        /// </summary>
        /// <param name="nMarkedFileId">Id de la fiche marquée</param>
        /// <param name="sLabel">Nom de la sélection</param>
        /// <returns></returns>
        private Boolean RenameMarkedFile(Int32 nMarkedFileId, String sLabel)
        {
            String errorMsg = String.Empty;
            eMarkedFiles.RenameSelection(_pref, nMarkedFileId, sLabel, out errorMsg);
            if (!String.IsNullOrEmpty(errorMsg))
            {
                if (!this.ErrorContainer.IsSet)
                {
                    this.ErrorContainer.AppendTitle = eResApp.GetRes(_pref, 72);
                    this.ErrorContainer.AppendMsg = eResApp.GetRes(_pref, 72);
                }
                this.ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6552);
                this.ErrorContainer.AppendDebug = errorMsg;
                this.ErrorContainer.IsSet = true;
            }
            return String.IsNullOrEmpty(errorMsg);
        }



        #endregion
    }
}