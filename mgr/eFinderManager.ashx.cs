using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;    // ajout using MCR e36826
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eFinderManager</className>
    /// <summary>Classe reroutant les différent appels asynchrone</summary>
    /// <authors>GCH</authors>
    /// <date>2012-09-07</date>
    public class eFinderManager : eEudoManager
    {
        /// <summary>Si une erreur se produit elle est recopiée ici</summary>
        private StringBuilder sbError = new StringBuilder();
        /// <summary>Type d'Action demandée</summary>
        private string _sAction = string.Empty;

        /// <summary>Nom de l'objet JS appelant</summary>
        private string _sJsVarName = string.Empty;
        private Int32 _nHeight = 0;
        private Int32 _nWidth = 0;
        /// <summary>Table sur laquelle on recherche</summary>
        Int32 _nTargetTab = 0;
        /// <summary>id de la fiche de départ</summary> 
        Int32 _nFileId = 0;
        /// <summary>Champ catalogue sur la fiche de départ</summary>
        Int32 _nDescid = 0;
        /// <summary>Id de la table appelante détaillée</summary>
        Int32 _nTabFrom = 0;
        /// <summary>Option Recherche étendue cochée</summary>
        eFinderList.SearchMode _currentSearchMode = eFinderList.SearchMode.STANDARD;
        /// <summary>Option Recherche Phonétique cochée</summary>
        Boolean _bPhoneticSearch = false;
        /// <summary>Option Rechercher sur toutes les fiches TABLE cochée</summary>
        Boolean _bAllRecord = false;
        /// <summary>Indique si l'Option Rechercher sur toutes les fiches TABLE est affichée dépend de UserValue</summary>
        Boolean _enabledSearchAll = true;
        /// <summary>depuis un filtre opérateur est dans la liste</summary>
        Boolean _bMulti = false;
        /// <summary>depuis un filtre opérateur est dans la liste : indique les ids de fiches selectionnées</summary>
        List<Int32> _liIds = new List<Int32>();
        /// <summary>Afficher les fiches historiques</summary>
        Boolean _bHisto = false;
        /// <summary>Indique que si on est sur PP le nom doit être ajouté en attribut de la cellule (eNO)</summary>
        Boolean _bNameOnly = false;
        /// <summary>Recherche demandée</summary>
        string _sSearch = string.Empty;
        /// <summary>MRU</summary>
        string _sMRU = string.Empty;
        /// <summary>Id de la frame depuis laquelle le finder est appelé</summary>
        string _sOrigFrameId = string.Empty;
        /// <summary>
        /// Type de champ de liaison
        ///     0 => Champ de liaison classique (ligne sélectionnable avant validation)
        ///     1 => Recherche avancée (cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée)
        ///     2 => MRU (permet de sélectionner au clique)
        /// </summary>
        eFinderList.Mode _fileMode = eFinderList.Mode.ADVANCED_SEARCH;

        /// <summary>Type de recherche</summary>
        eFinderList.SearchType _searchType = eFinderList.SearchType.Search;

        Int32 _nSearchLimit = 0;
        /// <summary>Colonnes demandées à l'affichage (vide = valeurs par défaut)</summary>
        List<Int32> _listCol = null;
        /// <summary>Colonnes obligatoires (vide = valeurs par défaut)</summary>
        List<Int32> _listColSpec = null;
        /// <summary>Liste des infos des champs affichés pour Uservalue (IsFound$|$Parameter$|$Value$|$Label)</summary>
        List<UserValueField> _listDisplayedUserValueField = new List<UserValueField>();

        // demande 36826 : MCR/RMA : recuperation du phone number pour l action CTI
        string _pn = string.Empty;

        eConst.ShFileCallFrom _callFrom = eConst.ShFileCallFrom.CallFromNavBar;
        bool _noLoadFileAfterValid = false;


        /// <summary>Gestion des actions asynchrones du champ de liaison (principalement : recherche MRU, affichage fenetre, recherche depuis fenêtre)</summary>
        protected override void ProcessManager()
        {
            string messageError = eResApp.GetRes(_pref, 6547);
            #region Récupération des infos postées
            Boolean bSearchAllUserDefined = false;

            _sAction = _context.Request.Form["action"];

            if (_requestTools.AllKeys.Contains("jsvarname"))
                _sJsVarName = _context.Request.Form["jsvarname"];
            //TODO, transmettre la taille en search detail
            if (_requestTools.AllKeys.Contains("width"))
                _nWidth = eLibTools.GetNum(_context.Request.Form["width"]);
            if (_requestTools.AllKeys.Contains("height"))
                _nHeight = eLibTools.GetNum(_context.Request.Form["height"]);

            //Table sur laquelle on recherche
            if (_requestTools.AllKeys.Contains("targetTab"))
                _nTargetTab = eLibTools.GetNum(_context.Request.Form["targetTab"]);

            //id de la fiche de départ
            if (_requestTools.AllKeys.Contains("FileId"))
                _nFileId = eLibTools.GetNum(_context.Request.Form["FileId"]);

            //Champ catalogue sur la fiche de départ
            if (_requestTools.AllKeys.Contains("targetfield"))
            {
                string strTargetField = HttpUtility.UrlDecode(_context.Request.Form["targetfield"]);
                //Champ de départ
                _nDescid = eLibTools.GetNum(strTargetField);
            }
            //Table de départ
            if (_requestTools.AllKeys.Contains("tabfrom"))
                _nTabFrom = eLibTools.GetNum(_context.Request.Form["tabfrom"]);
            //Recherche
            if (_requestTools.AllKeys.Contains("Search"))
                _sSearch = HttpUtility.UrlDecode(_context.Request.Form["Search"]);
            //Recherche étendue
            if (_requestTools.AllKeys.Contains("SearchMode"))
                Enum.TryParse<eFinderList.SearchMode>(_context.Request.Form["SearchMode"], out _currentSearchMode);
            //Recherche phonétique
            if (_requestTools.AllKeys.Contains("PhoneticSearch"))
                _bPhoneticSearch = eLibTools.GetNum(_context.Request.Form["PhoneticSearch"]) == 1;
            //Recherche sur toutes les fiches <TABLE> coché ?
            if (_requestTools.AllKeys.Contains("AllRecord"))
            {
                _bAllRecord = eLibTools.GetNum(_context.Request.Form["AllRecord"]) == 1;
                bSearchAllUserDefined = true;
            }

            //Recherche sur toutes les fiches <TABLE> activé (si pas définit il est par défaut à true)
            if (_requestTools.AllKeys.Contains("EnabledSearchAll"))
                _enabledSearchAll = eLibTools.GetNum(_context.Request.Form["EnabledSearchAll"]) == 1;

            //Afficher l'historique ?
            if (_requestTools.AllKeys.Contains("histo"))
                _bHisto = eLibTools.GetNum(_context.Request.Form["histo"]) == 1;

            //Multiple ?
            if (_requestTools.AllKeys.Contains("Multi"))
                _bMulti = eLibTools.GetNum(_context.Request.Form["Multi"]) == 1;
            //si choix multiples : ids sélectionnés
            if (_requestTools.AllKeys.Contains("sids"))
                _liIds = _context.Request.Form["sids"].ToString().ConvertToListInt(";");

            _fileMode = (eFinderList.Mode)(_requestTools.GetRequestFormKeyI("eMode") ?? 0);

            _searchType = (eFinderList.SearchType)(_requestTools.GetRequestFormKeyI("SearchType") ?? 0);

            if (_requestTools.AllKeys.Contains("SearchLimit"))
                _nSearchLimit = eLibTools.GetNum(_context.Request.Form["SearchLimit"]);
            //Récupération des MRU
            if (_requestTools.AllKeys.Contains("MRU"))
                _sMRU = HttpUtility.UrlDecode(_context.Request.Form["MRU"]);
            //Récupération des MRU
            if (_requestTools.AllKeys.Contains("NameOnly"))
                _bNameOnly = eLibTools.GetNum(_context.Request.Form["NameOnly"]) == 1;

            //Récupération de la frame d'origine dans le cas ou la fenêtre a été appelée depuis une fiche en popup
            if (_requestTools.AllKeys.Contains("origframeid"))
                _sOrigFrameId = _context.Request.Form["origframeid"];

            _noLoadFileAfterValid = _requestTools.GetRequestFormKeyB("noloadfile") ?? false;
            _callFrom = (eConst.ShFileCallFrom)(_requestTools.GetRequestFormKeyI("callfrom") ?? 0);

            //Table ciblé est un champ de liaison haute non PP ni PM
            Boolean bEventInFile = (_nDescid == _nTargetTab && _nTargetTab != 200 && _nTargetTab != 300);
            List<int> mruIds = new List<int>(eLibTools.GetMruIdsFromParam(_sMRU));

            if (_requestTools.AllKeys.Contains("listCol"))
                _listCol = HttpUtility.UrlDecode(_context.Request.Form["listCol"]).ConvertToListInt(";");
            if (_requestTools.AllKeys.Contains("listColSpec"))
                _listColSpec = HttpUtility.UrlDecode(_context.Request.Form["listColSpec"]).ConvertToListInt(";");

            //USERVALUE
            _listDisplayedUserValueField.Clear();
            if (_requestTools.AllKeys.Contains("UserValueFieldList"))
            {
                string UserValueFieldList = HttpUtility.UrlDecode(_context.Request.Form["UserValueFieldList"]);
                if (!string.IsNullOrEmpty(UserValueFieldList))
                {
                    UserValueField uvField;
                    foreach (string currentField in UserValueFieldList.Split(EudoQuery.SEPARATOR.LVL1))
                    {
                        uvField = new UserValueField(currentField);
                        if (!_listDisplayedUserValueField.Exists(u => u.Parameter == uvField.Parameter))
                        {
                            _listDisplayedUserValueField.Add(uvField);
                        }

                    }
                }


            }

            // demande 36826 : MCR/RMA : recuperation du phone number pour l action CTI            
            if (_requestTools.AllKeys.Contains("pn"))
                _pn = _context.Request.Form["pn"];
            #endregion
            switch (_sAction)
            {
                #region Recherche CTI
                case "cti":
                    // demande 36826 : MCR/RMA : _pn : recuperation du phone number pour l action CTI 
                    // rechercher les n° de téléphone correspondant à l'argument passé
                    string errorCti = string.Empty;
                    eFinderListRendererCti finderRendCti = null;
                    try
                    {
                        #region Init de la liste CTI
                        _listCol = new List<int> { 201, 301 };
                        finderRendCti = new eFinderListRendererCti(_pref, _nHeight, _nWidth, _nTargetTab, 0, _nFileId, _sSearch, _bHisto, _currentSearchMode, _bPhoneticSearch, _bAllRecord, _nDescid, mruIds, _listDisplayedUserValueField, _listCol, _listColSpec, _fileMode, _pn);

                        if (!_enabledSearchAll)
                            finderRendCti.EnabledSearchAll = _enabledSearchAll;

                        finderRendCti.AddNameOnly = _bNameOnly;
                        if (finderRendCti.ErrorMsg.Length > 0)
                            throw (new Exception(finderRendCti.ErrorMsg));
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                        sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref.LangId, 72),   // Message En-tête : Une erreur est survenue
                           string.Concat(eResApp.GetRes(_pref.LangId, 422), "<br>", eResApp.GetRes(_pref.LangId, 544)),  //  Détail : pour améliorer...
                           eResApp.GetRes(_pref.LangId, 72),  //   titre
                           string.Concat(sDevMsg));

                        LaunchError();
                    }

                    //--------------------------- affichage de l entete avec la methode GetFinderTop() et construction de l iFrame

                    Panel mainDivCti = new Panel();
                    mainDivCti.ID = "mainDiv";
                    mainDivCti.CssClass = "window_iframe";

                    if ((finderRendCti != null) && (errorCti.Length == 0))
                    {
                        mainDivCti.Attributes.Add("edntype", "lnkfile");
                        /*PARTIE DU HAUT*/
                        foreach (eUlCtrl UL in finderRendCti.GetFinderCtiTop())
                            mainDivCti.Controls.Add(UL);                                 // entete a ajouter dans la iFrame
                        #region LIST
                        //Sauvegarde des infos réutilisé par la fenêtre en cours
                        mainDivCti.Attributes.Add("tab", _nTargetTab.ToString());
                        mainDivCti.Attributes.Add("tabfrom", _nTabFrom.ToString());
                        mainDivCti.Attributes.Add("did", _nDescid.ToString());
                        mainDivCti.Attributes.Add("fid", _nFileId.ToString());
                        mainDivCti.Attributes.Add("eMode", _fileMode.GetHashCode().ToString());
                        mainDivCti.Attributes.Add("SearchType", _searchType.GetHashCode().ToString());
                        mainDivCti.Attributes.Add("SearchLimit", finderRendCti.SearchLimit.ToString());
                        mainDivCti.Attributes.Add("AutobuildName", finderRendCti.AutobuildName ? "1" : "0");
                        mainDivCti.Attributes.Add("MRU", _sMRU);
                        mainDivCti.Attributes.Add("onClick", string.Concat("ocf(event, false, '", _fileMode.GetHashCode(), "');"));  //ocf : evennement, Si action Double click , NeedLoadFile
                        mainDivCti.Attributes.Add("onDblClick", string.Concat("ocf(event, true, '", _fileMode.GetHashCode(), "');"));

                        mainDivCti.Attributes.Add("uvflst", eLibTools.Join<UserValueField>(EudoQuery.SEPARATOR.LVL1, _listDisplayedUserValueField));
                        if (!String.IsNullOrEmpty(_sOrigFrameId))
                            mainDivCti.Attributes.Add("ofid", _sOrigFrameId); // id de la frame depuis laquelle le finder est lancé
                        if (finderRendCti.ListCol != null)
                            mainDivCti.Attributes.Add("listCol", eLibTools.Join<Int32>(";", finderRendCti.ListCol));
                        if (finderRendCti.ListColSpec != null)
                            mainDivCti.Attributes.Add("listColSpec", eLibTools.Join<Int32>(";", finderRendCti.ListColSpec));
                        /*LIST*/
                        mainDivCti.Controls.Add(finderRendCti.GetFinderList(out errorCti));
                        #endregion
                    }

                    if (errorCti.Length > 0)
                    {
                        HtmlInputHidden tbError = new HtmlInputHidden();
                        mainDivCti.Controls.Add(tbError);
                        tbError.ID = "tbError";
                        tbError.Value = errorCti;
                        tbError = null;

                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref.LangId, 72),
                            messageError,
                            eResApp.GetRes(_pref.LangId, 72),
                            errorCti));
                    }
                    finderRendCti = null;
                    AddHeadAndBody = true;
                    RenderResultHTML(mainDivCti);               // la iFrame pour le CTI renderer renvoye au navigateur

                    break;

                #endregion
                #region Fenetre principale
                case "dialog":
                    string error = string.Empty;

                    eFinderListRenderer finderRend = null;

                    eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();

                    //Uservalue de filtre et d'option recherchersur toutes les valeurs
                    try
                    {
                        // Au premier lancement, on supprime les anciens filtre express
                        eColsPref finderPref = new eColsPref(_pref, _nTargetTab, ColPrefType.FINDERPREF);
                        finderPref.SetColsPref(
                            new SetParam<eLibConst.PREF_COLSPREF>[] { new SetParam<eLibConst.PREF_COLSPREF>(eLibConst.PREF_COLSPREF.FilterOptions, null) });

                        // Recherche étendue
                        // #62 380 - et/ou étendue sur toutes les rubriques
                        // la recherche sur toutes les rubriques n'est, pour l'instant, jamais activée par défaut pour des questions de performances
                        // S'il est décidé, un jour, de mémoriser cette option, récupérer la valeur mémorisée ici
                        bool bExtended = _pref.GetConfig(eLibConst.PREF_CONFIG.SEARCHEXTENDED).Equals("1");
                        bool bExtendedAllFields = false;

                        _currentSearchMode = eFinderList.SearchMode.STANDARD;
                        if (bExtended)
                        {
                            if (bExtendedAllFields)
                                _currentSearchMode = eFinderList.SearchMode.EXTENDED_ALLFIELDS;
                            else
                                _currentSearchMode = eFinderList.SearchMode.EXTENDED;
                        }

                        Int32 nUservalueTab, nUservalueField;

                        //Uservalue filtres - gestion des uservalues finder
                        if (_nTabFrom != (_nDescid - _nDescid % 100))
                        {
                            nUservalueTab = _nTabFrom;
                            nUservalueField = ((_nDescid - _nDescid % 100) + 1); //dans ce cas,
                        }
                        else
                        {
                            nUservalueTab = _nTargetTab;
                            nUservalueField = _nDescid;
                        }

                        eUserValue uvFilter = new eUserValue(dal, nUservalueField, TypeUserValue.SEARCH_CUSTOM_SQL_XRM, _pref.User, nUservalueTab);
                        uvFilter.Build();

                        // La case "chercher sur toutes les valeurs doit être décochée s'il y a un uservalue activé
                        // sauf si le uservalue n'est pas obligatoire et que l'utilisateur a explicitement coché la case
                        //   A ce stade, cette option n'a pu être modifié que pas la récupération d'un choix utilisateur.
                        // on ne modifice cette valeur donc que si elle n'a pas été modifié par le user (affichage initial : bSearchAllUserDefined=false) ou que le uservaluer est obligatoire (index=1) 
                        if (uvFilter.Enabled && (!bSearchAllUserDefined || uvFilter.Index == 1))
                            _bAllRecord = !uvFilter.Enabled;

                        // S'il n'y a pas de uservalue de filtre obligatoire, recherche des parents
                        if (_nTabFrom > 0 && _nFileId > 0 && !uvFilter.Enabled)
                        {
                            eFileHeader prtFileHd = eFileHeader.CreateFileHeader(_pref, _nTabFrom, _nFileId);
                            if ((!prtFileHd.ViewMainTable.InterPM || !prtFileHd.ParentFileId.HasParentLnk(TableType.PM))
                                && (!prtFileHd.ViewMainTable.InterPP || !prtFileHd.ParentFileId.HasParentLnk(TableType.PP)))
                            {
                                _bAllRecord = true;
                                _enabledSearchAll = false;
                            }
                        }

                        //UserValue - Chercher sur toutes les valeurs
                        // eUserValue uvSearchAll = new eUserValue(dal, _nDescid, TypeUserValue.SEARCH_ALL, _pref.User, _nTargetTab);
                        eUserValue uvSearchAll = new eUserValue(dal, nUservalueField, TypeUserValue.SEARCH_ALL, _pref.User, nUservalueTab);
                        uvSearchAll.Build();

                        // Si un uservalue force la recherche sur toutes les fiches
                        //on passe la variable à true si c'est le lancement initial( bSearchAllUserDefined= false ou si ce uservalue est obligatoire)
                        if (uvSearchAll.Enabled && (!bSearchAllUserDefined || uvSearchAll.Index == 1))
                            _bAllRecord = uvSearchAll.Enabled;

                        _enabledSearchAll = (uvSearchAll.Index == 0) ? true : false;

                        //Initialisation de l'objet rendu
                        // #33286 => on passe _sSearch
                        finderRend = new eFinderListRenderer(_pref, _nHeight, _nWidth, _nTargetTab, _nTabFrom, _nFileId, _sSearch, _bHisto, _currentSearchMode, _bPhoneticSearch, _bAllRecord, _nDescid, mruIds, _listDisplayedUserValueField, _listCol, _listColSpec, _fileMode, _bMulti);
                        if (!_enabledSearchAll)
                            finderRend.EnabledSearchAll = _enabledSearchAll;

                        if (_noLoadFileAfterValid)
                            finderRend.NoLoadFileAfterValidation = _noLoadFileAfterValid;

                        finderRend.CallFrom = _callFrom;


                        finderRend.AddNameOnly = _bNameOnly;
                        if (finderRend.ErrorMsg.Length > 0)
                            throw (new Exception(finderRend.ErrorMsg));
                    }
                    catch (Exception ex)
                    {
                        string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                        sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                           string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                           eResApp.GetRes(_pref, 72),  //   titre
                           string.Concat(sDevMsg));

                        LaunchError();
                    }
                    finally
                    {
                        dal.CloseDatabase();
                    }

                    Panel mainDiv = new Panel();
                    mainDiv.ID = "mainDiv";
                    // ELAIZ - ajout de la classe CSS add-file pour cibler l'iframe via le CSS. Vu avec Amna
                    mainDiv.CssClass = "window_iframe add-file";

                    if ((finderRend != null) && (error.Length == 0))
                    {
                        mainDiv.Attributes.Add("edntype", "lnkfile");
                        /*PARTIE DU HAUT*/
                        foreach (eUlCtrl UL in finderRend.GetFinderTop())
                            mainDiv.Controls.Add(UL);
                        #region LIST
                        //Sauvegarde des infos réutilisé par la fenêtre en cours
                        mainDiv.Attributes.Add("tab", _nTargetTab.ToString());
                        mainDiv.Attributes.Add("tabfrom", _nTabFrom.ToString());
                        mainDiv.Attributes.Add("did", _nDescid.ToString());
                        mainDiv.Attributes.Add("fid", _nFileId.ToString());
                        mainDiv.Attributes.Add("eMode", _fileMode.GetHashCode().ToString());
                        mainDiv.Attributes.Add("SearchType", _searchType.GetHashCode().ToString());
                        mainDiv.Attributes.Add("SearchLimit", finderRend.SearchLimit.ToString());
                        mainDiv.Attributes.Add("AutobuildName", finderRend.AutobuildName ? "1" : "0");
                        mainDiv.Attributes.Add("MRU", _sMRU);
                        if (_bMulti)
                            mainDiv.Attributes.Add("multi", "1");
                        mainDiv.Attributes.Add("onClick", string.Concat("ocf(event, false, '", _fileMode.GetHashCode(), "');"));  //ocf : evennement, Si action Double click , NeedLoadFile
                        mainDiv.Attributes.Add("onDblClick", string.Concat("ocf(event, true, '", _fileMode.GetHashCode(), "');"));

                        mainDiv.Attributes.Add("uvflst", eLibTools.Join<UserValueField>(EudoQuery.SEPARATOR.LVL1, _listDisplayedUserValueField));
                        if (!String.IsNullOrEmpty(_sOrigFrameId))
                            mainDiv.Attributes.Add("ofid", _sOrigFrameId); // id de la frame depuis laquelle le finder est lancé
                        if (finderRend.ListCol != null)
                            mainDiv.Attributes.Add("listCol", eLibTools.Join<Int32>(";", finderRend.ListCol));
                        if (finderRend.ListColSpec != null)
                            mainDiv.Attributes.Add("listColSpec", eLibTools.Join<Int32>(";", finderRend.ListColSpec));

                        //Création d'un input contenant les id des liaisons parentes
                        HtmlInputHidden linkedId = new HtmlInputHidden();
                        linkedId.ID = String.Concat("lnkid_finder");
                        linkedId.Value = finderRend.ParentFileId.GetLnkIdInfos();
                        mainDiv.Controls.Add(linkedId);

                        //Création d'un input contenant les id des liaisons parentes
                        HtmlInputHidden newDefValues = new HtmlInputHidden();
                        newDefValues.ID = "defValues";
                        newDefValues.Value = _requestTools.GetRequestFormKeyS("defvalues");
                        mainDiv.Controls.Add(newDefValues);


                        /*LIST*/
                        Panel pnListContent = finderRend.GetFinderList(out error);
                        mainDiv.Controls.Add(pnListContent);

                        if (_bMulti)
                        {
                            eListMainRenderer selectedRdr = (eListMainRenderer)eRendererFactory.CreateFinderSelectionRenderer(_pref, _nTargetTab, eLibTools.Join<Int32>(";", finderRend.ListCol.Count > 0 ? finderRend.ListCol : finderRend.ListColSpec), _liIds);
                            mainDiv.Controls.Add(selectedRdr.PgContainer);
                            TableRow trFinder = finderRend.MainHtmlTable.Rows[0];
                            TableRow trFinderSel = selectedRdr.MainHtmlTable.Rows[0];

                            for (int i = 0; i < trFinder.Cells.Count; i++)
                            {
                                if (i >= trFinderSel.Cells.Count)
                                    break;

                                trFinderSel.Cells[i].Width = trFinder.Cells[i].Width;
                                trFinderSel.Cells[i].Style.Value = trFinder.Cells[i].Style.Value;
                            }

                        }


                        #endregion
                    }

                    if (error.Length > 0)
                    {
                        HtmlInputHidden tbError = new HtmlInputHidden();
                        mainDiv.Controls.Add(tbError);
                        tbError.ID = "tbError";
                        tbError.Value = error;
                        tbError = null;

                        LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),
                            messageError,
                            eResApp.GetRes(_pref, 72),
                            error));
                    }
                    finderRend = null;
                    AddHeadAndBody = true;
                    RenderResultHTML(mainDiv);
                    break;
                #endregion
                #region Recherche depuis MRUx
                case "search":
                    {
                        Boolean bAddAllowed = false;
                        string SearchLimit = string.Empty;
                        Dictionary<Int32, string> values = new Dictionary<int, string>();
                        Dictionary<Int32, string> linkedValues = new Dictionary<int, string>();
                        Dictionary<Int32, string> linkedLabels = new Dictionary<int, string>();

                        //Si recherche on indique pas les MRU
                        if (!string.IsNullOrEmpty(_sSearch))
                            mruIds.Clear();

                        try
                        {
                            eudoDAL edal = eLibTools.GetEudoDAL(_pref);
                            edal.OpenDatabase();
                            TableLite targetTab = null;
                            try
                            {
                                targetTab = new TableLite(_nTargetTab);
                                targetTab.ExternalLoadInfo(edal, out error);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                edal.CloseDatabase();
                            }

                            if (_listCol == null)
                                _listCol = new List<Int32>();

                            if (_nTargetTab == 200) //Dans les MRU le contact doit affiché Particule NOM Prénom
                            {
                                _listCol.Add(201);
                                _listCol.Add(202);
                                _listCol.Add(203);
                                _listCol.Add(301);
                                _listCol.Add(401);
                                _listCol.Add(412);
                            }
                            else if (targetTab != null && (targetTab.TabType == TableType.EVENT || targetTab.TabType == TableType.ADR))
                            {
                                //KHA dans les mrus on doit pouvoir rapatrier les ppid pmid liés
                                if (targetTab.InterPP)
                                    _listCol.Add(TableType.PP.GetHashCode() + 1);

                                if (targetTab.InterPM)
                                    _listCol.Add(TableType.PM.GetHashCode() + 1);
                            }

                            eFinderList _list = eFinderList.CreateFinderList(_pref, _nTargetTab, _nTabFrom, _nFileId, _nDescid,
                                _sSearch, _bHisto, eFinderList.SearchMode.STANDARD, _bPhoneticSearch, _bAllRecord, mruIds, _listDisplayedUserValueField, _listCol, _listColSpec, _fileMode);

                            if (!string.IsNullOrEmpty(_list.ErrorMsg))
                                throw (new Exception(_list.ErrorMsg));

                            //mza #26 792 : Sodie XRM - Roadmap (06) - Pas de prise en compte des user-value à la création d'une fiche
                            if (_list.BUserValue)
                                mruIds.Clear();

                            try
                            {
                                Int32 idxLine = 0;
                                while (idxLine < _list.ListRecords.Count)
                                {
                                    string sDisplayValue = "";
                                    int nPpId = 0;
                                    int nPmId = 0;
                                    int nAdrId = 0;

                                    string sAdr01 = string.Empty;
                                    string sPm01 = string.Empty;

                                    eRecord row = _list.ListRecords[idxLine];
                                    //On ne récupère que le main Field
                                    eFieldRecord fieldRow = row.GetFieldByAlias(string.Concat(_nTargetTab, "_", _nTargetTab + 1));
                                    sDisplayValue = fieldRow.DisplayValue;
                                    if (_nTargetTab == 200) //Dans les MRU le contact doit affiché Particule NOM Prénom
                                    {
                                        sDisplayValue = fieldRow.DisplayValue;

                                        //On récupère ADRID et PMID                                        
                                        nPpId = fieldRow.FileId;
                                        eFieldRecord fldAdr = row.GetFieldByAlias(TableType.PP.GetHashCode() + "_401");
                                        if (fldAdr != null && fldAdr.RightIsVisible)  //Pas de rattachement si pas de droits
                                        {
                                            nAdrId = fldAdr.FileId;
                                            sAdr01 = fldAdr.DisplayValue;
                                        }
                                        eFieldRecord fldPm = row.GetFieldByAlias(TableType.PP.GetHashCode() + "_" + TableType.ADR.GetHashCode() + "_301");
                                        if (fldPm != null && fldPm.RightIsVisible)  //Pas de rattachement si pas de droits
                                        {
                                            nPmId = fldPm.FileId;
                                            sPm01 = fldPm.DisplayValue;
                                        }
                                    }
                                    else if (bEventInFile)   //Dans les MRU on retourne les contacts et sociétés liées en plus
                                    {
                                        eFieldRecord ppFld = row.GetFieldByAlias(string.Concat(_nTargetTab, "_201"));
                                        if (ppFld != null && ppFld.RightIsVisible)  //Pas de rattachement si pas de droits
                                        {
                                            sAdr01 = ppFld.DisplayValue;
                                            nAdrId = ppFld.FileId;
                                        }
                                        eFieldRecord pmFld = row.GetFieldByAlias(string.Concat(_nTargetTab, "_301"));
                                        if (pmFld != null && pmFld.RightIsVisible)  //Pas de rattachement si pas de droits
                                        {
                                            sPm01 = pmFld.DisplayValue;
                                            nPmId = pmFld.FileId;
                                        }
                                    }

                                    if (!values.ContainsKey(fieldRow.FileId))
                                    {
                                        values.Add(fieldRow.FileId, sDisplayValue);
                                        if (_nTargetTab == 200 || bEventInFile)
                                        {
                                            linkedValues.Add(fieldRow.FileId, string.Concat(nAdrId, ";|;", nPmId));
                                            linkedLabels.Add(fieldRow.FileId, string.Concat(sAdr01, ";|;", sPm01));
                                        }
                                    }
                                    else
                                    {
                                        if (_nTargetTab == 200 || bEventInFile)
                                        {
                                            //on attache en prio l'adresse principale
                                            eFieldRecord fldAdrPrinc = row.GetFieldByAlias(TableType.PP.GetHashCode() + "_412");
                                            if (fldAdrPrinc.Value == "1")
                                            {
                                                linkedValues.Remove(fieldRow.FileId);
                                                linkedLabels.Remove(fieldRow.FileId);

                                                linkedValues.Add(fieldRow.FileId, string.Concat(nAdrId, ";|;", nPmId));
                                                linkedLabels.Add(fieldRow.FileId, string.Concat(sAdr01, ";|;", sPm01));
                                            }
                                        }
                                    }

                                    idxLine++;
                                }
                                //Ajout autorisé seulement si Droits de traitement sur nouvelle fiche pour l'auser en cours et que pas un champ de recherche vers une source externe
                                bAddAllowed = ((_list.IsTreatAllowed && (_list.DataSourceId <= 0)));
                                if (_list != null && _list.ListRecords != null)
                                    SearchLimit = _list.SearchLimit.ToString();
                                else
                                    SearchLimit = "0";
                            }
                            catch
                            {
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            //   sbError.AppendLine(messageError).Append(" ").AppendLine(ex.ToString());
                            LaunchError(eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                messageError,
                                eResApp.GetRes(_pref, 72),
                                ex.ToString()));

                        }

                        if (sbError.Length > 0)
                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                messageError,
                                eResApp.GetRes(_pref, 72),
                                sbError.ToString()));
                        XmlDocument xmlResult = new XmlDocument();

                        XmlNode detailsNode = xmlResult.CreateElement("elementlist");
                        #region Droit d'ajout
                        XmlNode elementAddPermissionNode = xmlResult.CreateElement("addpermission");
                        // if element already there, it will override
                        elementAddPermissionNode.InnerText = bAddAllowed ? "1" : "0";
                        detailsNode.AppendChild(elementAddPermissionNode);
                        #endregion
                        #region Nombre de caractère minimum avant recherche
                        XmlNode elementSearchLimitNode = xmlResult.CreateElement("eSearchLimit");
                        elementSearchLimitNode.InnerText = SearchLimit;
                        detailsNode.AppendChild(elementSearchLimitNode);
                        #endregion
                        //Liste des valeurs
                        XmlNode _mainNode = xmlResult.CreateElement("elements");
                        detailsNode.AppendChild(_mainNode);

                        foreach (KeyValuePair<Int32, string> currentValue in values)
                        {
                            XmlNode elementNode = xmlResult.CreateElement("element");

                            XmlNode elementValueNode = xmlResult.CreateElement("value");
                            elementValueNode.InnerText = currentValue.Key.ToString();

                            XmlNode elementLabelNode = xmlResult.CreateElement("label");
                            elementLabelNode.InnerText = currentValue.Value;

                            elementNode.AppendChild(elementValueNode);
                            elementValueNode = null;
                            elementNode.AppendChild(elementLabelNode);
                            elementLabelNode = null;

                            if ((_nTargetTab == 200 || bEventInFile) && linkedValues.ContainsKey(currentValue.Key))
                            {
                                XmlNode adrNode = xmlResult.CreateElement("adrid");
                                adrNode.InnerText = linkedValues[currentValue.Key].Split(";|;")[0];
                                elementNode.AppendChild(adrNode);
                                adrNode = null;

                                XmlNode pmNode = xmlResult.CreateElement("pmid");
                                pmNode.InnerText = linkedValues[currentValue.Key].Split(";|;")[1];
                                elementNode.AppendChild(pmNode);
                                pmNode = null;

                                XmlNode adr01Node = xmlResult.CreateElement("adr01");
                                adr01Node.InnerText = linkedLabels[currentValue.Key].Split(";|;")[0];
                                elementNode.AppendChild(adr01Node);
                                adr01Node = null;

                                XmlNode pm01Node = xmlResult.CreateElement("pm01");
                                pm01Node.InnerText = linkedLabels[currentValue.Key].Split(";|;")[1];
                                elementNode.AppendChild(pm01Node);
                                pm01Node = null;
                            }


                            _mainNode.AppendChild(elementNode);
                            elementNode = null;
                        }

                        //Déclaration de l'objet XML principal
                        xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
                        XmlNode resultNode = xmlResult.CreateElement("return");
                        xmlResult.AppendChild(resultNode);

                        // Erreur ?
                        XmlNode successNode = xmlResult.CreateElement("result");
                        successNode.InnerText = (sbError.Length <= 0) ? "SUCCESS" : "ERROR";
                        resultNode.AppendChild(successNode);
                        successNode = null;

                        // Msg Erreur
                        XmlNode _errDesc = xmlResult.CreateElement("errordescription");
                        _errDesc.InnerText = sbError.ToString();
                        resultNode.AppendChild(_errDesc);
                        _errDesc = null;

                        //Valeurs de recherche à retourner
                        if (detailsNode != null)
                        {
                            resultNode.AppendChild(detailsNode);
                        }
                        resultNode = null;
                        linkedLabels = null;
                        linkedValues = null;
                        values = null;

                        //RETOUR du XML
                        RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    }
                    break;
                #endregion
                #region Recherche depuis MRU mais avec colonne détaillées
                case "searchMRUdetail":
                    {
                        eRenderer mainList = null;

                        //Si recherche on indique pas les MRU
                        if (!string.IsNullOrEmpty(_sSearch))
                            mruIds.Clear();

                        try
                        {
                            eFinderListRenderer findRend = new eFinderListRenderer(_pref, _nHeight, _nWidth, _nTargetTab, _nTabFrom, _nFileId, _sSearch, _bHisto, eFinderList.SearchMode.STANDARD, _bPhoneticSearch, _bAllRecord, _nDescid, mruIds, _listDisplayedUserValueField, _listCol, _listColSpec, _fileMode);

                            try
                            {
                                if (!string.IsNullOrEmpty(findRend.ErrorMsg))
                                    throw (new Exception(findRend.ErrorMsg));

                                //finderRend._list.
                                mainList = eRendererFactory.CreateFinderRenderer(findRend);
                                if (!string.IsNullOrEmpty(findRend.ErrorMsg))
                                    throw (new Exception(findRend.ErrorMsg));
                            }
                            finally
                            {
                                findRend = null;
                            }

                        }
                        catch (Exception ex)
                        {
                            sbError.AppendLine(messageError).AppendLine(ex.ToString());
                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                               messageError,
                                eResApp.GetRes(_pref, 72),
                                sbError.ToString()));
                        }

                        if (mainList.ErrorMsg.Length > 0 || sbError.Length > 0)
                        {
                            sbError.AppendLine(mainList.ErrorMsg).Append(" - ").Append(mainList.ErrorMsg);
                            LaunchError(eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                messageError,
                                eResApp.GetRes(_pref, 72),
                                sbError.ToString()));
                        }
                        //Si pas d'erreur on retourne la liste sinon on retourne une textbox cachée contenant l'erreur
                        if (mainList.ErrorMsg.Length == 0)
                        {
                            Panel mainDivMRU = mainList.PgContainer;
                            mainDivMRU.Attributes.Add("onClick", string.Concat("ocf(event, false, '", _fileMode.GetHashCode(), "','", _sJsVarName, "');"));  //ocf : evennement, Si action Double click , NeedLoadFile
                            mainDivMRU.Attributes.Add("onDblClick", string.Concat("ocf(event, true, '", _fileMode.GetHashCode(), "','", _sJsVarName, "');"));
                            RenderResultHTML(mainDivMRU);
                            mainDivMRU = null;
                            mainList = null;
                        }
                        else
                        {
                            HtmlInputHidden tbErrorUpdate = new HtmlInputHidden();
                            tbErrorUpdate.ID = "tbErrorUpdate";
                            tbErrorUpdate.Value = mainList.ErrorMsg;
                            mainList = null;
                            RenderResultHTML(tbErrorUpdate);
                        }
                    }
                    break;
                #endregion
                #region Recherche depuis LinkDialog
                case "searchdetail":
                    {
                        eRenderer mainList = null;

                        // #47231 : Si "Rechercher sur toutes les fiches" est coché, on devrait normalement afficher toute la liste et pas filtrer sur les MRU
                        //Si recherche on indique pas les MRU
                        if (!string.IsNullOrEmpty(_sSearch) || _bAllRecord)
                            mruIds.Clear();

                        try
                        {
                            #region Sauvegarde de recherche étendu dans les pref
                            ICollection<SetParam<eLibConst.PREF_CONFIG>> param = new List<SetParam<eLibConst.PREF_CONFIG>>();
                            param.Add(new SetParam<eLibConst.PREF_CONFIG>(eLibConst.PREF_CONFIG.SEARCHEXTENDED, _currentSearchMode != eFinderList.SearchMode.STANDARD ? "1" : "0"));
                            _pref.SetConfig(param);
                            param = null;
                            #endregion

                            eFinderListRenderer findRend = new eFinderListRenderer(_pref, _nHeight, _nWidth, _nTargetTab, _nTabFrom, _nFileId, _sSearch, _bHisto, _currentSearchMode, _bPhoneticSearch, _bAllRecord, _nDescid, mruIds, _listDisplayedUserValueField, _listCol, _listColSpec, _fileMode, _bMulti);



                            findRend.AddNameOnly = _bNameOnly;

                            try
                            {

                                if (findRend.InnerException != null)
                                {

                                    if (findRend.InnerException is EudoLinkException)
                                    {

                                        //Message d'erreur spécifique "Une des rubriques affichée/filtrée n'est plus disponible. Merci de vérifier vos rubriques affichées (bouton \"Rubriques\").]]"
                                        messageError = eResApp.GetRes(_pref, 8664);
                                        throw new EudoException(findRend.InnerException.Message, messageError, findRend.InnerException, false);

                                    }

                                    throw findRend.InnerException;
                                }
                                else if (!string.IsNullOrEmpty(findRend.ErrorMsg))
                                {
                                    throw new Exception(findRend.ErrorMsg);
                                }

                                mainList = eRendererFactory.CreateFinderRenderer(findRend);

                            }
                            finally
                            {
                                findRend = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (mainList == null)
                            {
                                //Création d'un rendere d'erreur a partir de l'Exception
                                mainList = eRendererFactory.CreateErrorRenderer(ex.Message, ex);
                            }
                            else
                                sbError.AppendLine(messageError).AppendLine(ex.ToString());
                        }


                        if (mainList.ErrorMsg.Length > 0)
                        {
                            eErrorContainer err;
                            if (mainList.InnerException != null && mainList.InnerException is EudoException)
                            {
                                //EudoException : On utilise l'exception directement
                                err = eErrorContainer.GetErrorContainerFromEudoException(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    (EudoException)mainList.InnerException,
                                    eResApp.GetRes(_pref, 72), "");

                            }
                            else
                            {
                                sbError.Append(messageError).AppendLine(" : ").AppendLine(mainList.ErrorMsg);


                                err = (eErrorContainer.GetDevUserError(
                                        eLibConst.MSG_TYPE.CRITICAL,
                                        eResApp.GetRes(_pref, 72),
                                        messageError,
                                        eResApp.GetRes(_pref, 72),
                                        sbError.ToString()));
                            }

                            LaunchError(err);
                        }
                        //Si pas d'erreur on retourne la liste sinon on retourne une textbox cachée contenant l'erreur
                        else
                        {
                            RenderResultHTML(mainList.PgContainer);
                        }

                    }
                    break;
                #endregion
                default:
                    break;
            }

        }

    }
}