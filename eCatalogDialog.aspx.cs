using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using static Com.Eudonet.Internal.eCatalog;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe qui fabrique un flux HTML pour les catalogues simples, avancées, multiples et affiche le rendu
    /// Elle fait aussi appel aux objets qui effectuent les mises à jour du catalogue
    /// </summary>
    public partial class eCatalogDialog : eEudoPage
    {
        #region propriétés

        /// <summary>Javascript des ressources de l'application</summary>
        protected String _resAppJS = String.Empty;
        private eCatalog _catalog = null;

        private String _catParentValue = String.Empty;
        /// <summary>La valeur recherchée dans la textbox</summary>
        private String _catSearch = String.Empty;
        /// <summary>Valeur(s) de départ(s) (valeur(s) enregistrée(s) en base)</summary>
        private List<String> _catInitialValues = null;
        /// <summary>Valeur(s) à selectionner</summary>
        private List<String> _catSelectedValues = null;
        private String _fieldEditorJsVarName = String.Empty;
        private String _fieldEditorFrameId = String.Empty;

        private StringBuilder _sbInitJSOutput = new StringBuilder();
        private StringBuilder _sbEndJSOutput = new StringBuilder();

        /// <summary>descid du champ catalogue</summary>
        private int _catDescId = 0;
        /// <summary>descid du champ catalogue lié</summary>
        private int _catParentId = -1;
        private PopupType _catBoundPopup = PopupType.NONE;
        private int _catBoundDescid = 0;
        private PopupType _catPopupType = PopupType.NONE;
        /// <summary>Catalogue Multiple ?</summary>
        private bool _catMultiple = false;
        /// <summary>Catalogue Arbo ?</summary>
        private bool _catTreeView = false;
        /// <summary>Titre du catalogue</summary>
        private String _catTitle = String.Empty;
        private String _sort = String.Empty;
        /// <summary>
        /// Largeur de la fenêtre
        /// </summary>
        private String _strWidth = String.Empty;
        /// <summary>
        /// Hauteur de la fenêtre
        /// </summary>
        private String _strHeight = String.Empty;

        /// <summary>
        /// Filtre pour l'affichage
        /// </summary>
        private String _displayFilter = String.Empty;
        /// <summary>
        /// Indique si la fenêtre sert à gérer les modèles de mail
        /// Permet d'afficher certaines fonctionnalités (édition du corps du modèle) et d'en désactiver d'autres (synchro)
        /// </summary>
        private bool _mailTemplateEdit = false;

        private DialogAction _dlgAction = DialogAction.NONE;

        private LOADCATFROM _from = LOADCATFROM.UNDEFINED;

        ///// <summary>Indique si on vient d'un filtre</summary>
        //private Boolean _bFromFilter = false;

        ///// <summary>Indique si on vient d'un traitement de dupplication</summary>
        //private Boolean _bFromTreat = false;

        /// <summary>Indique si on vient de l'admin</summary>
        //private Boolean _bFromAdmin = false;

        /// <summary>Largeur de la fenêtre</summary>
        private int _nWidth = 0;
        /// <summary>Hauteur de la fenêtre</summary>
        private int _nHeight = 0;
        /// <summary>Largeur de la liste de valeurs</summary>
        private int _nListWidth = 0;

        /// <summary>Largeur de la colonne libellé</summary>
        private int _nDisplayColumnWidth = 0;
        /// <summary>Largeur de la colonne libellé (sans masque)</summary>
        private int _nLabelColumnWidth = 0;
        /// <summary>Largeur de la colonne code</summary>
        private int _nDataColumnWidth = 0;
        /// <summary>Largeur de la colonne desactive</summary>
        private int _nDisabledColumnWidth = 0;
        /// <summary>Largeur de la colonne ID</summary>
        private int _nIDColumnWidth = 0;
        /// <summary>Largeur de la colonne des boutons d'action</summary>
        private int _nBtnColumnWidth = 0;
        /// <summary>Largeur total de chaque ligne de valeurs</summary>
        private int _nTotalColumnWidth = 0;

        //private String _sDisplayColumnWidth = String.Empty;
        //private String _sDataColumnWidth = String.Empty;
        //private String _sIDColumnWidth = String.Empty;
        //private String _sTotalColumnWidth = String.Empty;
        //private String _sBtnColumnWidth = String.Empty; //non utilisé ?

        /// <summary>
        /// Recherche à partir de X caractères
        /// </summary>
        private int _searchLimit = 3;
        /// <summary>
        /// Source du catalogue 
        /// </summary>
        private CatSource _catSource = CatSource.Filedata;
        /// <summary>
        /// Dans le cas d'un catalogue DESC, renvoie-t-on une liste de tables ou de rubriques ?
        /// </summary>
        private eCatalogDesc.DescType _descType = eCatalogDesc.DescType.Undefined;
        /// <summary>
        /// Dans le cas d'un catalogue DESC, liste des ids parents
        /// </summary>
        private List<int> _descTypeIds = new List<int>();
        /// <summary>
        /// Dans le cas d'un catalogue ENUM, permet de définir l'Enum source
        /// </summary>
        private eCatalogEnum.EnumType _enumType = eCatalogEnum.EnumType.Undefined;

        #endregion

        #region constantes
        // Calcul (arbitraire) de la taille de la colonne des boutons
        // Les boutons étant définis via des classes CSS (ul/li), impossible d'accéder à la taille des éléments côté serveur
        // On définit donc une taille approximative en considérant que chaque bouton fait X pixels de large, marges comprises
        private const int BtnDefaultWidth = 32;

        // Calcul (arbitraire) de la largeur des padding, margin et border pour chaque ligne de valeurs
        // Permet de conpenser le petit décalage en fin de ligne pour les catalogues multiples
        private const int ValRowPaddingMarginBorderWidth = 7;

        #endregion

        #region accesseurs pour dispo en JS
        /// <summary>Objet métier catalogue</summary>
        public eCatalog Catalog
        {
            get { return _catalog; }
        }
        /// <summary>Compte les occurence</summary>
        public Int32 CountOccurencesOperation
        {
            get { return eCatalog.Operation.CountOccurencesOperation.GetHashCode(); }
        }
        /// <summary>Demande de suppression</summary>
        public Int32 DeleteOperation
        {
            get { return eCatalog.Operation.Delete.GetHashCode(); }
        }
        /// <summary>Demande d'inserion</summary>
        public Int32 InsertOperation
        {
            get { return eCatalog.Operation.Insert.GetHashCode(); }
        }
        /// <summary>Demande de synchronisation</summary>
        public Int32 SynchroOperation
        {
            get { return eCatalog.Operation.Synchro.GetHashCode(); }
        }
        /// <summary>Demande de modification</summary>
        public Int32 ChangeOperation
        {
            get { return eCatalog.Operation.Change.GetHashCode(); }
        }
        /// <summary>Descid du champs du catalogue</summary>
        public int CatDescId
        {
            get { return _catDescId; }
        }
        /// <summary>Descid du champs du catalogue parent du catalogue courant</summary>
        public int CatParentId
        {
            get { return _catParentId; }
        }

        /// <summary>
        /// Gets the cat bound popup.
        /// </summary>
        public PopupType CatBoundPopup
        {
            get { return _catBoundPopup; }
        }
        /// <summary>
        /// Gets the cat bound descid.
        /// </summary>
        public int CatBoundDescid
        {
            get { return _catBoundDescid; }
        }
        /// <summary>
        /// Gets the type of the cat popup.
        /// </summary>
        public PopupType CatPopupType
        {
            get { return _catPopupType; }
        }


        /// <summary>Indique si c'est Catalogue multiple qui est affiché</summary>
        public bool CatMultiple
        {
            get { return _catMultiple; }
        }


        /// <summary>Indique que le catalogue demandé est un catalogue arborescent</summary>
        public bool CatTreeView
        {
            get { return _catTreeView; }
        }

        /// <summary>Indique s'il s'agit de la liste des modèles de mail</summary>
        public bool MailTemplateEdit
        {
            get { return _mailTemplateEdit; }
        }


        /// <summary>Javascript à charger après chargement de la page</summary>
        public String InitJSOutput
        {
            get { return _sbInitJSOutput.ToString(); }
        }
        /// <summary>Javascript à charger après chargement de la page mais en fin de page</summary>
        public String EndJSOutput
        {
            get { return _sbEndJSOutput.ToString(); }
        }

        #endregion

        /// <summary>
        /// Source du catalogue
        /// </summary>
        public enum CatSource
        {
            /// <summary>
            /// Table FILEDATA (ou CATALOG) : catalogues standards -avancés ou simples-
            /// </summary>
            Filedata = 0,
            /// <summary>
            /// Table DESC : tables ou rubriques
            /// </summary>
            Desc = 1,
            /// <summary>
            /// Enum : valeurs d'une énumeration utilisée dans XRM/EudoInternal/EudoQuery
            /// </summary>
            Enum = 2,
        }

        /// <summary>
        /// contexte d'appel de la fenêtre des catalogues
        /// </summary>
        public enum LOADCATFROM
        {
            /// <summary>par défaut</summary>
            UNDEFINED = 0,
            /// <summary>Filtre autre que express</summary>
            FILTER = 1,
            /// <summary>Traitement</summary>
            TREAT = 2,
            /// <summary>Administration</summary>
            ADMIN = 3,
            /// <summary>Filtres express</summary>
            EXPRESSFILTER = 4
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Appelé au chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            try
            {
                #region ajout des css

                PageRegisters.AddCss("eCatalog");
                PageRegisters.AddCss("eMain", "all");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eTitle");
                PageRegisters.AddCss("eIcon");

                if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                    PageRegisters.AddCss("ie7-styles");

                #endregion


                #region ajout des js
                PageRegisters.AddScript("ePopup");
                PageRegisters.AddScript("eFieldEditor");
                PageRegisters.AddScript("eTreeView");
                PageRegisters.AddScript("eDrag");
                PageRegisters.AddScript("eCatalog");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eModalDialog");
                #endregion

                //DateTime debut = DateTime.Now; // Utilisé pour tracer les temps d'execution, à conserver pour d'éventuels tests de performance
                Boolean onError = false;
                String action = String.Empty;
                XmlNode detailsNode = null;
                XmlDocument _xmlResult = new XmlDocument();

                #region Initialisation


                try
                {
                    action = Request.Form["CatAction"].ToString();

                    switch (action)
                    {
                        case "ShowDialog":
                            _dlgAction = DialogAction.SHOW_DIALOG;
                            break;
                        case "RefreshDialog":
                            _dlgAction = DialogAction.REFRESH_DIALOG;
                            break;
                        case "GetAllValues":
                            _dlgAction = DialogAction.ALL_VALUES_DIALOG;
                            break;
                    }

                    _catDescId = int.Parse(Request.Form["CatDescId"].ToString());
                    if (Request.Form["CatParentId"] != null)
                    {
                        int.TryParse(Request.Form["CatParentId"].ToString(), out _catParentId);
                    }
                    else if (Request.Form["CatParentValue"] != null)
                        _catParentValue = Request.Form["CatParentValue"].ToString();

                    // Info sur le bound
                    _catBoundPopup = (PopupType)eLibTools.GetNum(Request.Form["CatBoundPopup"].ToString());
                    int.TryParse(Request.Form["CatBoundDescid"].ToString(), out _catBoundDescid);

                    _catPopupType = (PopupType)eLibTools.GetNum(Request.Form["CatPopupType"].ToString());

                    _catMultiple = (int.Parse(Request.Form["CatMultiple"].ToString()) == 1);

                    if (Request.Form["treeview"] != null)
                        _catTreeView = bool.Parse(Request.Form["treeview"].ToString());

                    // #43472
                    if (Request.Form["searchlimit"] != null)
                        int.TryParse(Request.Form["searchlimit"], out _searchLimit);

                    if (Request.Form["CatTitle"] != null)
                        _catTitle = HttpUtility.UrlDecode(Request.Form["CatTitle"].ToString());
                    _catSearch = Request.Form["CatSearch"].ToString();

                    _fieldEditorJsVarName = Request.Form["CatEditorJsVarName"].ToString();
                    _sbInitJSOutput.Append("eC.jsVarNameEditor = '").Append(_fieldEditorJsVarName).AppendLine("';");

                    //bool fromFilter = _requestTools.GetRequestFormKeyB("FromFilter") ?? false;
                    //if (fromFilter)
                    //{
                    //    _bFromFilter = true;
                    //    _sbInitJSOutput.Append("eC.fromFilter = true;");

                    //}

                    //bool fromTreat = _requestTools.GetRequestFormKeyB("FromTreat") ?? false;
                    //if (fromTreat)
                    //{
                    //    _bFromTreat = true;
                    //    _sbInitJSOutput.Append("eC.fromTreat = true;");

                    //}

                    //_bFromAdmin = _requestTools.GetRequestFormKeyB("FromAdmin") ?? false;
                    //if (_bFromAdmin)
                    //    _sbInitJSOutput.Append("eC.fromAdmin = true;");

                    if (_requestTools.AllKeys.Contains("From"))
                    {
                        _from = _requestTools.GetRequestFormEnum<LOADCATFROM>("From");
                        _sbInitJSOutput.Append($"eC.from = {(int)_from};");

                        //en attendant une refacto de eCatalog.js

                        switch (_from)
                        {
                            case LOADCATFROM.UNDEFINED:
                                break;
                            case LOADCATFROM.FILTER:
                            case LOADCATFROM.EXPRESSFILTER:
                                _sbInitJSOutput.Append("eC.fromFilter = true;");
                                break;
                            case LOADCATFROM.TREAT:
                                _sbInitJSOutput.Append("eC.fromTreat = true;");
                                break;
                            case LOADCATFROM.ADMIN:
                                _sbInitJSOutput.Append("eC.fromAdmin = true;");
                                break;
                            default:
                                break;
                        }
                    }



                    if (!String.IsNullOrEmpty(Request.Form["FrameId"]))
                    {
                        _fieldEditorFrameId = Request.Form["FrameId"].ToString();
                        _sbInitJSOutput.Append("eC.iFrameId = '").Append(_fieldEditorFrameId).AppendLine("';");
                    }

                    if (Request.Form["CatInitialValues"] != null)
                    {
                        _catInitialValues = new List<String>();
                        _catInitialValues.AddRange(Request.Form["CatInitialValues"].ToString().Split(';'));
                    }

                    _catSelectedValues = new List<String>();
                    if (Request.Form["CatSelectedValues"] != null && (_catMultiple || _catTreeView))
                        _catSelectedValues.AddRange(Request.Form["CatSelectedValues"].ToString().Split(';'));
                    else
                        _catSelectedValues = _catInitialValues;


                    if (Request.Form["displayFilter"] != null)
                        _displayFilter = Request.Form["displayFilter"].ToString();

                    if (Request.Form["MailTemplate"] != null)
                        _mailTemplateEdit = bool.Parse(Request.Form["MailTemplate"].ToString());

                    int nCatSource = _requestTools.GetRequestFormKeyI("CatSource") ?? 0;
                    _catSource = (CatSource)nCatSource;

                    if (_catSource == CatSource.Desc)
                    {
                        int nDescType = _requestTools.GetRequestFormKeyI("DescType") ?? 0;
                        _descType = (eCatalogDesc.DescType)nDescType;

                        string sDescTypeIds = _requestTools.GetRequestFormKeyS("DescTypeIds") ?? "";
                        if (!String.IsNullOrEmpty(sDescTypeIds))
                            _descTypeIds = sDescTypeIds.Split(';').Select(i => eLibTools.GetNum(i)).ToList<int>();
                    }
                    else if (_catSource == CatSource.Enum)
                    {
                        int nEnumType = _requestTools.GetRequestFormKeyI("EnumType") ?? 0;
                        _enumType = (eCatalogEnum.EnumType)nEnumType;
                    }

                    _sort = _requestTools.GetRequestFormKeyS("sort") ?? "";
                    _strWidth = _requestTools.GetRequestFormKeyS("width") ?? "";
                    _strHeight = _requestTools.GetRequestFormKeyS("height") ?? "";
                }
                catch (Exception exp)
                {
                    StringBuilder sDevMsg = new StringBuilder();
                    StringBuilder sUserMsg = new StringBuilder();

                    sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "_catDescId, _catPopupType"), " (_catDescId = ", _catDescId, ", _catPopupType = ", _catPopupType.GetHashCode(), ")"));

                    sDevMsg.AppendLine(exp.Message).AppendLine(exp.StackTrace);

                    sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        sUserMsg.ToString(),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        sDevMsg.ToString()

                        );


                    //Arrete le traitement et envoi l'erreur
                    try
                    {
                        LaunchError();
                    }
                    catch (eEndResponseException)
                    { }

                    return;
                }

                #endregion

                onError = onError || (_catDescId <= 0 && _catSource == CatSource.Filedata) || _catPopupType == PopupType.NONE;

                if (onError)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                        eResApp.GetRes(_pref, 72),
                        String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "_catDescId, _catPopupType"), " (_catDescId = ", _catDescId, ", _catPopupType = ", _catPopupType.GetHashCode(), ")")
                    );

                    //Arrete le traitement et envoi l'erreur
                    try
                    {
                        LaunchError();
                    }
                    catch (eEndResponseException)
                    { }
                }


                eudoDAL _edal = eLibTools.GetEudoDAL(_pref);



                try
                {
                    _edal.OpenDatabase();


                    if (_catBoundDescid > 0)
                    {
                        if (_catParentId <= 0)
                        {
                            //Recupère le popupdescid du catBoundDescid
                            int _catBoundPopupDescid = 0;
                            FieldLite fieldInfo = eLibTools.GetFieldInfo(_edal, _catBoundDescid, FieldLite.Factory());
                            if (fieldInfo != null)
                                _catBoundPopupDescid = fieldInfo.PopupDescId;

                            //Recupère la valeur parente
                            _catParentId = eCatalog.GetCatalogValueID(_pref, _catBoundPopup, _catBoundPopupDescid, _catParentValue, _edal, _pref.User);
                        }
                    }
                    else
                    {
                        _catParentId = -1;
                    }

                    if (_catSource == CatSource.Filedata)
                    {

                        // on va chercher le tri dans le uservalue
                        eUserValue uv = new eUserValue(_edal, _catDescId, TypeUserValue.FILEDATA_SORT, _pref.User, _catDescId - (_catDescId % 100));
                        uv.Build();
                        if (_sort.Length == 0)
                        {

                            if (uv.Value.Length > 0)
                            {
                                if (uv.Value == "[TEXT]")
                                    _sort = "lab";
                                else if (uv.Value == "[DATA]")
                                    _sort = "dat";
                                else if (uv.Value == "[FILEID]")
                                    _sort = "fid"; //type fileid non pris en charge par xrm
                                else
                                    _sort = "lab";


                                if (uv.Label == "&order=desc")
                                    _sort = String.Concat(_sort, "dsc");
                                else
                                    _sort = String.Concat(_sort, "asc");
                            }

                        }
                        else
                        {
                            //on enreistre le tri dans le uservalue
                            if (_sort.StartsWith("lab"))
                                uv.Value = "[TEXT]";
                            else if (_sort.StartsWith("dat"))
                                uv.Value = "[DATA]";
                            else if (_sort.StartsWith("fid"))
                                uv.Value = "[FILEID]"; // type [FILEID] non pris en charge par xrm
                            else
                                uv.Value = "[TEXT]";

                            if (_sort.EndsWith("dsc"))
                                uv.Label = "&order=desc";
                            else
                                uv.Label = "";

                            uv.Save();
                        }

                        bool bShowHiddenValues = (new List<LOADCATFROM>() { LOADCATFROM.FILTER, LOADCATFROM.EXPRESSFILTER, LOADCATFROM.ADMIN }).Contains(_from);

                        if (_catMultiple)
                        {
                            _catalog = new eCatalog(_edal, _pref, _catPopupType, _pref.User, _catDescId, _catTreeView, _catParentId, String.Empty, sort: _sort,
                                isSnapshot: _pref.IsSnapshot,
                                showHiddenValues: bShowHiddenValues

                                );
                            InitColumnsWidth();



                            List<CatalogValue> lstValToSearch = new List<CatalogValue>();
                            //85888
                            //dans le cas de no autoload et en l'absence de recherche, on ajoute les valeurs sélectionnées à la recherche
                            // pour qu4il s'affiche dans la popup de catalogue
                            if (!_catTreeView && _catalog.NoAutoload && string.IsNullOrEmpty(_catSearch))
                            {
                                foreach (var t in _catSelectedValues)
                                {
                                    int nval;
                                    if (Int32.TryParse(t, out nval))
                                        lstValToSearch.Add(new CatalogValue() { Id = nval, ParentId = _catParentId });
                                }
                            }
                            else
                            {
                                lstValToSearch.Add(new CatalogValue() { Label = _catSearch, ParentId = _catParentId });
                            }


                            if (lstValToSearch.Count == 0)
                                _catalog = new eCatalog(_edal, _pref, _catPopupType, _pref.User, _catDescId, _catTreeView, _catParentId, _catSearch, sort: _sort, isSnapshot: _pref.IsSnapshot, showHiddenValues: bShowHiddenValues);
                            else
                                _catalog = new eCatalog(_edal, _pref, _catPopupType, _pref.User, _catDescId, _catTreeView, lstValToSearch, _sort, _pref.IsSnapshot, bShowHiddenValues);
                        }
                        else
                        {
                            _catalog = new eCatalog(_edal, _pref, _catPopupType, _pref.User, _catDescId, _catTreeView, _catParentId, _catSearch, sort: _sort, isSnapshot: _pref.IsSnapshot, showHiddenValues: bShowHiddenValues);
                            InitColumnsWidth();
                        }

                    }
                    else if (_catSource == CatSource.Desc)
                    {
                        _catalog = new eCatalogDesc(_pref, _edal, _pref.User, _descType, _descTypeIds, iDescId: CatDescId);
                        ((eCatalogDesc)_catalog).Load();
                        InitColumnsWidth();
                    }
                    else if (_catSource == CatSource.Enum)
                    {
                        _catalog = new eCatalogEnum(_pref, _edal, _pref.User, _enumType);
                        ((eCatalogEnum)_catalog).Load();
                        InitColumnsWidth();
                    }


                    //eModelTools.EudoTraceLog("Apres eCatalog:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
                }
                finally
                {
                    _edal.CloseDatabase();
                }

                // Si les valeurs de catalogue ne depasse pas TREEVIEW_HIGH_THRESHOLD (1000)  "Demande 82234 (Affichage - Lenteur ouverture catalogue arborescent)" QBO
                if (_catalog.Values.Count > eLibConst.TREEVIEW_HIGH_THRESHOLD)
                {
                    PageRegisters.DisplayTheme2019 = false;
                }

                //eModelTools.EudoTraceLog("Catalog chargé:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
                //debut = DateTime.Now;
                switch (_dlgAction)
                {
                    #region Affichage d'ouverture de la fenêtre - ShowDialog

                    case DialogAction.SHOW_DIALOG:
                        // Titre de la fenêtre
                        Panel divHeadTitle = (_catTreeView) ? null : RendHeadTitle();

                        // Panel (valeurs) du catalogue
                        Panel divValues = null;
                        if (_catTreeView)
                            divValues = RendTreeViewVal();
                        else if (_catMultiple && action.Equals("ShowDialog"))
                            divValues = RendMultVal();
                        else
                            divValues = RendVal();      // Catalogue simple et avancé non multiple

                        divCatDialog.Attributes.Add("class", "catDlg");
                        divCatDialog.Attributes["class"] += string.Concat(" ", eTools.GetClassNameFontSize(_pref));

                        if (_catPopupType == PopupType.DATA || _catPopupType == PopupType.ENUM || _catPopupType == PopupType.DESC)
                        {
                            divCatDialog.Attributes["class"] += " data";
                        }
                        else if (_catPopupType == PopupType.FREE || _catPopupType == PopupType.ONLY)
                        {
                            divCatDialog.Attributes["class"] += " v7";
                        }

                        if (_catTreeView)
                            divCatDialog.Attributes["class"] += " arbo";
                        else if (_catMultiple)
                            divCatDialog.Attributes["class"] += " multi";
                        else
                            divCatDialog.Attributes["class"] += " unit";



                        // On ajoute les differentes div entetes valeures .. au div principal
                        if (_catTreeView)
                        {
                            divCatDialog.Controls.Add(RendHeadDisplayOption("All", eResApp.GetRes(_pref, 1318), "eC.setDisplayFilterET(false);", true));
                            divCatDialog.Controls.Add(RendHeadDisplayOption("Collapse", eResApp.GetRes(_pref, 6254), "eC.setDisplayFilterET(false);"));
                            divCatDialog.Controls.Add(RendHeadDisplayOption("OnlySelected", eResApp.GetRes(_pref, 1319), "eC.setDisplayFilterET(false);"));
                        }

                        if (_catSource != CatSource.Desc && _catSource != CatSource.Enum)
                            divCatDialog.Controls.Add(RendHeadSearch());

                        if (!_mailTemplateEdit)
                            divCatDialog.Controls.Add(RendHeadAction());

                        if (divHeadTitle != null)
                            divCatDialog.Controls.Add(divHeadTitle);

                        divCatDialog.Controls.Add(divValues);
                        break;

                    #endregion

                    #region Affichage après une recherche - RefreshDialog

                    case DialogAction.REFRESH_DIALOG:
                        //*****  On force le rendu HTML avant qu'il s'affiche dans la page pour le XML  //
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        System.IO.StringWriter sw = new System.IO.StringWriter(sb);
                        HtmlTextWriter hw = new HtmlTextWriter(sw);

                        System.Text.StringBuilder sbRight = new System.Text.StringBuilder();
                        System.IO.StringWriter swRight = new System.IO.StringWriter(sbRight);
                        HtmlTextWriter hwRight = new HtmlTextWriter(swRight);
                        //**********************************************//

                        Control ctrl = null;
                        Control ctrlRight = null;
                        if (CatTreeView)
                        {
                            ctrl = GenerateTreeViewVal();
                            _sbInitJSOutput.Append("eTV.init(true);"); // reconstruction du TreeView après recherche
                            if (!String.IsNullOrEmpty(_displayFilter))
                            {
                                switch (_displayFilter.ToLower())
                                {
                                    case "onlyselected":
                                        _sbInitJSOutput.AppendLine("eC.ViewHideValues('none');");
                                        break;
                                    case "collapse":
                                        _sbInitJSOutput.AppendLine("eC.CollapseAll('none');");
                                        break;
                                }
                            }
                        }
                        else
                        {
                            ctrl = GenerateTableVal();

                            //si catalogue multiple et pas en mode recherche, on rafraichit aussi le tableau de droite
                            if (_catMultiple && String.IsNullOrEmpty(_catSearch))
                                ctrlRight = GenerateTableValRightPart();
                        }

                        // Pour appel Ajax on recupere la table contenant seulement les valeurs
                        try
                        {
                            ctrl.RenderControl(hw);

                            //si catalogue multiple et pas en mode recherche, on rafraichit aussi le tableau de droite
                            if (_catMultiple && String.IsNullOrEmpty(_catSearch) && !CatTreeView)
                                ctrlRight.RenderControl(hwRight);
                        }
                        catch (Exception ex)
                        {

                            String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                            sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                            ErrorContainer = eErrorContainer.GetDevUserError(
                               eLibConst.MSG_TYPE.CRITICAL,
                               eResApp.GetRes(_pref, 72),
                               eResApp.GetRes(_pref, 6236),
                               eResApp.GetRes(_pref, 72),
                             String.Concat("Catalogues : Erreur lors du rendu du contrôle à afficher après recherche (RefreshDialog)", Environment.NewLine, sDevMsg)
                           );

                            //Arrete le traitement et envoi l'erreur
                            LaunchError();
                            break;
                        }

                        detailsNode = _xmlResult.CreateElement("contents");

                        XmlNode _searchValue = _xmlResult.CreateElement("searchValue");
                        _searchValue.InnerText = _catSearch;
                        detailsNode.AppendChild(_searchValue);

                        XmlNode _nbResultNode = _xmlResult.CreateElement("nbResults");
                        _nbResultNode.InnerText = _catalog.Values.Count.ToString();
                        detailsNode.AppendChild(_nbResultNode);

                        XmlNode _htmlNode = _xmlResult.CreateElement("html");
                        _htmlNode.InnerText = sb.ToString();
                        detailsNode.AppendChild(_htmlNode);

                        //si catalogue multiple et pas en mode recherche, on rafraichit aussi le tableau de droite
                        if (_catMultiple && String.IsNullOrEmpty(_catSearch))
                        {
                            XmlNode _htmlNodeRight = _xmlResult.CreateElement("htmlRight");
                            _htmlNodeRight.InnerText = sbRight.ToString();
                            detailsNode.AppendChild(_htmlNodeRight);
                        }

                        XmlNode _jsNode = _xmlResult.CreateElement("js");
                        _jsNode.InnerText = _sbInitJSOutput.ToString();
                        detailsNode.AppendChild(_jsNode);
                        break;

                    #endregion

                    #region Affichage en MRU

                    case DialogAction.ALL_VALUES_DIALOG:

                        detailsNode = _xmlResult.CreateElement("elementlist");
                        //Test des permissions d'ajout conditionnant l'ajout de la ligne ajouter dans la MRU.
                        XmlNode addAllowedNode = _xmlResult.CreateElement("addpermission");
                        addAllowedNode.InnerText = _catalog.AddAllowed ? "1" : "0";

                        //Liste des valeurs résultat
                        XmlNode _mainNode = _xmlResult.CreateElement("elements");

                        detailsNode.AppendChild(addAllowedNode);
                        detailsNode.AppendChild(_mainNode);

                        foreach (eCatalog.CatalogValue catalogValue in _catalog.Values)
                        {
                            XmlNode _elementNode = _xmlResult.CreateElement("element");

                            XmlNode _elementValueNode = _xmlResult.CreateElement("value");
                            _elementValueNode.InnerText = catalogValue.DbValue;
                            XmlNode _elementLabelNode = _xmlResult.CreateElement("label");
                            _elementLabelNode.InnerText = catalogValue.DisplayValue;

                            _elementNode.AppendChild(_elementValueNode);
                            _elementNode.AppendChild(_elementLabelNode);

                            _mainNode.AppendChild(_elementNode);
                        }
                        break;

                    #endregion

                    default:
                        ErrorContainer = eErrorContainer.GetDevUserError(
                           eLibConst.MSG_TYPE.CRITICAL,
                           eResApp.GetRes(_pref, 72),
                           eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                           eResApp.GetRes(_pref, 72),
                           String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "_dlgAction"), " (_dlgAction = ", _dlgAction, ")")
                       );

                        //Arrete le traitement et envoi l'erreur
                        LaunchError();
                        break;
                }

                //eModelTools.EudoTraceLog("Rendu terminé:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);


                #region Retour en XML dans les cas des recherche (action <> ShowDialog)

                if (detailsNode != null)
                {
                    XmlNode _maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                    _xmlResult.AppendChild(_maintNode);

                    XmlNode _resultNode = _xmlResult.CreateElement("result");
                    _resultNode.InnerText = "SUCCESS";
                    detailsNode.AppendChild(_resultNode);

                    XmlNode _errDesc = _xmlResult.CreateElement("errordescription");
                    _errDesc.InnerText = String.Empty;
                    detailsNode.AppendChild(_errDesc);

                    _xmlResult.AppendChild(detailsNode);

                    Response.Clear();
                    Response.ClearContent();
                    Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    Response.ContentType = "text/xml";
                    Response.Write(_xmlResult.OuterXml);
                    Response.End();
                }

                #endregion
            }
            catch (eEndResponseException)
            { }
            catch (ThreadAbortException)
            { }
            catch (Exception exp)
            {

                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ")
                    .Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1])
                    .Append(String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "_catDescId, _catPopupType"), " (_catDescId = ", _catDescId, ", _catPopupType = ", _catPopupType.GetHashCode(), ")"));

                sDevMsg.AppendLine(exp.Message).AppendLine(exp.StackTrace);

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );


                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchErrorHTML(true);
                }
                catch (eEndResponseException)
                { }
            }
        }

        /// <summary>
        /// Creation d'un bouton radio "Afficher toutes les valeurs" (pour le TreeView)
        /// Le nom de la classe du bouton est "catDivSrchTV"
        /// Le name du bouton est "displayFilter + buttonValue"
        /// </summary>
        /// <param name="buttonValue">La value du Button</param>
        /// <param name="buttonLabel">Le label correspondant au Button (InnerText)</param>
        /// <param name="buttonJavascript">Ce qui sera executé sur le OnClick du Button</param>
        /// <param name="bSelected">Indique si le bouton doit être coché ou non</param>
        /// <returns>Renvoi une Div avec le label et le bouton </returns>
        private Panel RendHeadDisplayOption(string buttonValue, string buttonLabel, string buttonJavascript, Boolean bSelected = false)
        {
            Panel divCatalogHeaderOption = new Panel();
            divCatalogHeaderOption.Attributes.Add("class", "catDivSrchTV");
            HtmlInputRadioButton inputRadioButton = new HtmlInputRadioButton();
            inputRadioButton.Name = "displayFilter";
            inputRadioButton.ID = String.Concat(inputRadioButton.Name, buttonValue);
            inputRadioButton.Value = buttonValue;
            inputRadioButton.Attributes.Add("onclick", buttonJavascript);
            inputRadioButton.Checked = bSelected;

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("for", inputRadioButton.ID);
            label.InnerText = buttonLabel;

            divCatalogHeaderOption.Controls.Add(inputRadioButton);
            divCatalogHeaderOption.Controls.Add(label);

            return divCatalogHeaderOption;
        }

        /// <summary>
        /// Creation d'un DIV avec le libelé Recherche, zone de recherche et le bouton Rechercher
        /// </summary>
        /// <returns>On renvoi un DIV complet</returns>
        private Panel RendHeadSearch()
        {
            // DIV
            Panel divCatalogHeader = new Panel();

            // Entete avec la zone de recherche et la loupe pour rechercher
            divCatalogHeader.CssClass = (_catTreeView) ? "catDivSrchTV" : "catDivSrch";
            divCatalogHeader.ID = "catDivSrch";
            TableRow rows = new TableRow();
            TableCell tbCell = new TableCell();

            // 1ère colonne
            if (_catTreeView)
            {
                tbCell.Controls.Add(RendHeadDisplayOption("Search", String.Concat(eResApp.GetRes(_pref, 595), " : "), "eC.setDisplayFilter(false);"));
                tbCell.CssClass = "eTVLibSrch";
            }
            else
            {
                tbCell.CssClass = "txtsrch";
                tbCell.Text = string.Concat(eResApp.GetRes(_pref, 595), " : ");
            }
            rows.Cells.Add(tbCell);

            // 2e colonne
            tbCell = new TableCell();
            // Champs de recherche
            HtmlInputText inputSearch = new HtmlInputText("text");
            inputSearch.ID = "eTxtSrch";
            inputSearch.Attributes.Add("class", (_catTreeView) ? "eTVTxtSrch" : "eTxtSrch");
            inputSearch.Attributes.Add("onkeyup", "eC.srch(event);");
            inputSearch.Attributes.Add("data-searchlimit", _catalog.SearchLimit.ToString());
            tbCell.Controls.Add(inputSearch);
            rows.Controls.Add(tbCell);

            // 3e colonne
            tbCell = new TableCell();
            tbCell.Attributes.Add("class", "search-btn");
            if (_catTreeView)
                tbCell.CssClass = "eTVLoupe";
            // Div qui contiendra la loupe de recherche
            Panel divLoupe = new Panel();
            divLoupe.CssClass = "catloupe";
            // Image loupe ou croix
            HtmlGenericControl htmlImg = new HtmlGenericControl("span");
            htmlImg.Attributes.Add("title", eResApp.GetRes(_pref, 924));
            htmlImg.Attributes.Add("srchState", "off");
            htmlImg.Attributes.Add("class", "logo-search icon-magnifier");
            htmlImg.Attributes.Add("onclick", "javascript:eC.btnSrch();");
            htmlImg.ID = "eBtnSrch";

            // Action de l'image
            //HyperLink aspLink = new HyperLink();
            //aspLink.Attributes.Add("href", "javascript:eC.btnSrch();");
            //aspLink.Controls.Add(htmlImg);
            //divLoupe.Controls.Add(aspLink);
            divLoupe.Controls.Add(htmlImg);
            tbCell.Controls.Add(divLoupe);
            rows.Cells.Add(tbCell);

            System.Web.UI.WebControls.Table tbheader = new System.Web.UI.WebControls.Table();
            tbheader.Attributes.Add("cellpadding", "0");
            tbheader.Attributes.Add("cellspacing", "0");
            if (_catTreeView)
                tbheader.CssClass = "eTVTabSrch";

            tbheader.Rows.Add(rows);
            divCatalogHeader.Controls.Add(tbheader);

            return divCatalogHeader;
        }

        /// <summary>
        /// Creation d'un DIV contenant le titre de la fenêtre
        /// </summary>
        /// <returns>Retourne une DIV</returns>
        private Panel RendHeadTitle()
        {
            if (!_catMultiple)
                return null;

            // DIV
            Panel divHeaderTitle = new Panel();
            divHeaderTitle.ID = "catMultTitle";
            divHeaderTitle.CssClass = "catMultTitle";
            divHeaderTitle.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 6215)));
            return divHeaderTitle;
        }

        /// <summary>
        /// Creation d'une DIV contenant les boutons synchro et impression ainsi que le bouton ajouter
        /// </summary>
        /// <returns>Retourne une DIV</returns>
        private Panel RendHeadAction()
        {
            // DIV bouton synchro et impression
            Panel divSyncPrint = new Panel();
            divSyncPrint.CssClass = "catDivHead";
            divSyncPrint.ID = "catDivHead";
            HtmlGenericControl ulToolbar = new HtmlGenericControl("ul");
            ulToolbar.Attributes.Add("class", "catTool");

            if (_catPopupType.Equals(PopupType.DATA))
            {

                // Bouton ajouter
                HtmlGenericControl ulToolbarAdd = new HtmlGenericControl("ul");
                ulToolbarAdd.Attributes.Add("class", (CatTreeView) ? "catTVToolAdd" : "catToolAdd");

                if (!CatTreeView)
                {
                    ulToolbarAdd.Attributes.Add("class", "catBtnUl");
                    ulToolbarAdd.Controls.Add(BtnAddLib(_catalog.AddAllowed));
                    ulToolbarAdd.Controls.Add(BtnExportCatalog(_catalog.ExportAllowed));
                    ulToolbarAdd.Controls.Add(BtnImportCatalog(_catalog.ImportAllowed));
                }
                else
                {
                    ulToolbarAdd.Controls.Add(BtnAdd(_catalog.AddAllowed));

                    ulToolbarAdd.Controls.Add(BtnEdit(_catalog.UpdateAllowed));

                    ulToolbarAdd.Controls.Add(BtnDel(_catalog.DeleteAllowed));
                }


                divSyncPrint.Controls.Add(ulToolbarAdd);
                ulToolbar.Controls.Add(CreateLiPrint());
                divSyncPrint.Controls.Add(ulToolbar);
                return divSyncPrint;
            }
            else
            {

                ulToolbar.Controls.Add(CreateLiPrint());

                // HyperLink linkSync = new HyperLink();
                HtmlGenericControl liToolbarSync = new HtmlGenericControl("li");

                //  linkSync.Attributes.Add("href", "javascript:");
                liToolbarSync.Attributes.Add("title", eResApp.GetRes(_pref, 3000));
                if (_catalog.SynchroAllowed)
                {
                    liToolbarSync.Attributes.Add("onclick", "eConfirm(1, top._res_3000, top._res_481 + ' ?','',500,200,function(){eC.syncCat();},function(){});");
                    liToolbarSync.Attributes.Add("class", "icon-sync");
                }
                else
                {
                    liToolbarSync.Attributes.Add("class", "icon-sync disable");
                }

                // liToolbarSync.Controls.Add(linkSync);

                ulToolbar.Controls.Add(liToolbarSync);

                divSyncPrint.Controls.Add(ulToolbar);
            }

            return divSyncPrint;
        }

        /// <summary>
        /// Fabrique un LI contenant un lien pour l'icone impression
        /// </summary>
        /// <returns>Rend un LI</returns>
        private HtmlGenericControl CreateLiPrint()
        {

            HtmlGenericControl liToolbarprint = new HtmlGenericControl("li");


            liToolbarprint.Attributes.Add("title", eResApp.GetRes(_pref, 13));
            liToolbarprint.Attributes.Add("onclick", "return false;");

            //liToolbarprint.Attributes.Add("class", "icon-print"); //TODO, imprimante désactivée car non codé


            return liToolbarprint;
        }

        /// <summary>
        /// Création du bouton d'edition pour renommer une valeur 
        /// </summary>
        /// <param name="_UpdateAllowed">Boolean correspondant au droit de modification sur ce catalogue</param>
        /// <returns>Rend un LI</returns>
        private HtmlGenericControl BtnEdit(bool _UpdateAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");

            //HyperLink link = new HyperLink();
            if (_UpdateAllowed)
            {
                liToolbar.Attributes.Add("onclick", "eC.renameVal();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(_pref, 151));

                // 37200 MCR / GCH si catatogue arborescent, on met "icon-edn-pen" , suppression de "catTVBtnModif"
                // liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVBtnModif" : "icon-edn-pen");
                liToolbar.Attributes.Add("class", "icon-edn-pen");

            }
            else
            {
                //liToolbar.Attributes.Add("href", "javascript:return false;");

                // 37200 MCR / GCH si catatogue arborescent, on met "icon-edn-pen disable" , suppression de "catTVBtnModifDis"
                // liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVBtnModifDis" : "icon-edn-pen disable");
                liToolbar.Attributes.Add("class", "icon-edn-pen disable");

            }
            //liToolbar.Controls.Add(link);
            return liToolbar;
        }

        private HtmlGenericControl BtnExportCatalog(bool _ExportAllowed)
        {
            HtmlGenericControl btnExport = new HtmlGenericControl("li");
            HyperLink link = new HyperLink();

            if (_ExportAllowed)
            {
                link.Attributes.Add("href", $"javascript:eC.exportCat({ _catDescId });");
                link.CssClass = "buttonExport";
                link.ToolTip = eResApp.GetRes(_pref, 16);
                link.Attributes.Add("title", eResApp.GetRes(_pref, 16));
                btnExport.Attributes.Add("class", (CatTreeView) ? "catTVToolAddBtn" : "catToolAddLib exportVal");
                HtmlGenericControl icnSpan = new HtmlGenericControl("span");
                link.Controls.Add(icnSpan);
                icnSpan.Attributes.Add("class", "icon-export");

                Label libBtn = new Label();
                libBtn.Text = eResApp.GetRes(_pref, 16);
                link.Controls.Add(libBtn);
                btnExport.Controls.Add(link);
            }
            else
            {
                link.Attributes.Add("title", eResApp.GetRes(_pref, 2459));
                btnExport.Attributes.Add("class", "icon-export disable");
                btnExport.Attributes.Add("style", "display: none");
            }

            return btnExport;
        }

        private HtmlGenericControl BtnImportCatalog(bool _ImportAllowed)
        {
            HtmlGenericControl btnImport = new HtmlGenericControl("li");
            HyperLink link = new HyperLink();

            if (_ImportAllowed)
            {
                link.Attributes.Add("href", "javascript:eC.openImportCat(" + _catDescId + ");");
                link.CssClass = "buttonImport";
                link.ToolTip = eResApp.GetRes(_pref, 8479);
                link.Attributes.Add("title", eResApp.GetRes(_pref, 8479));
                btnImport.Attributes.Add("class", (CatTreeView) ? "catTVToolAddBtn" : "catToolAddLib importVal");
                HtmlGenericControl icnSpan = new HtmlGenericControl("span");
                link.Controls.Add(icnSpan);
                icnSpan.Attributes.Add("class", "icon-import");

                Label libBtn = new Label();
                libBtn.Text = eResApp.GetRes(_pref, 8479);
                link.Controls.Add(libBtn);
            }
            else
            {
                link.Attributes.Add("title", eResApp.GetRes(_pref, 2458));
                btnImport.Attributes.Add("class", "icon-import disable");
                btnImport.Attributes.Add("style", "display: none");

            }
            btnImport.Controls.Add(link);
            return btnImport;
        }

        /// <summary>
        /// Création du bouton de suppression pour une valeur de catalogue
        /// </summary>
        /// <param name="_DeleteAllowed">Boolean correspondant au droit de suppression sur ce catalogue</param>
        /// <returns>rend un LI</returns>
        private HtmlGenericControl BtnDel(bool _DeleteAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");
            //       HyperLink link = new HyperLink();
            if (_DeleteAllowed)
            {
                liToolbar.Attributes.Add("onclick", "eC.delCatTreeViewVal();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(_pref, 19));

                // 37200 MCR / GCH si catatogue arborescent, on met "icon-delete" , suppression de "catTVBtnDel"
                // liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVBtnDel" : "icon-delete");
                liToolbar.Attributes.Add("class", "icon-delete");
            }
            else
            {
                //link.Attributes.Add("href", "javascript:return false;");

                // 37200 MCR / GCH si catatogue arborescent, on met "icon-delete disable" , suppression de "catTVBtnDelDis"
                // liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVBtnDelDis" : "icon-delete disable");
                liToolbar.Attributes.Add("class", "icon-delete disable");

            }
            //   liToolbar.Controls.Add(link);
            return liToolbar;
        }

        /// <summary>
        /// Création d'un libellé "Ajouter" pour une valeur de catalogue
        /// </summary>
        /// <returns>Rend un LI</returns>
        private HtmlGenericControl BtnAddLib(bool _AddAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");
            HyperLink link = new HyperLink();
            if (_AddAllowed)
            {
                link.Attributes.Add("href", "javascript:eC.addVal();");
                link.CssClass = "buttonAdd";
                link.ToolTip = eResApp.GetRes(_pref, 1486);
                link.Attributes.Add("title", eResApp.GetRes(_pref, 1486));
                liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVToolAddBtn" : "catToolAddLib addVal");
                HtmlGenericControl icnSpan = new HtmlGenericControl("span");
                link.Controls.Add(icnSpan);
                icnSpan.Attributes.Add("class", "icon-add");

                Label libBtn = new Label();
                libBtn.Text = eResApp.GetRes(_pref, 18);
                link.Controls.Add(libBtn);
            }
            else
            {
                link.Attributes.Add("href", "javascript:return false;");
                link.ToolTip = eResApp.GetRes(_pref, 6279);
                link.Attributes.Add("title", eResApp.GetRes(_pref, 6279));
                liToolbar.Attributes.Add("class", (CatTreeView) ? "catTVToolAddBtnDis" : "catToolAddLibDis");
            }



            liToolbar.Controls.Add(link);
            return liToolbar;


        }

        /// <summary>
        /// Création d'un bouton d'ajout pour les catalogues
        /// </summary>
        /// <param name="_AddAllowed">Droit d'ajout</param>
        /// <returns>Rend un LI avec le bouton</returns>
        private HtmlGenericControl BtnAdd(bool _AddAllowed)
        {

            // 37200 MCR /GCH   pour les catalogues arboresecents, ajouter le bouton Ajouter, Modifier et Supprimer

            HtmlGenericControl liToolbar = new HtmlGenericControl("li");

            //HyperLink link = new HyperLink();
            if (_AddAllowed)
            {
                liToolbar.Attributes.Add("onclick", "eC.addVal();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(_pref, 1486));

                liToolbar.Attributes.Add("class", "icon-add");

            }
            else
            {

                liToolbar.Attributes.Add("title", eResApp.GetRes(_pref, 6279));
                liToolbar.Attributes.Add("class", "catTVToolAddBtnDis");

            }
            //liToolbar.Controls.Add(link);
            return liToolbar;


        }

        /// <summary>
        /// Entete du tableau avec les libellés en catalogue avancé ("libellé", "code", "désactivé", "ID")
        /// </summary>
        /// <param name="multSel">True si catalogue multiple </param>
        /// <param name="bLefttPart">Affiche la partie gauche </param>
        /// <returns>Rend une TableHeaderRow </returns>
        private eLiCtrl GenerateTableHeader(Boolean multSel = false, Boolean bLefttPart = true)
        {
            eLiCtrl hdLimain = null;
            //eDivCtrl hdDiv = null;
            Label hdSpan = null;
            //TableHeaderRow hdRow = null;
            //TableHeaderCell hdCell = null;

            if (_catPopupType != PopupType.DATA && !_catMultiple)
                return hdLimain;

            hdLimain = new eLiCtrl();
            hdLimain.Attributes.Add("hd", String.Empty);
            eUlCtrl hdUl = new eUlCtrl();
            hdLimain.Controls.Add(hdUl);
            eLiCtrl hdLi = null;
            //hdDiv = new eDivCtrl();

            if (_catPopupType != PopupType.DATA)
            {
                hdLi = new eLiCtrl();
                if (!multSel)
                    hdLi.InnerText = eResApp.GetRes(_pref, 6213);
                else
                    hdLi.InnerText = eResApp.GetRes(_pref, 6214);

                hdUl.Controls.Add(hdLi);
                hdLi.CssClass = "maskwidth";

            }
            else
            {
                #region Libellé
                hdLi = new eLiCtrl();
                hdSpan = new Label();

                //if (_catMultiple)
                //    hdLi.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDisplayColumnWidth));

                // Verifier si affichage avec Mask du libelle - et code
                if (!_catalog.DisplayMask.Equals("[TEXT]"))
                {
                    // libellé + code selon le masque choisi
                    //hdCell.Style.Add(HtmlTextWriterStyle.Width, "35%");
                    hdSpan.Text = _catTitle; // Nom du catalogue
                    _sbInitJSOutput.Append("eC.catalogTitle = '").Append((_catTitle).Replace("'", @"\'")).AppendLine("';");
                    hdLi.CssClass = "maskwidth";

                }
                else
                {
                    //hdCell.Style.Add(HtmlTextWriterStyle.Width, "40%");
                    hdSpan.Text = eResApp.GetRes(_pref, 223);     // Libellé
                    hdLi.Controls.Add(hdSpan);
                    hdLi.CssClass = "valwidth";
                    GenerateTableHeader_AddSortBtn(hdLi, "lab");  //Ajout des Boutons de tri                    
                }
                hdUl.Controls.Add(hdLi);
                #endregion

                #region Code
                // Vérification si affichage du code
                if (_catalog.DataEnabled)
                {
                    hdLi = new eLiCtrl();
                    //BSE :#50551 
                    //hdLi.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDataColumnWidth));

                    // Isolation du texte dans un span pour une meilleur mise en page
                    hdSpan = new Label();
                    hdSpan.Text = eResApp.GetRes(_pref, 973);     // Code
                    hdLi.Controls.Add(hdSpan);
                    GenerateTableHeader_AddSortBtn(hdLi, "dat");  //Ajout des Boutons de tri
                    hdLi.CssClass = "datawidth";

                    hdUl.Controls.Add(hdLi);
                }
                #endregion

                #region Libellé si il y a un masque
                if (!_catalog.DisplayMask.Equals("[TEXT]"))
                {
                    hdLi = new eLiCtrl();
                    // Libellé seul
                    //hdCell.Style.Add(HtmlTextWriterStyle.Width, "25%");
                    // Isolation du texte dans un span pour une meilleur mise en page
                    hdSpan = new Label();
                    hdSpan.Text = eResApp.GetRes(_pref, 223);     // Libellé
                    hdLi.Controls.Add(hdSpan);
                    GenerateTableHeader_AddSortBtn(hdLi, "lab");  //Ajout des Boutons de tri
                    hdLi.CssClass = "valwidth";

                    hdUl.Controls.Add(hdLi);
                }
                #endregion

                #region ID
                if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                {
                    #region Desactiver
                    // Vérification si affichage du code
                    if (_from == LOADCATFROM.ADMIN)
                    {
                        hdLi = new eLiCtrl();
                        // hdLi.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDisabledColumnWidth));
                        hdLi.CssClass = "diswidth";

                        // Isolation du texte dans un span pour une meilleur mise en page
                        hdSpan = new Label();
                        hdSpan.Text = eResApp.GetRes(_pref, 690);     // Désactivé
                        hdLi.Controls.Add(hdSpan);
                        // TODO - A prendre en charge
                        //GenerateTableHeader_AddSortBtn(hdLi, "dat");  //Ajout des Boutons de tri

                        hdUl.Controls.Add(hdLi);
                    }
                    #endregion

                    // On affiche la colonne ID que si l'on est admin
                    hdLi = new eLiCtrl();
                    //_sIDColumnWidth = GetIDColumnWidth();
                    //hdLi.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nIDColumnWidth));
                    hdLi.CssClass = "idwidth";
                    hdSpan = new Label();
                    hdSpan.Text = "ID";     // ID
                    hdLi.Controls.Add(hdSpan);

                    hdUl.Controls.Add(hdLi);
                }
                #endregion

                #region Boutons
                //Li des Boutons
                hdLi = new eLiCtrl();
                hdLi.CssClass = "tdHBtn";
                // hdLi.Text = "Actions";
                hdUl.Controls.Add(hdLi);
                #endregion
            }

            return hdLimain;
        }

        /// <summary>
        /// Rajoute dans le webcontrol en paramètre le rendu des boutons de tri
        /// </summary>
        /// <param name="webControl">endroit on l'on souhaite ajouter</param>
        /// <param name="sSortPrefix">prefix de boutons de tri</param>
        private void GenerateTableHeader_AddSortBtn(eGenericWebControl webControl, String sSortPrefix)
        {
            String asc = String.Concat(sSortPrefix, "asc"),
                dsc = String.Concat(sSortPrefix, "dsc");

            webControl.OnClick = String.Concat("Sort = ", _sort == asc ? String.Concat("'", dsc, "'") : String.Concat("'", asc, "'"), "; eC.startSearch();");
            webControl.Style.Add("cursor", "pointer");

            HtmlImage sortCtrl = new HtmlImage();
            sortCtrl.Attributes.Add("class", String.Concat("rIco picto SortAsc"));
            sortCtrl.Attributes.Add("src", eConst.GHOST_IMG);
            sortCtrl.ID = "IMG_ASC";
            webControl.Controls.Add(sortCtrl);

            if (_sort == asc)
                sortCtrl.Style.Add(HtmlTextWriterStyle.Visibility, "initial");

            sortCtrl = new HtmlImage();
            sortCtrl.Attributes.Add("class", String.Concat("rIco picto SortDesc"));
            sortCtrl.Attributes.Add("src", eConst.GHOST_IMG);
            sortCtrl.ID = "IMG_DSC";
            webControl.Controls.Add(sortCtrl);

            if (_sort == dsc)
                sortCtrl.Style.Add(HtmlTextWriterStyle.Visibility, "initial");
        }

        /// <summary>
        /// Creation du tableau avec les valeurs catalogues pour les catalogues simple et avancé non multiple
        /// </summary>
        /// <returns>Retourne une table HTML </returns>
        private eUlCtrl GenerateTableVal()
        {
            /*string sAttr_Label = "lib";*/
            /*string sAttr_Label2 = "lib2";*/

            int cntLigne = 0; // Compteur de lignes
            String hypLinkAction = String.Empty;

            //Boolean noValues = (!String.IsNullOrEmpty(_catSearch) && (_catalog.Values.Count == 0 || _catalog.Values[0].Label.ToLower() != _catSearch.ToLower()));
            bool noValues = (!String.IsNullOrEmpty(_catSearch) && (_catalog.Values.Count == 0 || _catalog.Values[0].Label.ToLower() != _catSearch.ToLower()));

            // Table avec les valeures du catalogue
            //System.Web.UI.WebControls.Table tbCatValues = new System.Web.UI.WebControls.Table();

            eUlCtrl ulCatValues = new eUlCtrl();

            ulCatValues.ID = "tbCatVal";
            ulCatValues.CssClass = "catEditVal";
            ulCatValues.Attributes.Add("cellpadding", "0");
            ulCatValues.Attributes.Add("cellspacing", "0");
            ulCatValues.Attributes.Add("descid", _catDescId.ToString());
            ulCatValues.Attributes.Add("pop", _catPopupType.GetHashCode().ToString());
            if (_catPopupType == PopupType.ENUM)
                ulCatValues.CssClass = String.Concat(ulCatValues.CssClass, " enumCat");
            //if (_catMultiple && _nDisplayColumnWidth > 0)
            //    ulCatValues.Width = new Unit(_nTotalColumnWidth, UnitType.Pixel);

            // Valeurs initiales passées au JS
            if (_catInitialValues != null && _catInitialValues.Count > 0 && _dlgAction == DialogAction.SHOW_DIALOG)
                _sbInitJSOutput.Append("eC.initialListElem = '").Append(String.Join(";", _catInitialValues.ToArray()).Replace("'", "\\'")).AppendLine("';");
            if (_catPopupType == PopupType.DATA)
            {
                _sbInitJSOutput.Append("eC.langUsed = '").Append(_catalog.UsedLang).AppendLine("';");
                _sbInitJSOutput.Append("eC.dataEnabled = ").Append(_catalog.DataEnabled.ToString().ToLower()).AppendLine(";");
                _sbInitJSOutput.Append("eC.dataEdit = ").Append(_catalog.DataEditable.ToString().ToLower()).AppendLine(";");

            }
            // Entete du tableau avec les libellés en catalogue avancé ("libellé", "code", "désactivé", "ID")
            eLiCtrl li = GenerateTableHeader(false);
            if (li != null)
            {
                ulCatValues.CssClass = String.Concat(ulCatValues.CssClass, " withHead");
                if (_catPopupType == PopupType.ENUM)
                    ulCatValues.CssClass = String.Concat(ulCatValues.CssClass, " enumCat");
                ulCatValues.Controls.Add(li);
            }

            //Int32 customCellColspan = 0;

            #region Colonnes custom catalogue avancé
            /*
            if (_catPopupType == PopupType.DATA)
            {
                // -1 car le libellé de Non renseigné est dans un td à part
                customCellColspan = row.Cells.Count - 1;
            }
            */
            #endregion

            #region Non renseigné

            if (!_catMultiple || _from == LOADCATFROM.EXPRESSFILTER)
            {
                eLiCtrl liRows = null;
                eUlCtrl ulSubRows = null;
                eLiCtrl liSubRows = null;
                //eDivCtrl divCell = null;
                // Utilisé pour la valeur vide
                liRows = new eLiCtrl();
                liRows.ID = "val__empty_";
                liRows.CssClass = (cntLigne % 2) == 1 ? "" : "odd";
                liRows.OnClick = "eC.clickVal(event);";
                if (_catMultiple)
                    liRows.OnMouseDown = "strtDrag(event);";    //DRAG AND DROP

                liRows.Attributes.Add("index", cntLigne.ToString());
                liRows.Attributes.Add("ednval", _catMultiple ? QueryConst.EMPTY_CAT_VALUE.ToString() : String.Empty);
                liRows.Attributes.Add("ednid", String.Empty);
                liRows.Attributes.Add("bd", String.Empty);

                ulSubRows = new eUlCtrl();
                liRows.Controls.Add(ulSubRows);

                liSubRows = new eLiCtrl();
                string sClassWidth = _catalog.DisplayMask.Equals("[TEXT]") ? "valwidth" : "maskwidth";
                liSubRows.CssClass = $"catEditLbl system edit cell {sClassWidth}";
                liSubRows.ID = "lbl__empty_";
                liSubRows.InnerText = eResApp.GetRes(_pref, 6211);
                ulSubRows.Controls.Add(liSubRows);

                //colonnes spéciales catalogues avancés 
                if (_catPopupType == PopupType.DATA)
                {
                    // Dans le cas ou on autorise l'affichage du code
                    if (_catalog.DataEnabled)
                    {
                        // Colonne Code
                        //BSE :#50551  la largeur de la colonne Code dans les catalogues avancés n'est pas gérée
                        liSubRows = new eLiCtrl();
                        liSubRows.CssClass = "catEditRub edit cell datawidth";
                        //liSubRows.InnerText = EudoQuery.HtmlTools.StripHtml(_catalogValue.Data);
                        //liSubRows.OnMouseOver = String.Concat("ste(event, '", cellIdentity, "');");
                        liSubRows.OnMouseOut = "ht();";
                        //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDataColumnWidth));
                        ulSubRows.Controls.Add(liSubRows);
                    }

                    // Dans le cas d'un masque d'affichage
                    if (!_catalog.DisplayMask.Equals("[TEXT]"))
                    {
                        liSubRows = new eLiCtrl();
                        //MOU ajout de la class catEditRub pour le overflow du texte #35995
                        liSubRows.CssClass = "catEditRub valwidth";
                        liSubRows.InnerText = eResApp.GetRes(_pref, 6211);
                        ulSubRows.Controls.Add(liSubRows);
                    }


                    if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                    {
                        // On affiche la colonne ID que si l'on est admin
                        liSubRows = new eLiCtrl();
                        //liSubRowsValues.InnerText = _catalogValue.Id.ToString();
                        //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nIDColumnWidth));
                        liSubRows.CssClass = "idwidth";

                        ulSubRows.Controls.Add(liSubRows);
                    }
                }


                liSubRows = new eLiCtrl();
                liSubRows.CssClass = "catBtn";
                liSubRows.ID = String.Empty;
                ulSubRows.Controls.Add(liSubRows);



                ulCatValues.Controls.Add(liRows);
            }

            #endregion

            #region Parcours des valeurs du catalogue ou des valeur trouvé après une recherhe

            String lineIdentity = String.Empty;
            String cellIdentity = String.Empty;
            //TableRow rowsValues = null;
            //TableCell tbCellValues = null;

            eLiCtrl liRowsValues = null;
            //eDivCtrl divCellValues = null;
            eUlCtrl ulSubRowsValues = null;
            eLiCtrl liSubRowsValues = null;
            if (_catalog.Values != null)
            {
                // Utilisées pour la dernière colonne boutons d'actions
                // HyperLink hypLink = null;
                eLiCtrl liBtn = null;
                eUlCtrl ulBtnValues = null;

                Panel divbtnValues = null;
                /**/
                #region Taille des boutons sur le Header
                WebControl wcBtn = null;
                if (wcBtn == null)
                    wcBtn = ((WebControl)ulCatValues.Controls[0].Controls[0].Controls[ulCatValues.Controls[0].Controls[0].Controls.Count - 1]);
                if (String.IsNullOrEmpty(wcBtn.Style[HtmlTextWriterStyle.Width]))
                    wcBtn.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nBtnColumnWidth));
                #endregion


                if (_catMultiple && _catSelectedValues.Contains("-1"))
                {
                    _sbInitJSOutput.Append($"eC.selectListValue('val__empty_');");      // Selection des valeurs

                }


                foreach (eCatalog.CatalogValue _catalogValue in _catalog.Values)
                {
                    cntLigne++;

                    lineIdentity = String.Concat("val_", _catalogValue.Id.ToString());
                    cellIdentity = String.Concat("lbl_", _catalogValue.Id.ToString());
                    //BSE: #54 134
                    _catSelectedValues = _catSelectedValues.ConvertAll(x => x.ToUpper());
                    if (_catSelectedValues != null && _catSelectedValues.Contains(_catalogValue.DbValue.ToUpper()))
                    {
                        if (_catMultiple)
                            _sbInitJSOutput.Append("eC.selectListValue('").Append(lineIdentity).AppendLine("');");      // Selection des valeurs
                        else
                            _sbInitJSOutput.Append("eC.hlgltSelVal('").Append(lineIdentity).AppendLine("');");          // Sélection et surbrillance de la valeur
                    }


                    liRowsValues = new eLiCtrl();
                    liRowsValues.Attributes.Add("bd", String.Empty);
                    liRowsValues.ID = lineIdentity;
                    liRowsValues.CssClass = (cntLigne % 2) == 1 ? "" : "odd";
                    liRowsValues.OnClick = "eC.clickVal(event);";
                    if (_catMultiple)
                        liRowsValues.OnMouseDown = "strtDrag(event);";    //DRAG AND DROP

                    liRowsValues.Attributes.Add("index", cntLigne.ToString());
                    // Valeur enregistré en base
                    liRowsValues.Attributes.Add("ednval", _catalogValue.DbValue);
                    // Valeur affiché à l'utilisateur (adv : avec le masque) --- TODO - A SUP EN CAT SIMPLE
                    // Id de la valeur (simple : catid ; adv : dataid)
                    liRowsValues.Attributes.Add("ednid", _catalogValue.Id.ToString());
                    // Infos specifiques
                    if (_catPopupType != PopupType.DATA)
                        liRowsValues.Attributes.Add("parentid", _catParentId.ToString());

                    ulSubRowsValues = new eUlCtrl();
                    liRowsValues.Controls.Add(ulSubRowsValues);

                    // Colonne Valeurs
                    liSubRowsValues = new eLiCtrl();
                    liSubRowsValues.ID = cellIdentity;
                    liSubRowsValues.CssClass = String.Concat("catEditRub edit cell ", _catalog.DisplayMask.Equals("[TEXT]") ? "valwidth" : "maskwidth");
                    liSubRowsValues.InnerText = EudoQuery.HtmlTools.StripHtml(_catalogValue.DisplayValue);
                    liSubRowsValues.Attributes.Add("title",liSubRowsValues.InnerText );
                    liSubRowsValues.Attributes.Add("tt", _catalogValue.ToolTipText);

                    //#67917 - avant on affichait le libellé et l'info bulle était dans un bouton à part
                    //liSubRowsValues.OnMouseOver = String.Concat("ste(event, '", cellIdentity, "');");
                    //liSubRowsValues.OnMouseOut = "ht();";

                    liSubRowsValues.Attributes.Add("onmouseover", String.Concat("st(event, ' ", _catalogValue.ToolTipText.Replace("'", "\\'").Replace("[[BR]]", "<br />"), "' , 'divTTipcat');"));
                    liSubRowsValues.Attributes.Add("onmouseout", "ht();");

                    //if (_catMultiple)
                    //    liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDisplayColumnWidth));
                    ulSubRowsValues.Controls.Add(liSubRowsValues);

                    #region Colonnes custom catalogue avancé

                    if (_catPopupType == PopupType.DATA)
                    {
                        // Dans le cas ou on autorise l'affichage du code
                        if (_catalog.DataEnabled)
                        {
                            // Colonne Code
                            //BSE :#50551  la largeur de la colonne Code dans les catalogues avancés n'est pas gérée
                            liSubRowsValues = new eLiCtrl();
                            liSubRowsValues.CssClass = "catEditRub edit cell datawidth";
                            liSubRowsValues.InnerText = EudoQuery.HtmlTools.StripHtml(_catalogValue.Data);
                            liSubRowsValues.OnMouseOver = String.Concat("ste(event, '", cellIdentity, "');");
                            liSubRowsValues.OnMouseOut = "ht();";
                            //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nDataColumnWidth));
                            ulSubRowsValues.Controls.Add(liSubRowsValues);
                        }

                        // Dans le cas d'un masque d'affichage
                        if (!_catalog.DisplayMask.Equals("[TEXT]"))
                        {
                            liSubRowsValues = new eLiCtrl();
                            //MOU ajout de la class catEditRub pour le overflow du texte #35995
                            liSubRowsValues.CssClass = "catEditRub valwidth";
                            liSubRowsValues.InnerText = _catalogValue.Label;
                            ulSubRowsValues.Controls.Add(liSubRowsValues);
                        }

                        if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                        {
                            if (_from == LOADCATFROM.ADMIN)
                            {
                                // Option disponible uniquement en administration des valeurs
                                eCheckBoxCtrl chk = new eCheckBoxCtrl(_catalogValue.IsDisabled, true);

                                liSubRowsValues = new eLiCtrl();
                                liSubRowsValues.Controls.Add(chk);
                                //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nIDColumnWidth));
                                liSubRowsValues.CssClass = "diswidth";

                                ulSubRowsValues.Controls.Add(liSubRowsValues);
                            }

                            // On affiche la colonne ID que si l'on est admin
                            liSubRowsValues = new eLiCtrl();
                            liSubRowsValues.InnerText = _catalogValue.Id.ToString();
                            //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nIDColumnWidth));
                            liSubRowsValues.CssClass = "idwidth";

                            ulSubRowsValues.Controls.Add(liSubRowsValues);
                        }
                    }

                    #endregion

                    #region Derniere colonne - boutons modifier supprimer et tooltip
                    ulBtnValues = new eUlCtrl(); ;
                    ulBtnValues.CssClass = "catBtnUl";

                    // Bouton modifier
                    //hypLink = new HyperLink();
                    liBtn = new eLiCtrl();
                    liBtn.ToolTip = eResApp.GetRes(_pref, 151);

                    if (_catalog.UpdateAllowed)
                    {
                        liBtn.Attributes.Add("onclick", String.Concat("eC.renameVal('", lineIdentity, "', '", cellIdentity, "', this);"));
                        liBtn.CssClass = "icon-edn-pen";
                    }
                    else
                    {
                        liBtn.CssClass = "catBtnModifDis";
                    }

                    //liBtn.Controls.Add(hypLink);
                    ulBtnValues.Controls.Add(liBtn);

                    if (_catPopupType == PopupType.DATA)
                    {
                        // Bouton toolTip
                        //hypLink = new HyperLink();
                        liBtn = new eLiCtrl();
                        if (!_catalogValue.IsToolTipHTML && !String.IsNullOrEmpty(_catalogValue.ToolTipText))
                        {
                            //hypLink.ToolTip = _catalogValue.ToolTipText;
                            liBtn.Attributes.Add("onmouseover", String.Concat("st(event, ' ", _catalogValue.ToolTipText.Replace("'", "\\'").Replace("[[BR]]", "<br />"), "' , 'divTTipcat');"));
                            liBtn.Attributes.Add("onmouseout", "ht();");
                            liBtn.CssClass = "icon-edn-info";
                        }
                        else
                        {
                            liBtn.ToolTip = "";
                            liBtn.CssClass = "icon-edn-info disable";
                        }

                        //liBtn.Controls.Add(hypLink);
                        ulBtnValues.Controls.Add(liBtn);

                    }

                    // Bouton d'édition de corps de mail si la fenêtre affiche les modèles de mail
                    if (_mailTemplateEdit)
                    {
                        //  hypLink = new HyperLink();
                        liBtn = new eLiCtrl();

                        liBtn.Attributes.Add("onclick", "eC.editMT();");
                        liBtn.CssClass = "catBtnTool";


                        // liBtn.Controls.Add(hypLink);
                        ulBtnValues.Controls.Add(liBtn);
                    }


                    // Bouton Supprimer
                    //hypLink = new HyperLink();
                    liBtn = new eLiCtrl();

                    liBtn.ToolTip = eResApp.GetRes(_pref, 19);


                    if (_catalog.DeleteAllowed)
                    {
                        //liBtn.Attributes.Add("onclick", String.Concat("eC.cfmDelCatVal('", lineIdentity, "');"));
                        liBtn.Attributes.Add("onclick", String.Concat("eC.GetNbOccur('", lineIdentity, "', eC.cfmDelCatVal);"));
                        liBtn.CssClass = "icon-delete";
                    }
                    else
                    {
                        liBtn.CssClass = "catBtnDelDis disable";
                    }


                    //   liBtn.Controls.Add(hypLink);
                    ulBtnValues.Controls.Add(liBtn);

                    divbtnValues = new Panel();
                    divbtnValues.CssClass = "catBtnVal";
                    divbtnValues.Controls.Add(ulBtnValues);

                    liSubRowsValues = new eLiCtrl();
                    liSubRowsValues.CssClass = "catBtn";
                    liSubRowsValues.Controls.Add(divbtnValues);
                    liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, GetColumnWidthStr(_nBtnColumnWidth));
                    //liSubRowsValues.Style.Add(HtmlTextWriterStyle.Width, String.Concat(btnColWidth, "px"));
                    ulSubRowsValues.Controls.Add(liSubRowsValues);

                    #endregion

                    // On insere la ligne générée (<tr>) dans le tableau 
                    ulCatValues.Controls.Add(liRowsValues);
                }
            }
            #endregion

            #region Affichage d'ajout de valeur en catalogue simple

            if (noValues)
            {
                cntLigne++;

                if (_catalog.Values.Count == 0)
                {
                    liRowsValues = new eLiCtrl();
                    liRowsValues.ID = String.Concat("val", cntLigne.ToString());
                    liRowsValues.CssClass = (cntLigne % 2) == 1 ? "catEditVal2" : "catEditVal";
                    liRowsValues.Attributes.Add("index", cntLigne.ToString());
                    liRowsValues.Attributes.Add("bd", String.Empty);

                    ulSubRowsValues = new eUlCtrl();
                    ulSubRowsValues.CssClass = "eCatalogMenuListNoRes";
                    liRowsValues.Controls.Add(ulSubRowsValues);

                    liSubRowsValues = new eLiCtrl();
                    liSubRowsValues.ID = string.Concat("lbl", cntLigne.ToString());
                    liSubRowsValues.CssClass = "eCatalogMenuItemNoRes catEditLbl edit cell";
                    liSubRowsValues.InnerText = eResApp.GetRes(_pref, 6195);

                    ulSubRowsValues.Controls.Add(liSubRowsValues);
                    ulCatValues.Controls.Add(liRowsValues);
                }

                if (_catPopupType != PopupType.DATA && _catalog.AddAllowed)
                {
                    cntLigne++;

                    //Ajout d'une valeur
                    liRowsValues = new eLiCtrl();
                    liRowsValues.ID = string.Concat("val", cntLigne.ToString());
                    liRowsValues.CssClass = (cntLigne % 2) == 1 ? "catEditVal2" : "catEditVal";
                    liRowsValues.Attributes.Add("index", cntLigne.ToString());
                    liRowsValues.Attributes.Add("bd", String.Empty);

                    ulSubRowsValues = new eUlCtrl();
                    liRowsValues.Controls.Add(ulSubRowsValues);

                    liSubRowsValues = new eLiCtrl();
                    liSubRowsValues.CssClass = "catEditLbl system edit cell";
                    liSubRowsValues.OnClick = String.Concat("eConfirm(1, top._res_1486, (top._res_6194 + '').replace('<VALUE>', '", _catSearch.Replace("'", @"\'"), "') + ' ?', '', 500, 200, function(){eC.addCatVal('", _catSearch.Replace("'", @"\'"), "');},function (){});");
                    liSubRowsValues.ID = String.Concat("lbl", cntLigne.ToString());
                    liSubRowsValues.InnerText = eResApp.GetRes(_pref, 6194).Replace("<VALUE>", _catSearch);

                    ulSubRowsValues.Controls.Add(liSubRowsValues);
                    ulCatValues.Controls.Add(liRowsValues);
                }
            }

            #endregion

            return ulCatValues;
        }

        /// <summary>
        /// Creation du tableau de droite avec les valeurs catalogues pour les catalogues simple et avancé multiple
        /// </summary>
        /// <returns>Retourne une table HTML </returns>
        private eUlCtrl GenerateTableValRightPart()
        {
            eUlCtrl ulCatalogRightPart = new eUlCtrl();
            ulCatalogRightPart.ID = "tbCatSelVal";
            ulCatalogRightPart.CssClass = "catMultSelVal";
            //if (_nDisplayColumnWidth > 0)
            //    ulCatalogRightPart.Width = new Unit(_nTotalColumnWidth, UnitType.Pixel);
            /*TODOGCH
            tbCatalogRightPart.Attributes.Add("cellpadding", "0");
            tbCatalogRightPart.Attributes.Add("cellspacing", "0");*/
            if (_catPopupType == PopupType.ENUM)
                ulCatalogRightPart.CssClass = String.Concat(ulCatalogRightPart.CssClass, " enumCat");

            eLiCtrl liRow = GenerateTableHeader(true, false);
            if (liRow != null)
            {
                ulCatalogRightPart.CssClass = String.Concat(ulCatalogRightPart.CssClass, " withHead");
                // On supprime la colonne actions si catalogue avancé
                if (_catPopupType == PopupType.DATA)
                    liRow.Controls[0].Controls.RemoveAt(liRow.Controls[0].Controls.Count - 1);
                ulCatalogRightPart.Controls.Add(liRow);
            }

            return ulCatalogRightPart;
        }

        /// <summary>
        /// Création de la structure du catalogue arbo
        /// </summary>
        /// <returns>Retourne un UL</returns>
        private HtmlGenericControl GenerateTreeViewVal()
        {
            HtmlGenericControl ulCatalogRoot = new HtmlGenericControl("ul");
            ulCatalogRoot.ID = "eTVBC_0";
            ulCatalogRoot.Attributes.Add("class", "eTVRoot");
            ulCatalogRoot.Attributes["class"] = String.Concat(ulCatalogRoot.Attributes["class"], " eTVcol");

            HtmlGenericControl liCatalogRoot = new HtmlGenericControl("li");
            liCatalogRoot.Attributes.Add("class", "eTVO");
            HtmlGenericControl spanCatalogRoot = new HtmlGenericControl("span");
            HtmlGenericControl spanCatalogRootText = new HtmlGenericControl("span");

            // Ajout de la valeur passée en paramètre
            spanCatalogRoot.ID = "eTVBLV_0";
            spanCatalogRoot.Attributes.Add("onclick", "eC.clickVal(event, this, false);");
            spanCatalogRootText.ID = "eTVBLVT_0";
            spanCatalogRootText.Attributes.Add("ednval", "0");
            spanCatalogRootText.Attributes.Add("ednid", "0");

            if (!String.IsNullOrEmpty(_catTitle))
            {
                spanCatalogRootText.InnerHtml = _catTitle;
                _sbInitJSOutput.Append("eC.catalogTitle = '").Append((_catTitle).Replace("'", @"\'")).Append("';");
            }

            spanCatalogRoot.Controls.AddAt(0, spanCatalogRootText);

            liCatalogRoot.ID = "eTVB_0";
            liCatalogRoot.Controls.Add(spanCatalogRoot);


            HtmlGenericControl ulCatalogValue = new HtmlGenericControl("ul");

            _sbInitJSOutput.Append("eC.langUsed = '").Append(_catalog.UsedLang).AppendLine("';");
            _sbInitJSOutput.Append("eC.dataEnabled = ").Append(_catalog.DataEnabled.ToString().ToLower()).Append(";");
            bool bCollapse = true;
            // #38 930 - On dresse une liste des IDs des valeurs de catalogue affichables, pour pouvoir ensuite vérifier que chaque valeur
            // enfant ait bien une valeur parente. Ceci, afin d'éviter l'affichage de valeurs orphelines (iso-v7)
            // Cette liste n'étant utilisée que pour ce traitement particulier sur les catalogues arbo, il a été jugé inutile de la stocker
            // en permanence sur l'objet eCatalog.
            List<int> _catalogValueIds = new List<int>();
            foreach (eCatalog.CatalogValue _catalogValue in _catalog.Values)
                _catalogValueIds.Add(_catalogValue.Id);

            // Puis on effectue le parcours pour l'affichage
            foreach (eCatalog.CatalogValue _catalogValue in _catalog.Values)
            {
                if (!_catalogValue.IsAChild)	//On affiche en première branche que les non enfants
                {
                    // si la branche a au moins un enfant coché, on l'affichera dépliée
                    if (_catInitialValues.Contains(_catalogValue.Id.ToString()))
                        bCollapse = false;

                    // #38 930 - Si la valeur est enfante d'une valeur parente qui n'existe plus, on ne l'affiche pas (iso-v7)
                    // Plutôt qu'une recherche en base via GetCatalogValue(), plus lente (ouverture d'une connexion + transaction),
                    // on utilise le tableau mémoire rempli plus haut
                    if (_catalogValue.ParentId > 0 && !_catalogValueIds.Contains(_catalogValue.ParentId))
                        continue;

                    ulCatalogValue.Controls.Add(CreateTreeViewChildValueControl(_catalogValue));
                }
            }
            if (!bCollapse)
                ulCatalogValue.Attributes["class"] = String.Concat(ulCatalogValue.Attributes["class"], " eTVcol");

            liCatalogRoot.Controls.Add(ulCatalogValue);
            ulCatalogRoot.Controls.Add(liCatalogRoot);
            return ulCatalogRoot;
        }

        /// <summary>
        /// Creation du panel des catalogues non multiple et non arbo
        /// </summary>
        /// <returns>Retourne une DIV</returns>
        private Panel RendVal()
        {
            Panel divValues = new Panel();

            eUlCtrl tbValues = GenerateTableVal();      // Catalogue simple et avancé non multiple

            // DIV contenant le tableau avec les valeurs du catalogue
            divValues.CssClass = "catEditVal";
            divValues.ID = "eCEDValues";

            // Validation de la popup par double-clic : l'objet JS est situé sur le parent (car le tableau est situé dans l'iframe interne de eModalDialog)
            divValues.Attributes.Add("onDblClick", "eC.dblClickVal(event);");
            // On ajoute la table au div
            divValues.Controls.Add(tbValues);

            return divValues;
        }

        /// <summary>
        /// Creation du panel des catalogues multiple
        /// </summary>
        /// <returns>Retourne une DIV</returns>
        private Panel RendMultVal()
        {
            TableRow tbRow = null;
            TableCell tbCell = null;

            System.Web.UI.WebControls.Table tbGlobal = new System.Web.UI.WebControls.Table();
            tbGlobal.Attributes.Add("cellpadding", "0");
            tbGlobal.Attributes.Add("cellspacing", "0");
            tbGlobal.Attributes.Add("width", "100%");
            #region Ligne de tout les select

            Panel divSelVal = null;
            tbRow = new TableRow();

            // Cellule : Valeurs du catalogue
            tbCell = new TableCell();
            divSelVal = new Panel();
            divSelVal.ID = "eCEDValues";
            if (_nWidth > 0)
                divSelVal.Width = new Unit(_nListWidth, UnitType.Pixel);
            divSelVal.CssClass = "catMultEditVal";
            divSelVal.Attributes.Add("onDblClick", "eC.btnSelItem(event);");  //GCH demande #19139 : Sur IE8, la sélection d'une valeur de catalogue MULTIPLE par double-clic n'est pas prise en compte.
            divSelVal.Controls.Add(GenerateTableVal());
            tbCell.Controls.Add(divSelVal);
            tbCell.CssClass = "catMultHead";
            tbRow.Controls.Add(tbCell);

            // Cellule : Boutons de selection de valeurs
            tbCell = new TableCell();
            tbCell.Attributes.Add("style", "text-align: center;");
            tbCell.CssClass = "catMultHeadSep";
            HtmlGenericControl centerCell = new HtmlGenericControl("center");
            tbCell.Controls.Add(centerCell);

            // Bouton deplacement à droite
            Panel divBtn = new Panel();
            divBtn.ID = "BtnSelect";
            divBtn.CssClass = "icon-item_add";
            divBtn.Attributes.Add("onclick", "eC.btnSelItem();");
            centerCell.Controls.Add(divBtn);

            centerCell.Controls.Add(new HtmlGenericControl("br"));

            // Bouton deplacement à gauche
            divBtn = new Panel();
            divBtn.ID = "BtnUnselect";
            divBtn.CssClass = "icon-item_rem";
            divBtn.Attributes.Add("onclick", "eC.btnUnSelItem();");
            centerCell.Controls.Add(divBtn);

            tbRow.Controls.Add(tbCell);

            // Cellule : Zone de selection de droite
            // On supprime la colonne actions si catalogue avancé
            if (_catPopupType == PopupType.DATA && _nListWidth < _nTotalColumnWidth)
            {
                _nDisplayColumnWidth -= _nBtnColumnWidth;
                _nTotalColumnWidth -= _nBtnColumnWidth;
            }

            tbCell = new TableCell();
            divSelVal = new Panel();
            divSelVal.ID = "eCEDSelValues";
            if (_nWidth > 0)
                divSelVal.Width = new Unit(_nListWidth, UnitType.Pixel);
            divSelVal.CssClass = "catMultEditVal";
            divSelVal.Attributes.Add("onDblClick", "eC.btnUnSelItem(event);");  //GCH demande #19139 : Sur IE8, la sélection d'une valeur de catalogue MULTIPLE par double-clic n'est pas prise en compte.
            divSelVal.Controls.Add(GenerateTableValRightPart());
            tbCell.Controls.Add(divSelVal);
            tbCell.CssClass = "catMultHead";
            tbRow.Controls.Add(tbCell);

            tbGlobal.Controls.Add(tbRow);

            #endregion

            #region Ligne des boutons de toutes selections/deselections

            tbRow = new TableRow();
            tbRow.CssClass = "catMultSelUnsel";
            // Sel
            tbCell = new TableCell();
            tbCell.Attributes.Add("align", "center");
            HtmlGenericControl Div = new HtmlGenericControl("div");
            HtmlGenericControl DivIcon = new HtmlGenericControl("div");
            HtmlGenericControl DivText = new HtmlGenericControl("div");
            Div.ID = "BtnSelectAll";
            DivIcon.ID = "BtnSelectAllIcon";
            DivText.ID = "BtnSelectAllText";
            Div.Attributes.Add("class", "btnSelectUnselectAll");
            DivIcon.Attributes.Add("class", "btnSelectUnselectAllIcon icon-select_all");
            DivText.Attributes.Add("class", "btnSelectUnselectAllText");
            Div.Attributes.Add("onclick", "eC.btnAllSelItem();");
            DivText.InnerHtml = String.Concat(" ", eResApp.GetRes(_pref, 431));
            Div.Controls.Add(DivIcon);
            Div.Controls.Add(DivText);
            tbCell.Controls.Add(Div);
            tbRow.Controls.Add(tbCell);

            tbCell = new TableCell();
            tbRow.Controls.Add(tbCell);

            // Unsel
            tbCell = new TableCell();
            tbCell.Attributes.Add("align", "center");
            Div = new HtmlGenericControl("div");
            DivIcon = new HtmlGenericControl("div");
            DivText = new HtmlGenericControl("div");
            Div.ID = "BtnUnselectAll";
            DivIcon.ID = "BtnUnselectAllIcon";
            DivText.ID = "BtnUnselectAllText";
            Div.Attributes.Add("class", "btnSelectUnselectAll");
            DivIcon.Attributes.Add("class", "btnSelectUnselectAllIcon icon-remove_all");
            DivText.Attributes.Add("class", "btnSelectUnselectAllText");
            Div.Attributes.Add("onclick", "eC.btnAllUnSelItem();");
            DivText.InnerHtml = String.Concat(" ", eResApp.GetRes(_pref, 432));
            Div.Controls.Add(DivIcon);
            Div.Controls.Add(DivText);
            tbCell.Controls.Add(Div);
            tbRow.Controls.Add(tbCell);

            tbGlobal.Controls.Add(tbRow);

            #endregion

            Panel divCatalogWindow = new Panel();
            divCatalogWindow.CssClass = "catGlobalVal";
            divCatalogWindow.Controls.Add(tbGlobal);

            return divCatalogWindow;
        }

        /// <summary>
        /// Génère le contenu d'un catalogue arborescent sous la forme d'un contrôle HTML DIV 
        /// </summary>
        /// <returns>Retourne une DIV</returns>
        private Panel RendTreeViewVal()
        {
            // Valeurs initiales passées au JS
            if (_catInitialValues != null && _catInitialValues.Count > 0 && _dlgAction == DialogAction.SHOW_DIALOG)
                _sbInitJSOutput.Append("eC.initialListElem = '").Append(String.Join(";", _catInitialValues.ToArray()).Replace("'", "\\'")).AppendLine("';");
            if (_catPopupType == PopupType.DATA)
            {
                _sbInitJSOutput.Append("eC.langUsed = '").Append(_catalog.UsedLang).AppendLine("';");
                _sbInitJSOutput.Append("eC.dataEnabled = ").Append(_catalog.DataEnabled.ToString().ToLower()).AppendLine(";");
                _sbInitJSOutput.Append("eC.dataEdit = ").Append(_catalog.DataEditable.ToString().ToLower()).AppendLine(";");

            }

            Panel divValues = new Panel();

            HtmlGenericControl ulValues = GenerateTreeViewVal();      // Structure du catalogue arbo

            // DIV contenant le tableau avec les valeurs du catalogue
            divValues.CssClass = "catEditVal";
            divValues.ID = "eCEDValues";
            // On ajoute la table au div
            divValues.Controls.Add(ulValues);

            return divValues;
        }


        /// <summary>
        /// Vérifie si la valeur ou un de ces enfants/sous-enfant... dans l'arbo est cochée
        /// </summary>
        /// <param name="catValue">valeur a vérifier</param>
        /// <returns>true si la valeur ou un enfant est coché</returns>
        private bool LookForCheckedInTree(eCatalog.CatalogValue catValue)
        {

            if (_catSelectedValues.Contains(catValue.Id.ToString()))
            {
                return true;
            }
            else if (catValue.ChildrenValues == null || catValue.ChildrenValues.Count == 0)
                return false;
            if (catValue.ChildrenValues.Find(item => _catSelectedValues.Contains(item.Id.ToString())) != null)
            {
                return true;
            }
            else
            {
                bool isChecked = false;
                foreach (CatalogValue cat in catValue.ChildrenValues)
                {
                    isChecked = LookForCheckedInTree(cat);
                    if (isChecked)
                        return isChecked;
                }
            }

            return false;
        }

        /// <summary>
        /// Génère le contrôle HTML LI correspondant à une branche de catalogue arborescent, avec ses enfants - Fonction récursive
        /// </summary>
        /// <param name="catValue">Valeur de catalogue pour laquelle générer le code</param>
        /// <returns>Un contrôle HTML LI correspondant à la valeur de catalogue passée en paramètre, incluant ses propres enfants</returns>
        private HtmlGenericControl CreateTreeViewChildValueControl(eCatalog.CatalogValue catValue)
        {
            HtmlGenericControl liCatalogValue = new HtmlGenericControl("li");
            HtmlGenericControl spanCatalogValue = new HtmlGenericControl("span");
            HtmlGenericControl spanCatalogValueText = new HtmlGenericControl("span");

            eCheckBoxCtrl cbCatalogValue = new eCheckBoxCtrl(false, false);
            cbCatalogValue.AddClass("chkAction");
            cbCatalogValue.AddClass("TVChk");
            //cbCatalogValue.AddClick("eC.clickVal(event, this, true);");
            cbCatalogValue.ID = String.Concat("chkValue_", catValue.Id);
            cbCatalogValue.Attributes.Add("name", "chkValue");
            cbCatalogValue.AddClick(String.Empty);  //vide plutôt que non appelée pour qu'il y ait au moins le code JS de base contenant le return false (necessaire pou firefox 17 et quelques versions supérieures)
            // Sélection (cochage) des valeurs actuellement sélectionnées (ou initialement présentes dans le champ)
            if (LookForCheckedInTree(catValue))
            {
                cbCatalogValue.SetChecked(true);
                _sbInitJSOutput.Append("eC.hlgltSelVal('eTVBLV_").Append(catValue.Id).AppendLine("', true);");
            }

            // Ajout de la valeur passée en paramètre
            spanCatalogValue.ID = String.Concat("eTVBLV_", catValue.Id);
            spanCatalogValue.Attributes.Add("onclick", "eC.clickVal(event, this, true);");
            spanCatalogValue.Attributes.Add("title", catValue.ToolTipText);

            spanCatalogValueText.InnerHtml = HttpUtility.HtmlEncode(catValue.DisplayValue);
            spanCatalogValueText.ID = "eTVBLVT_" + catValue.Id;

            spanCatalogValueText.Attributes.Add("ednid", catValue.Id.ToString());
            spanCatalogValueText.Attributes.Add("ednval", catValue.Id.ToString());

            spanCatalogValue.Controls.AddAt(0, cbCatalogValue);
            spanCatalogValue.Controls.AddAt(1, spanCatalogValueText);

            liCatalogValue.ID = "eTVB_" + catValue.Id;
            liCatalogValue.Controls.Add(spanCatalogValue);

            // Et des enfants, récursivement
            if (catValue.ChildrenValues.Count > 0)
            {
                HtmlGenericControl ulCatalogValues = new HtmlGenericControl("ul");
                ulCatalogValues.ID = "eTVBC_" + catValue.Id;
                bool bCollapse = true;
                foreach (eCatalog.CatalogValue _childValue in catValue.ChildrenValues)
                {
                    // si la branche a au moins un enfant coché, on l'affichera dépliée
                    if (_catInitialValues.Contains(_childValue.Id.ToString()))
                        bCollapse = false;

                    ulCatalogValues.Controls.Add(CreateTreeViewChildValueControl(_childValue));

                }
                if (!bCollapse)
                    ulCatalogValues.Attributes["class"] = String.Concat(ulCatalogValues.Attributes["class"], " eTVcol");
                liCatalogValue.Controls.Add(ulCatalogValues);
            }

            return liCatalogValue;
        }

        /// <summary>
        /// Initialise la largeur de toutes les colonnes
        /// </summary>
        private void InitColumnsWidth()
        {
            _nDisplayColumnWidth = GetDisplayColumnWidth();
            _nDataColumnWidth = GetDataColumnWidth();
            _nDisabledColumnWidth = 90;
            _nIDColumnWidth = GetIDColumnWidth();
            _nLabelColumnWidth = GetLabelColumnWidth();
            _nBtnColumnWidth = GetBtnColumnWidth();

            _nTotalColumnWidth = _nDisplayColumnWidth;

            if (_catPopupType == PopupType.DATA)
            {
                if (_catalog.DataEnabled)
                {
                    _nTotalColumnWidth += _nDataColumnWidth;
                }

                if (_from == LOADCATFROM.ADMIN)
                {
                    _nTotalColumnWidth += _nDisabledColumnWidth;
                }

                // #49381 : Ajout de la taille de la colonne [Libellé]
                if (!_catalog.DisplayMask.Equals("[TEXT]"))
                {
                    _nTotalColumnWidth += _nLabelColumnWidth;
                }

                if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                {
                    _nTotalColumnWidth += _nIDColumnWidth;
                }
            }

            _nTotalColumnWidth += _nBtnColumnWidth;

            _nTotalColumnWidth += ValRowPaddingMarginBorderWidth;

            InitListDimensions();
        }

        /// <summary>
        /// Initialise la largeur des listes de valeurs
        /// </summary>
        private void InitListDimensions()
        {
            Int32.TryParse(_strWidth, out _nWidth);
            Int32.TryParse(_strHeight, out _nHeight);

            _nListWidth = _nWidth;
            if (_catMultiple)
            {
                // Taille de chaque liste = taille de la fenêtre / 2 - marge de 2% de la taille de la fenêtre
                if (_nWidth > 0)
                    _nListWidth = (int)Math.Round(_nWidth / 2 - (_nWidth * ((float)6 / 100)));

                if (_nListWidth > _nTotalColumnWidth)
                {
                    _nDisplayColumnWidth += _nListWidth - _nTotalColumnWidth;
                    _nTotalColumnWidth = _nListWidth;
                }
            }
        }

        /// <summary>
        /// Retourne la chaine de caractère représentant la largeur pour le style
        /// </summary>
        /// <param name="nWidth">largeur en px</param>
        /// <returns></returns>
        private string GetColumnWidthStr(int nWidth)
        {
            string returnValue = String.Empty;

            if (nWidth > 0)
                returnValue = String.Concat(nWidth.ToString(), "px");

            return returnValue;
        }

        /// <summary>
        /// Calcule la taille à donner pour l'affichage de la colonne ID sur la fenêtre de catalogue avancé multiple
        /// </summary>
        /// <returns></returns>
        private int GetIDColumnWidth()
        {
            // On récupère les valeurs du catalogue dans un tableau temporaire
            List<String> tempValues = new List<String>();
            foreach (eCatalog.CatalogValue v in _catalog.Values)
            {
                tempValues.Add(v.Id.ToString());
            }

            return GetColumnWidth(tempValues);
        }

        /// <summary>
        /// Calcule la taille à donner pour l'affichage de la colonne Code ("Data") sur la fenêtre de catalogue avancé multiple
        /// </summary>
        /// <returns></returns>
        private int GetDataColumnWidth()
        {
            // On récupère les valeurs du catalogue dans un tableau temporaire
            List<String> tempValues = new List<String>();
            foreach (eCatalog.CatalogValue v in _catalog.Values)
            {
                tempValues.Add(v.Data);
            }

            int width = GetColumnWidth(tempValues);
            if (width < 65)
                width = 65;
            return width;
        }

        /// <summary>
        /// Calcule la taille à donner pour l'affichage de la colonne Libellé sur la fenêtre de catalogue avancé multiple
        /// </summary>
        /// <returns></returns>
        private int GetDisplayColumnWidth()
        {

            // On récupère les valeurs du catalogue dans un tableau temporaire
            List<String> tempValues = new List<String>();
            foreach (eCatalog.CatalogValue v in _catalog.Values)
            {
                tempValues.Add(v.DisplayValue);
            }

            return GetColumnWidth(tempValues);
        }

        /// <summary>
        /// Calcule la taille à donner pour l'affichage de la colonne Libellé (sans masque) sur la fenêtre de catalogue avancé multiple
        /// </summary>
        /// <returns></returns>
        private int GetLabelColumnWidth()
        {

            // On récupère les valeurs du catalogue dans un tableau temporaire
            List<String> tempValues = new List<String>();
            foreach (eCatalog.CatalogValue v in _catalog.Values)
            {
                tempValues.Add(v.Label);
            }

            return GetColumnWidth(tempValues);
        }


        /// <summary>
        /// Retourne la largeur maximum par rapport à une liste de valeurs
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private int GetColumnWidth(List<String> values)
        {
            if (values.Count == 0)
                return 0;

            // Tri par longueur de valeur
            values.Sort((a, b) => a.Length.CompareTo(b.Length));

            int maxWidth = 0;
            int tempWidth = 0;

            // On prend les 5 plus longues valeurs et on fait un MesureString pour obtenir la plus grande largeur
            int maxCount = (values.Count > 5) ? 5 : values.Count;
            for (int i = 0; i < maxCount; i++)
            {
                tempWidth = eTools.MesureString(values[i]);
                if (tempWidth > maxWidth)
                {
                    maxWidth = tempWidth;
                }
            }

            return maxWidth;
        }

        /// <summary>
        /// Calcule la taille à donner pour l'affichage de la colonne contenant les bouton d'action
        /// </summary>
        /// <returns></returns>
        private int GetBtnColumnWidth()
        {
            int nWidth = 0;
            nWidth += BtnDefaultWidth;//Modif
            if (_catPopupType == PopupType.DATA)
                nWidth += BtnDefaultWidth;//ToolTipText
            nWidth += BtnDefaultWidth;//Delete
            if (_mailTemplateEdit)
                nWidth += BtnDefaultWidth;//Modèle de mail
            if (nWidth == 0)
                nWidth = BtnDefaultWidth * 3;

            return nWidth;
        }

        private enum DialogAction
        {
            SHOW_DIALOG,
            REFRESH_DIALOG,
            ALL_VALUES_DIALOG,
            NONE
        }
    }
}