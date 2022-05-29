using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu des modes fiches
    /// </summary>
    public class eMainFileRenderer : eMasterFileRenderer
    {
        #region PROPRIETES

        /// <summary>
        /// Paramètres nécessaire au rendu de la fiche
        /// </summary>
        protected ExtendedDictionary<String, Object> _dicParams;
        /// <summary>Fait-on un traitement de masse ?</summary>
        internal Boolean GlobalAffect = false;

        /// <summary>Gestion ++/xxx invitation?</summary>
        internal Boolean GlobalInvit = false;


        /// <summary>Affichage de la fiche en readonly (utilisé pour l'affichage en popup depuis une specif)</summary>
        protected Boolean _bReadOnly = false;

        /// <summary>Indique si la page a été reposté sur elle-même. Utilisé uniquement sur les fiches planning</summary>
        protected bool _isPostback = false;

        /// <summary>Input contenant la liste des séparateurs de pages à clore lors de l'ouverture de la fiche</summary>
        protected HtmlInputHidden _closedSep;


        /// <summary>Lafiche est elle affichée dans une popup</summary>
        protected Boolean _bPopupDisplay = false;

        /// <summary>Indique l'ID du premier champ de type E-mail que l'on a analysé sur la fiche</summary>
        private String _sFstMailFieldId = String.Empty;

        /// <summary>Liste des descid autorisée en écriture cumulatif : sur des reload rules en blank, la liste est enrichie, jamais diminuée. Encodée pour éviter les failles d'escalade de privilège</summary>
        protected List<String> _liCumulativeAllowedFieldsDescId = new List<String>();

        // Sauvegarde les descid des propriétés de la fiche
        private List<String> _listOfFilePropDescId = new List<String>();

        /// <summary>La fiche est-elle affichée en signet mode fiche ?</summary>
        protected bool _isBkmFile = false;

        #endregion

        #region ACCESSEURS

        /// <summary>
        /// indique si la fiche est affichée depuis une popup
        /// </summary>
        public Boolean PopupDisplay
        {
            get { return _bPopupDisplay; }
            set { _bPopupDisplay = value; }
        }

        /// <summary>
        /// Indique si on est en signet mode fiche
        /// </summary>
        public bool IsBkmFile
        {
            get { return _isBkmFile; }
            set { _isBkmFile = value; }
        }


        /// <summary>Accesseur vers le conteneur de champs de liaison/pied de page (ex : depuis eMailRendererTools)</summary>
        public Panel PnlDetailsBkms
        {
            get { return _backBoneRdr.PnlDetailsBkms; }
        }

        /// <summary>
        /// Paramètres nécessaire au rendu de la fiche
        /// </summary>
        public ExtendedDictionary<String, Object> DicParams
        {
            get { return _dicParams; }
        }

        /// <summary>
        /// Paramètres nécessaire au rendu de la fiche
        /// </summary>
        /// <param name="dicParams"></param>
        public virtual void SetDicParams(ExtendedDictionary<String, Object> dicParams)
        {
            this._dicParams = dicParams;
        }

        #endregion

        #region CONSTRUCTEUR
        /// <summary>
        /// Constructeur sans paramètres par défaut
        /// </summary>
        protected eMainFileRenderer()
        {
            _rType = RENDERERTYPE.MainFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab"></param>
        /// <param name="nFileId">Id de la fiche</param>
        public eMainFileRenderer(ePref pref, Int32 nTab, Int32 nFileId)
        {
            Pref = pref;
            _tab = nTab;
            _nFileId = nFileId;
            _rType = RENDERERTYPE.MainFile;



        }

        #endregion

        #region METHODES PRINCIPALES DE RENDERER

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {
                if (_tab == (int)TableType.USER)
                    _myFile = eFileMainUser.CreateMainFile(Pref, _tab, _nFileId, 0);
                else if (_tab == (int)TableType.RGPDTREATMENTSLOGS)
                    _myFile = eFileMainRGPDTreatmentLog.CreateMainFile(Pref, _tab, _nFileId, 0);
                else
                    _myFile = eFileMain.CreateMainFile(Pref, _tab, _nFileId, 0);


                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eMainFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eMainFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }


        /// <summary>
        /// Construction des objets HTML
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            Boolean bDisplayBkmBlock = true;
            if (_myFile.ViewMainTable.TabType == TableType.PP && _myFile.FileId == 0)
                bDisplayBkmBlock = true;
            else if (_myFile.ViewMainTable.TabType == TableType.USER && _myFile.FileId > 0)
                bDisplayBkmBlock = true;
            else if (_myFile.ViewMainTable.EdnType != EdnType.FILE_MAIN)
                bDisplayBkmBlock = false;
            else if (_bPopupDisplay)
                bDisplayBkmBlock = false;
            //else if (_myFile.ViewMainTable.TabType == TableType.ADR)
            //    bDisplayBkmBlock = false;

            _backBoneRdr = (eFileBackBoneRenderer)eRendererFactory.CreateFileBackBone(_tab, _bPopupDisplay, bDisplayBkmBlock, _isBkmFile);

            this._pgContainer = _backBoneRdr.PgContainer;

            this._pgContainer.Attributes.Add("edntype", ((int)_myFile.ViewMainTable.EdnType).ToString());

            // Ajout du lien vers les propriétés de la fiche dans le cas du mode fiche en signet
            if (_isBkmFile && _backBoneRdr.PnlBkmFileProps != null)
            {
                _backBoneRdr.PnlBkmFileProps.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 54)));
                _backBoneRdr.PnlBkmFileProps.Attributes.Add("onclick", $"shPties({_tab}, {_myFile.FileId})");
            }

            // Div de champ caché
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", _myFile.ViewMainTable.DescId);
            _backBoneRdr.PnFilePart1.Controls.Add(_divHidden);

            //CSS ICON STANDARD
            // MAB - Utilisation de l'extension .png pour toutes les icônes
            //String sCssIconPosition = " center center ";
            //if (_bPopupDisplay)
            //    sCssIconPosition = " 0 0 ";
            //String sCSSStdIcon = String.Concat("background:url(themes/",
            //    Pref.ThemePaths.GetImageWebPath("/images/iFileIcon/" + _myFile.ViewMainTable.GetIcon.Replace(".jpg", ".png"))
            //    , ") ", sCssIconPosition, " no-repeat !important  ");
            //HtmlInputHidden inptDefIconCss = new HtmlInputHidden();
            //inptDefIconCss.ID = "ICON_DEF_" + _myFile.ViewMainTable.DescId;
            //inptDefIconCss.Attributes.Add("etype", "css");
            //inptDefIconCss.Attributes.Add("ecssname", String.Concat("iconDef_", _tab));
            //inptDefIconCss.Attributes.Add("ecssclass", sCSSStdIcon);
            //_divHidden.Controls.Add(inptDefIconCss);

            //histodescid pour la fonction historiser et créer
            HtmlInputHidden iptHistoDescid = new HtmlInputHidden();
            iptHistoDescid.ID = String.Concat("hdid_" + _myFile.ViewMainTable.DescId);
            iptHistoDescid.Value = _myFile.HistoInfo.Descid.ToString();
            _divHidden.Controls.Add(iptHistoDescid);


            #region Couleur conditionnelle
            // TODO : gérer le tooltip
            //Création de la css Couleur Conditionnelle
            if (_myFile.Record.RuleColor.HasRuleColor)
            {
                String sLstRulesCss = String.Empty;
                String sRulesId = String.Concat(_myFile.Record.RuleColor.Idendity, "_1");
                AddPochoirInputCss(_myFile.Record, 1, ref sLstRulesCss, sRulesId);

                /*

                //Construction de la liste
                String sRulesCss = String.Empty;

                String sRulesId = String.Concat(_myFile.Record.RuleColor.Idendity, "_1");

                sRulesCss = String.Concat(sRulesCss, "background:");


                if (!String.IsNullOrEmpty(_myFile.Record.RuleColor.BgColor))
                    sRulesCss = String.Concat(sRulesCss, _myFile.Record.RuleColor.BgColor);

                if (!string.IsNullOrEmpty(_myFile.Record.RuleColor.Icon))
                {
                    //TODO : gestion réelle des icone conditionnel
                    String sIconColor = String.Concat("themes/", Pref.Theme, "/images/iconPochoirTMP/", "contact1.png");

                    sRulesCss = String.Concat(sRulesCss, " url(", sIconColor, ") center center no-repeat");
                }

                sRulesCss = String.Concat(sRulesCss, "  !important ;");

                //Création du css de la règle
                HtmlInputHidden inptRulesCss = new HtmlInputHidden();
                inptRulesCss.ID = sRulesId;
                inptRulesCss.Attributes.Add("etype", "css");
                inptRulesCss.Attributes.Add("ecssname", sRulesId);
                inptRulesCss.Attributes.Add("ecssclass", sRulesCss);
                _divHidden.Controls.Add(inptRulesCss);

                */

            }

            #endregion


            #region FICHE


            //creation d'une input contenant les titres séparateurs de page  fermer lors de l'ouverture de la page
            _closedSep = new HtmlInputHidden();
            _closedSep.ID = String.Concat("ClosedSep_", _tab);
            _divHidden.Controls.Add(_closedSep);
            //if (_myFile.IsGlobalSepClosed)
            //    _closedSep.Value = "global";

            _divHidden.Controls.Add(GetPropertiesTable());


            // Liste des descid de la proprité de la fiche : 95, 96, 97, 98, 99 ..etc
            HtmlInputHidden listeOfFilePropDescid = new HtmlInputHidden();
            listeOfFilePropDescid.ID = String.Concat("fileProp_", _tab);
            listeOfFilePropDescid.Value = eLibTools.Join(";", _listOfFilePropDescId);

            _divHidden.Controls.Add(listeOfFilePropDescid);

            List<eFieldRecord> sortedFields = null;
            SortFields(out sortedFields);
            FillContent(sortedFields);

            #endregion

            return true;
        }


        /// <summary>
        /// Procure les champs à afficher
        /// </summary>
        /// <param name="sortedFields"></param>
        protected override void SortFields(out List<eFieldRecord> sortedFields)
        {
            base.SortFields(out sortedFields);
            if (!_bPopupDisplay || _layoutMode == eFileLayout.Mode.DISPORDER)
                return;

            eFieldRecord myMemoField = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + (int)EudoQuery.AllField.MEMO_NOTES));

            if (myMemoField != null && myMemoField.RightIsVisible)
            {
                sortedFields.Add(myMemoField);
                //myMemoField.FldInfo.PosDisporder = 1;
                myMemoField.FldInfo.PosColSpan = _myFile.ViewMainTable.ColByLine;

            }
        }

        /// <summary>
        /// Crée un tableau htm contenant les propriétés de la fiche (Appartient à etc.)
        /// </summary>
        /// <returns></returns>
        protected virtual System.Web.UI.WebControls.Table GetPropertiesTable()
        {
            if (_myFile.ViewMainTable.TabType == TableType.PJ)
                return new System.Web.UI.WebControls.Table();

            #region champs composant les propriétés : je fais ma liste de courses
            List<Int32> liPtyFieldsDescId = new List<Int32>();
            GetPropertiesFields(ref liPtyFieldsDescId);
            #endregion

            #region Création du tableau HTML et utilisation des fonctions de renderer par cellules

            System.Web.UI.WebControls.Table tbPtyFields = new System.Web.UI.WebControls.Table();
            tbPtyFields.ID = String.Concat("pty_", _myFile.ViewMainTable.DescId);
            tbPtyFields.CssClass = "filePty";

            // [07/01/2014 - MOU, #35912]
            // Le contenu de la fenêtre des propiétés de la fiche (table id= tbPtyFields.ID)  est transferé du document principal vers le document de l'iframe de la fenêtre, en js,
            // le click ainsi définie sur la table ne fonctionne pas correctement sur IE8. L'évènement n'est pas passé en paramètre (undefined) quand on clique sur le "appartient a".
            // Du coup on définit dans les lignes suivantes les appels aux fonctions parentes, puisque le context est bien preservé (eInlineEditorObject, eCatalogEditorObject...)             
            tbPtyFields.Attributes.Add("onclick", "fldLClick(event);");
            tbPtyFields.Attributes.Add("onselect", "fldSelect(event);");
            tbPtyFields.Attributes.Add("onkeyup", "fldKeyUp(event);");

            foreach (Int32 fldDescId in liPtyFieldsDescId)
            {
                eFieldRecord fld = GetFldRecord(fldDescId);



                if (fld == null)
                    throw new Exception(String.Concat("eMainfFileRenderer - GetPropertiesTable() - Rubrique introuvable : ", fldDescId));

                if (!fld.RightIsVisible)
                    continue;

                Boolean bDisplayBtn = _bIsEditRenderer && fld.RightIsUpdatable;

                #region affectation de fiche

                /* * MOU 26/09/2013  Traitement de masse ; affectation d une nouvelle fiche
                 * 
                 * le "appartient a" est egale a l'utilisateur connecté (userlogin) si on affecte un template (hors planning)
                 * sinon <identique a la fiche @table> ou table est egale a PP , PM, EVT ou EVT_XX
                 * 
                 * */

                if (fldDescId == _myFile.ViewMainTable.GetOwnerDescId())
                {
                    Boolean isExtendedTarget = _myFile.ViewMainTable.ProspectEnabled && _myFile.ViewMainTable.EdnType != EdnType.FILE_MAIL;
                    Boolean isPlusPlus = _myFile.ViewMainTable.AdrJoin && _myFile.ViewMainTable.InterEVT && _myFile.ViewMainTable.InterPP && _myFile.ViewMainTable.EdnType != EdnType.FILE_MAIL;

                    //identique à @tab ou tab = PP, PM ou EVT
                    if (GlobalAffect && !_isPostback && !isExtendedTarget && !isPlusPlus
                        && (fld.FldInfo.Table.EdnType == EdnType.FILE_PLANNING || fld.FldInfo.Table.EdnType == EdnType.FILE_MAIN))
                        fld.Value = "-1";

                    if (GlobalAffect && fld.Value.Equals("-1") && (fld.FldInfo.Table.EdnType == EdnType.FILE_PLANNING || fld.FldInfo.Table.EdnType == EdnType.FILE_MAIN))
                    {
                        //Traitement de masse
                        //Sur les fiches créees le 'Appartient à' a la valeur '<Identique à la fiche Contacts>'    
                        fld.DisplayValue = String.Concat("<", eResApp.GetRes(Pref, 819), " ", eLibTools.GetPrefName(Pref, _myFile.FileContext != null ? _myFile.FileContext.TabFrom : 0), ">"); //
                    }

                }

                #endregion

                TableRow tr = new TableRow();
                tbPtyFields.Rows.Add(tr);

                TableCell tcLabel = new TableCell();
                TableCell tcButton;
                TableCell tcValue;

                AllField sysfld = (AllField)(fld.FldInfo.Descid % 100);
                GetFieldLabelCell(tcLabel, _myFile.Record, fld);
                tcLabel.Attributes.Add("sys", "1");

                switch (sysfld)
                {

                    case AllField.DATE_CREATE:
                    case AllField.DATE_MODIFY:
                    case AllField.USER_CREATE:
                    case AllField.USER_MODIFY:

                        tcValue = new TableCell();
                        tcButton = new TableCell();
                        //fld.RightIsUpdatable = false;                       
                        tcValue = (TableCell)GetFieldValueCell(_myFile.Record, fld, 0, Pref);
                        break;
                    default:
                        tcValue = (TableCell)GetFieldValueCell(_myFile.Record, fld, 0, Pref);
                        tcButton = GetButtonCell(tcValue, bDisplayBtn);
                        break;
                }

                tcValue.CssClass += " table_values";
                tcLabel.CssClass += " table_lab_pty";
                tr.Cells.Add(tcLabel);
                tr.Cells.Add(tcValue);

                tr.Cells.Add(tcButton);

                _listOfFilePropDescId.Add(fld.FldInfo.Descid.ToString());
            }


            #endregion

            return tbPtyFields;
        }

        /// <summary>
        /// Recupere le fieldRecord de descid dans le record
        /// </summary>
        /// <param name="fldDescId"></param>
        /// <returns></returns>
        protected virtual eFieldRecord GetFldRecord(Int32 fldDescId)
        {
            return _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", fldDescId));
        }

        /// <summary>
        /// Les champs systèmes de propriété de la fiche ont un rendu de type cell, car ils ne sont pas modifiables
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        protected override EdnWebControl CreateEditEdnControl(eFieldRecord fld)
        {
            AllField sysfld = (AllField)(fld.FldInfo.Descid % 100);
            switch (sysfld)
            {


                //case AllField.GEOGRAPHY:
                case AllField.DATE_CREATE:
                case AllField.DATE_MODIFY:
                case AllField.USER_CREATE:
                case AllField.USER_MODIFY:

                    return new EdnWebControl() { WebCtrl = new TableCell(), TypCtrl = EdnWebControl.WebControlType.TABLE_CELL };
                default:
                    return base.CreateEditEdnControl(fld);
            }
        }


        /// <summary>
        /// champs composant les propriétés : je fais ma liste de courses
        /// </summary>
        /// <returns></returns>
        protected override List<Int32> GetPropertiesFields(ref List<Int32> PtyFieldsDescId)
        {
            if (PtyFieldsDescId == null)
                PtyFieldsDescId = new List<Int32>();

            // Appartient à
            PtyFieldsDescId.Add(_myFile.ViewMainTable.GetOwnerDescId());

            // <fiche> de : - copropriétaires - appartient à multiple
            Int32 multiOwnerDescId = _myFile.ViewMainTable.GetMultiOwnerDescId();
            if (multiOwnerDescId != 0)
                PtyFieldsDescId.Add(multiOwnerDescId);

            // Confidentielle
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.CONFIDENTIAL);

            // Créé par, Créé le
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.DATE_CREATE);
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.USER_CREATE);

            // Modifié le, Modifié par
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.DATE_MODIFY);
            PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.USER_MODIFY);

            //CNA - Bug #57275
            //Dans la montée de version 10.305 on ne rajoute pas le champ geography sur toutes les tables.
            //On récupère donc le champ geography uniquement pour ces tables.
            if (_myFile.ViewMainTable.DescId < 100000
                || _myFile.ViewMainTable.DescId == (int)TableType.USER
                || _myFile.ViewMainTable.DescId == (int)TableType.CAMPAIGN
                || _myFile.ViewMainTable.DescId == (int)TableType.TRACKLINK
                || _myFile.ViewMainTable.DescId == (int)TableType.CAMPAIGNSTATS
                || _myFile.ViewMainTable.DescId == (int)TableType.NOTIFICATION
                || _myFile.ViewMainTable.DescId == (int)TableType.NOTIFICATION_TRIGGER
            )
                PtyFieldsDescId.Add(_myFile.ViewMainTable.DescId + (int)AllField.GEOGRAPHY);

            return PtyFieldsDescId;
        }


        /// <summary>
        /// implémente le champ note (94) dans le cas du mode création
        /// </summary>
        protected override void AddMemoField()
        {
            if (!(_nFileId == 0 || _bPopupDisplay))
                return;

            base.AddMemoField();

        }

        /// <summary>
        /// Adds the parent head.
        /// </summary>
        protected override void AddParentHead()
        {
            if (_bPopupDisplay)
                return;

            base.AddParentHead();
        }

        /// <summary>
        /// Séparateur
        /// </summary>
        /// <param name="myField">eFieldRecord</param>
        protected override void ListClosedSep(eFieldRecord myField)
        {
            Boolean bSepClose = false;
            Boolean.TryParse(myField.Value, out bSepClose);

            if (bSepClose)
            {
                if (_closedSep.Value.Length > 0)
                    _closedSep.Value += ";";

                _closedSep.Value += myField.FldInfo.Descid.ToString();
            }

        }

        /// <summary>
        /// Gestion des champs correspondant à la cible étendue
        /// </summary>
        /// <param name="myField">Champ concerné</param>
        /// <param name="fldPPRec">Enregistrement de PP concerné</param>
        protected override void SetProspectMatchedFields(eFieldRecord myField, eFieldRecord fldPPRec)
        {
            // Cibles étendues ISOV7: les champs mappés sont en lecture seule lors de la création
            // demande d'evolution #39838 pouvoir editer la fiche à la création unitaire
            if (_myFile.ViewMainTable.ProspectEnabled
                    && myField.FileId == 0
                    && myField.FldInfo.ProspectMatch > 100
                    && (this.GlobalInvit || fldPPRec.FileId > 0))
            {

                myField.RightIsUpdatable = false;

                //Si la liaison est rattachée à un contact, on vide les champs mappés 
                if (myField.FldInfo.ProspectMatch > 0)
                    myField.DisplayValue = myField.Value = String.Empty;
            }
        }



        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            if (_myFile.IsClone)
                return;

            // en création de contact on affiche le masque de l'adresse
            if ((_bPopupDisplay || IsBkmFile) && !(_myFile.ViewMainTable.TabType == TableType.PP && _myFile.FileId == 0))
                return;

            Boolean bDisplayAll = (_myFile.ActiveBkm == -1);
            Int32 nWidth = 0;

            if (_dicParams != null)
            {
                if (_dicParams.ContainsKey("width") && _dicParams["width"] != null)
                    _dicParams.TryGetValueConvert("width", out nWidth, 0);

                if (_dicParams.ContainsKey("filecontext") && _dicParams["filecontext"] != null)
                    _myFile.FileContext = (eFileTools.eFileContext)_dicParams["filecontext"];
            }

            base.GetBookMarkBlock();

            eRenderer bkmsListRdr;
            if (_myFile.FileId == 0 && _myFile.ViewMainTable.TabType == TableType.PP)
            {
                String sPMValue = String.Empty;

                if (_myFile.DicValues == null)
                    _myFile.DicValues = new Dictionary<int, string>();

                _myFile.DicValues.TryGetValue((int)TableType.PM, out sPMValue);
                if (String.IsNullOrEmpty(sPMValue)
                    && _myFile.FileContext != null 
                    && _myFile.ParentFileId != null 
                    && _myFile.ParentFileId.ParentPmId > 0)
                {
                    if (_myFile.DicValues.ContainsKey((int)TableType.PM))
                        _myFile.DicValues[(int)TableType.PM] = _myFile.ParentFileId.ParentPmId.ToString();
                    else
                        _myFile.DicValues.Add((int)TableType.PM, _myFile.ParentFileId.ParentPmId.ToString());

                    sPMValue = _myFile.FileContext.ParentFileId.ParentPmId.ToString();
                }

                Boolean bFromPM = _myFile.FileContext != null && _myFile.FileContext.TabFrom == (int)TableType.PM;
                _dicParams.Add("isPPAdrCombined", true);
                //#37334  - Passage de dicparams complet, on extrait dans la méthode les paramètres nécessaires
                bkmsListRdr = eRendererFactory.CreateAddressCreationRenderer(Pref, _myFile.DicValues, bFromPM, _dicParams);
            }
            else
            {
                bkmsListRdr = eRendererFactory.CreateBookmarkListRenderer(Pref, _myFile, bDisplayAll);
            }

            if (_myFile.ActiveBkm == (int)EudoQuery.ActiveBkm.DISPLAYFIRST && _bFileTabInBkm)
                bkmsListRdr.PgContainer.Style.Add("display", "none");

            if (bkmsListRdr.InnerException != null)
                _eException = bkmsListRdr.InnerException;

            eTools.TransfertFromTo(bkmsListRdr.PgContainer, _backBoneRdr.PnBkmContainer);
        }


        /// <summary>
        /// traitement de fin de rendu
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {

            AddParentInFoot();
            if (!base.End())
                return false;
            #region Liste des champs et liaison parentes présentes dans la fiche

            // indique les identifiants des input contenant les informations parentes
            // on retire les descids des liaisons parentes de la liste des rubriques de la fiche.
            HtmlInputHidden parentInputElements = new HtmlInputHidden();
            parentInputElements.ID = String.Concat("PrtTabs_", _tab);
            _divHidden.Controls.Add(parentInputElements);

            StringBuilder sbParentInputElements = new StringBuilder();

            if (!this._myFile.FldFieldsInfos.Exists(delegate (Field field) { return field.Format == FieldFormat.TYP_ALIASRELATION; }))
            {
                if (_myFile.ViewMainTable.InterPP || _myFile.ViewMainTable.TabType == TableType.ADR)
                {
                    sbParentInputElements.Append((int)TableType.PP + 1);
                    FieldsDescId.Remove(((int)TableType.PP + 1).ToString());

                }

                if (_myFile.ViewMainTable.InterPM || _myFile.ViewMainTable.TabType == TableType.ADR)
                {
                    if (sbParentInputElements.Length > 0)
                        sbParentInputElements.Append(";");

                    sbParentInputElements.Append((int)TableType.PM + 1);
                    FieldsDescId.Remove(((int)TableType.PM + 1).ToString());
                }

                if (_myFile.ViewMainTable.AdrJoin)
                {
                    if (sbParentInputElements.Length > 0)
                        sbParentInputElements.Append(";");

                    sbParentInputElements.Append((int)TableType.ADR + 1);
                    FieldsDescId.Remove(((int)TableType.ADR + 1).ToString());
                }

                if (_myFile.ViewMainTable.InterEVT)
                {
                    if (sbParentInputElements.Length > 0)
                        sbParentInputElements.Append(";");

                    sbParentInputElements.Append(_myFile.ViewMainTable.InterEVTDescid + 1);
                    FieldsDescId.Remove((_myFile.ViewMainTable.InterEVTDescid + 1).ToString());

                }
            }
            parentInputElements.Value = sbParentInputElements.ToString();

            //Création d'un input contenant les id des liaisons parentes
            HtmlInputHidden linkedId = new HtmlInputHidden();
            linkedId.ID = String.Concat("lnkid_", _tab);
            linkedId.Value = _myFile.ParentFileId.GetLnkIdInfos();

            _divHidden.Controls.Add(linkedId);



            #region Gestion des PJ en mode Pop Up
            if (_bPopupDisplay && _myFile.Record.IsPJViewable && !_myFile.IsClone && _myFile.ViewMainTable.TabType != TableType.ADR)
            {
                Int32 pjDescId = _myFile.ViewMainTable.DescId + (int)AllField.ATTACHMENT;

                if (_myFile.FileId == 0)
                {
                    FieldsDescId.AddContains(pjDescId.ToString());
                    AllowedFieldsDescId.AddContains(pjDescId.ToString());
                }

                String cellName = eTools.GetFieldValueCellName(_myFile.ViewMainTable.DescId, String.Concat(_myFile.ViewMainTable.DescId, "_", pjDescId));
                HtmlGenericControl hidPj = new HtmlGenericControl("span");  //SPAN pour ENGINE
                hidPj.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                hidPj.Style.Add(HtmlTextWriterStyle.Display, "none");
                hidPj.Attributes.Add("did", pjDescId.ToString());
                hidPj.ID = eTools.GetFieldValueCellId(cellName, _myFile.Record.MainFileid, _myFile.Record.MainFileid);
                hidPj.Attributes.Add("eName", cellName);

                _pgContainer.Controls.Add(hidPj);
                hidPj.Attributes.Add("view", "1");

                AppendPJAttributes(pjDescId, hidPj);
            }
            #endregion

            //creation d'une input contenant la liste des champs de la fiche
            HtmlInputHidden fieldsDescId = new HtmlInputHidden();
            fieldsDescId.ID = String.Concat("fieldsId_", _tab);
            _divHidden.Controls.Add(fieldsDescId);
            fieldsDescId.Value = eLibTools.Join(";", FieldsDescId);

            // Liste cumulative des champs autorisée en écriture
            // utilisé pour les fiches popup rechargé via applyrulesonblanl
            //  -> si des règles de modifs sont modifié, il faut garder la trace des champs qui ont été autorisée en écriture pendant le cycle
            // des applyrules
            HtmlInputHidden fieldsAllowedDescId = new HtmlInputHidden();

            fieldsAllowedDescId.ID = String.Concat("ctrlId_", _tab);

            _liCumulativeAllowedFieldsDescId = AllowedFieldsDescId;

            try
            {
                //récupération des paramètres précédent
                List<string> lstPrev = new List<string>();
                if (DicParams?.ContainsKey("blankallowedfield") ?? false)
                {
                    _liCumulativeAllowedFieldsDescId.AddRange(DicParams["blankallowedfield"].ToString().Split(";"));

                    if (_liCumulativeAllowedFieldsDescId.Count > 0)
                    {
                        //#43797 - 13/04/2016
                        //La liste des field dans le input caché fieldsId_<tab>
                        // est utilisé pour envoyé la liste des descid de ce champ sur les maj
                        // il faut donc ajouter la liste cumulative des champs ayant été autorisée.
                        HashSet<String> allCumulTransferedField = new HashSet<string>();
                        allCumulTransferedField.UnionWith(FieldsDescId);
                        allCumulTransferedField.UnionWith(_liCumulativeAllowedFieldsDescId);
                        fieldsDescId.Value = eLibTools.Join(";", allCumulTransferedField);


                        //random
                        Random rnd = new Random();
                        _liCumulativeAllowedFieldsDescId = _liCumulativeAllowedFieldsDescId.OrderBy(des => rnd.Next()).ToList();


                    }
                }

                fieldsAllowedDescId.Value = CryptoAESRijndael.Encrypt(eLibTools.Join(";", _liCumulativeAllowedFieldsDescId), CryptographyConst.KEY_CRYPT_LINK6, CryptographyConst.KEY_CRYPT_LINK1.Substring(0, 16));
            }
            catch
            {
                //Si echec de crytage, la liste des champs valide est laissée vide
            }

            _divHidden.Controls.Add(fieldsAllowedDescId);



            #endregion

            _pgContainer.ID = String.Concat("fileDiv_", _myFile.ViewMainTable.DescId);
            _pgContainer.CssClass = "fileDiv";

            //Ajout de la fontsize 
            String sUserFontSize = eTools.GetUserFontSize(Pref);
            _pgContainer.CssClass += " fs_" + sUserFontSize + "pt";

            _pgContainer.Attributes.Add("fid", _myFile.FileId.ToString());
            if (_myFile.ViewMainTable.InterPM)
                _pgContainer.Attributes.Add("pmid", _myFile.Record.GetFieldByAlias(String.Concat(_myFile.CalledTabDescId, "_", 301)).FileId.ToString());
            // cas de la duplication unitaire _nFileId correspond à la fiche d'origine 
            // alors que _myFile correspond à la fiche en cours de création
            if (_myFile.FileId == 0 && _nFileId > 0)
                _pgContainer.Attributes.Add("fid0", _nFileId.ToString());

            _pgContainer.Attributes.Add("edntype", ((int)_myFile.ViewMainTable.EdnType).ToString());
            if (_myFile.ViewMainTable.EdnType == EdnType.FILE_PLANNING)
                _pgContainer.Attributes.Add("ecal", Pref.GetPref(_myFile.ViewMainTable.DescId, ePrefConst.PREF_PREF.CALENDARENABLED));
            _pgContainer.Attributes.Add("did", _myFile.ViewMainTable.DescId.ToString());

            _pgContainer.Attributes.Add("tabfrom", _myFile.FileContext != null ? _myFile.FileContext.TabFrom.ToString() : "0");

            if (!_myFile.Record.RightIsUpdatable)
                _pgContainer.Attributes.Add("ro", "1");

            return true;
        }

        /// <summary>
        /// Construit la fin du contenu de la fiche
        /// </summary>
        protected override void EndFillContent()
        {
            if (_bPopupDisplay)
            {
                HtmlInputHidden hidDel = new HtmlInputHidden();
                hidDel.ID = String.Concat("rightInfo_", _tab);
                _divHidden.Controls.Add(hidDel);
                if (_myFile.Record.RightIsDeletable)
                    hidDel.Attributes.Add("del", "1");
                else
                    hidDel.Attributes.Add("del", "0");

                //BSE #49380 Cacher le bouton imprimer sur les template en mode fiche si on a pas les droits
                bool printAllowed = eLibDataTools.IsTreatmentAllowed(Pref, Pref.User, eLibConst.TREATID.PRINT);

                if (printAllowed && _tab != (int)TableType.USER)
                    hidDel.Attributes.Add("print", "1");
                else
                    hidDel.Attributes.Add("print", "0");

            }
        }

        /// <summary>
        /// Ajoute les ids et le nombre de pj
        /// </summary>
        /// <param name="pjDescId"></param>
        /// <param name="hidPj"></param>
        protected virtual void AppendPJAttributes(Int32 pjDescId, HtmlGenericControl hidPj)
        {
            // Récupération des PjIds 
            String error = string.Empty;
            String strPjIds = string.Empty;
            Int32 nPjCnt = 0;
            Dictionary<int, string> _dicoLstPj = new Dictionary<int, string>();
            if (_myFile != null && _myFile.DicValues != null && _myFile.DicValues.TryGetValue(pjDescId, out strPjIds)) //Pour le ApplyRuleOnBlank (les pj de la fiche ouverte sont passés en param)
            {
                if (String.IsNullOrEmpty(strPjIds))
                    nPjCnt = 0;
                else
                    nPjCnt = strPjIds.Split(';').Length;
            }
            else
            {
                try
                {
                    error = ePJTraitements.PjListSelect(Pref, _myFile.ViewMainTable.DescId, _myFile.FileId, null, out _dicoLstPj);

                    if (error.Length > 0)
                        throw new Exception(error);

                    if (_dicoLstPj != null && _dicoLstPj.Count > 0)
                    {
                        foreach (var item in _dicoLstPj)
                            strPjIds = string.Concat(strPjIds, ";", item.Key);

                        strPjIds = strPjIds.Remove(0, 1);
                    }

                    nPjCnt = _dicoLstPj.Count;
                }
                catch (Exception ex)
                {
                    eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.ToString()), Pref);
                }
            }

            hidPj.Attributes.Add("PjIds", strPjIds);
            hidPj.Attributes.Add("nbpj", nPjCnt.ToString());
        }


        /// <summary>
        /// ajoute les liaisons parentes en pied de page
        /// </summary>
        protected virtual void AddParentInFoot()
        {

        }

        #endregion



        /// <summary>
        /// Prépare un tableau contenant les rubriques à afficher en signet et y deverse les lignes du tableau principal qui sont concernées
        /// </summary>
        /// <param name="fileTabBody"></param>
        /// <param name="nbColByLine"></param>
        /// <param name="nBreakLine"></param>
        protected override System.Web.UI.WebControls.Table SetHtmlTabInBkm(System.Web.UI.WebControls.Table fileTabBody, Int32 nbColByLine, Int32 nBreakLine)
        {
            if (_bPopupDisplay)
                return new System.Web.UI.WebControls.Table();
            else
                return base.SetHtmlTabInBkm(fileTabBody, nbColByLine, nBreakLine);
        }









        /// <summary>
        /// Retourne un titre séparateur de bloc system (occupe toute une ligne)
        /// </summary>
        /// <param name="nbCol"></param>
        /// <param name="sLabel"></param>
        /// <param name="sId"></param>
        /// <returns></returns>
        public TableRow GetSystemSeparator(Int32 nbCol, String sLabel, String sId)
        {
            TableRow myRow = new TableRow();
            TableCell myCell = new TableCell();
            myRow.Cells.Add(myCell);

            GetSeparator(myCell, null, sLabel, true);
            myCell.ID = sId;
            myCell.Attributes.Add("efld", "1");
            myCell.Attributes.Add("eaction", "LNKSEP");
            myCell.Attributes.Add("eOpen", "1");
            // TODO : taille de cellule à mettre en const
            myCell.Style.Add("height", "36px");
            myCell.ColumnSpan = nbCol * eConst.NB_COL_BY_FIELD;

            return myRow;
        }

        /// <summary>
        /// Barre d'outils (imprimer/...)
        /// </summary>
        /// <returns></returns>
        protected override Panel GetToolBar()
        {
            Panel pnlToolBar = new Panel();
            pnlToolBar.CssClass = "fileTools";

            HtmlGenericControl myUl = new HtmlGenericControl("ul");
            myUl.Attributes.Add("class", "outils");
            pnlToolBar.Controls.Add(myUl);

            // Annulation des dernières saisies #55703
            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.File_CancelLastEntries))
            {
                HtmlGenericControl myLi = new HtmlGenericControl("li");
                myLi.ID = String.Concat("btnCancelLastModif_", this._tab);
                myLi.Attributes.Add("class", "btnCancelLastModif icon-undo");
                myLi.Attributes.Add("title", eResApp.GetRes(Pref, 8223));
                myLi.Attributes.Add("onmouseover", String.Concat("LastValuesManager.openContextMenu(this, ", this._tab, ", arrLastValues);"));

                myUl.Controls.Add(myLi);
            }


            //Diminuer la taile du texte
            /* HtmlGenericControl myLi = new HtmlGenericControl("li");
             myLi.Attributes.Add("class", "icon-a_moins");
             myLi.Attributes.Add("title", eResApp.GetRes(Pref, 6188));
             myLi.Attributes.Add("onclick", "resizeFont(-2);");

             myUl.Controls.Add(myLi);

             //Grossir la taille du texte
             myLi = new HtmlGenericControl("li");
             myLi.Attributes.Add("class", "icon-a_plus");
             myLi.Attributes.Add("onclick", "resizeFont(2);");
             myLi.Attributes.Add("title", eResApp.GetRes(Pref, 6189));

             myUl.Controls.Add(myLi);*/

            return pnlToolBar;
        }

        /// <summary>
        /// Génère une table d'entête
        /// </summary>       
        /// <param name="bHasButtons"></param>
        protected override System.Web.UI.WebControls.Table GetHeader(Boolean bHasButtons = true)
        {

            System.Web.UI.WebControls.Table myTable = new System.Web.UI.WebControls.Table();
            if (_bPopupDisplay || (_myFile.ViewMainTable.TabType == TableType.ADR && _myFile.FileId == 0))
                return new System.Web.UI.WebControls.Table();


            myTable = base.GetHeader(bHasButtons);

            return myTable;
        }

        /// <summary>
        /// Adds the avatar cell on table.
        /// </summary>
        /// <param name="myTable">Table webcontrol</param>
        /// <param name="bCellOnly">Cell only</param>
        /// <returns></returns>
        protected override bool AddAvatarCellOnTable(System.Web.UI.WebControls.Table myTable, bool bCellOnly)
        {
            if (myTable == null)
                return false;

            if (myTable.Rows.Count == 0)
                return false;


            TableCell tc = new TableCell();
            //cadre Photo
            tc.ID = "vcCadre";
            tc.CssClass = "vcCadreFile";
            tc.RowSpan = myTable.Rows.Count;
            myTable.Rows[0].Cells.AddAt(0, tc);

            if (bCellOnly)
            {
                return false;
            }
            else
            {
                // Initialisation de la photo de la VCARD
                Panel vcPhoto = new Panel();
                String sFileName = string.Empty;

                //Record disponible
                if (_myFile != null && _myFile.Record != null)
                {
                    eFieldRecord f = _myFile.GetField(_myFile.ViewMainTable.DescId + (int)EudoQuery.AllField.AVATAR);

                    if (f != null && f.DisplayValue.Length > 0)
                        sFileName = f.DisplayValue;
                }

                //Modifiable seulement en modif
                // MCR SPH 40510 : ajout du parametre optionnel : bFromFile a  true, en mode fiche, 

                eTools.SetAvatar(vcPhoto, Pref, _tab, true, sFileName, _nFileId, true);


                tc.Controls.Add(vcPhoto);

                return true;
            }
        }

        protected override void AddTableRow(System.Web.UI.WebControls.Table maTable, TableRow myTr, Int32 nbMaxCols, Int32 y, bool bLineNotEmpty, bool bHasPageSep, bool bHasSep, bool bDoNotAddTR)
        {
            if ((bLineNotEmpty || bHasPageSep || bHasSep) && !bDoNotAddTR)
            {
                base.AddTableRow(maTable, myTr, nbMaxCols, y, bLineNotEmpty, bHasPageSep, bHasSep, bDoNotAddTR);
            }
        }

        /// <summary>
        /// Ajout de la breakline séparant le résumé
        /// </summary>
        /// <param name="td">Table cell</param>
        protected override void AddResumeBreakLine(System.Web.UI.WebControls.TableCell td)
        {
            //if (_numLine > -1)
            //{
            //    Panel div = new Panel();
            //    div.ID = "resumeBreakline";
            //    div.Attributes.Add("data-numline", _numLine.ToString());
            //    div.Attributes.Add("class", "resumeBreakline icon-caret-right");
            //    div.ToolTip = eResApp.GetRes(Pref, 8164);
            //    div.Style.Add("display", "none");
            //    td.Controls.Add(div);
            //}
        }


    }


}