using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// calsse de rendu du mode impression
    /// </summary>
    public class ePrintFileRenderer : eMainFileRenderer
    {
        #region PROPRIETES

        private ePrintParams _nParamsOfPrint = null;
        #endregion


        #region constructeur
        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab">The tab.</param>
        /// <param name="nFileId">The file identifier.</param>
        /// <param name="nPrintParams">paramètres d'impression</param>
        public ePrintFileRenderer(ePref pref, Int32 nTab, Int32 nFileId, ePrintParams nPrintParams)
        {
            Pref = pref;
            _tab = nTab;
            _nFileId = nFileId;
            _nParamsOfPrint = nPrintParams;
            _rType = RENDERERTYPE.PrintFile;
        }


        #endregion

        #region méthodes de rendu
        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {
                _myFile = eFileMain.CreateMainFile(Pref, _tab, _nFileId, ActiveBkm.DISPLAYALL.GetHashCode(), null, true);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("ePrintFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
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
                _sErrorMsg = String.Concat("ePrintFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_BKM_NOT_LINKED;
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
            Boolean bReturn = false;

            #region Entete de page

            // Partie inutile car remplacée dans base.Build();

            // Entete de page  
            Panel _pnTopTitle = new Panel();
            _pnTopTitle.CssClass = "divHeadPage";
            _pnTopTitle.ID = "divHeadPage";

            Panel btnPrint = new Panel();
            btnPrint.ID = "btnPrint";
            HtmlGenericControl spanPrint = new HtmlGenericControl();
            spanPrint.InnerText = eResApp.GetRes(Pref, 13);
            btnPrint.Controls.Add(spanPrint);
            btnPrint.Attributes.Add("onclick", "window.print();");
            _pnTopTitle.Controls.Add(btnPrint);

            Literal liTitle = new Literal();
            liTitle.Text = _nParamsOfPrint.TopTitlePage;
            _pnTopTitle.Controls.Add(liTitle);

            PgContainer.Controls.Add(_pnTopTitle);

            #endregion

            #region Titre de la page ou du rapport d'impression

            // Entete de page  
            _pnTopTitle = new Panel();
            _pnTopTitle.CssClass = "divPrintTitle";
            _pnTopTitle.ID = "divPrintTitle";

            liTitle = new Literal();
            liTitle.Text = _nParamsOfPrint.Title;
            _pnTopTitle.Controls.Add(liTitle);

            PgContainer.Controls.Add(_pnTopTitle);

            #endregion

            bReturn = base.Build();

            #region Pied de Page

            // Pied de page 
            Panel _pnButtomTitle = new Panel();
            _pnButtomTitle.CssClass = "divButtomPage";
            _pnButtomTitle.ID = "divButtomPage";

            Literal buttomTitle = new Literal();
            buttomTitle.Text = _nParamsOfPrint.ButtomTitlePage;
            _pnButtomTitle.Controls.Add(buttomTitle);

            PgContainer.Controls.Add(_pnButtomTitle);

            #endregion


            return bReturn;
        }

        #endregion

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        protected override void GetHTMLMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            // [BUG XRM] Impression fiche avec champ Mémo HTML - #49286
            sValue = HtmlTools.StripHtml(sValue);

            WebControl webCtrl = ednWebCtrl.WebCtrl;

            HtmlGenericControl div = new HtmlGenericControl("div");
            div.InnerHtml = sValue;
            webCtrl.Controls.Add(div);
        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetRawMemoControl(EdnWebControl ednWebCtrl, string sValue)
        {
            // [BUG XRM] Impression fiche avec champ Mémo HTML - #49286
            sValue = HtmlTools.StripHtml(sValue);

            base.GetRawMemoControl(ednWebCtrl, sValue);
        }

        /// <summary>
        /// rend le block HTML de tous les signets dans une méthode overridable
        /// </summary>
        /// <returns></returns>
        protected override void GetBookMarkBlock()
        {
            EdnType ednType = _myFile.ViewMainTable.EdnType;
            if (ednType != EdnType.FILE_STANDARD
                && ednType != EdnType.FILE_PLANNING
                 && ednType != EdnType.FILE_TARGET
                && _myFile.ViewMainTable.TabType != TableType.ADR
                && ednType != EdnType.FILE_MAIL) // TODO: A compléter lorsque l'impression sera ajoutée à d'autres types de table
            {
                //Contenu des signets en mode Impression
                eRenderer rendererBkm = eRendererFactory.CreateBookmarkListRenderer(Pref, _myFile, true, true, true);
                if (rendererBkm != null && _backBoneRdr != null && _backBoneRdr.PnBkmContainer != null)
                    eTools.TransfertFromTo(rendererBkm.PgContainer, _backBoneRdr.PnBkmContainer);
            }
        }


        /// <summary>
        /// Prépare un tableau contenant les rubriques à afficher en signet et y deverse les lignes du tableau principal qui sont concernées
        /// </summary>
        /// <param name="fileTabBody"></param>
        /// <param name="nbColByLine"></param>
        /// <param name="nBreakLine"></param>
        protected override System.Web.UI.WebControls.Table SetHtmlTabInBkm(System.Web.UI.WebControls.Table fileTabBody, Int32 nbColByLine, Int32 nBreakLine)
        {
            return new System.Web.UI.WebControls.Table();
        }


        /// <summary>
        /// ajoute les liaisons parentes en pied de page
        /// </summary>
        protected override void AddParentInFoot()
        {
            // ajout du pied de page contenant les informations parentes en popup
            EdnType ednType = _myFile.ViewMainTable.EdnType;
            if (ednType == EdnType.FILE_STANDARD || ednType == EdnType.FILE_PLANNING || ednType == EdnType.FILE_MAIN)
            {
                eRenderer footRenderer = new ePrintFileParentInFootRenderer(Pref, this);
                footRenderer.Generate();
                Panel pgC = null;
                if (footRenderer.ErrorMsg.Length > 0)
                    this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
                if (footRenderer != null)
                    pgC = footRenderer.PgContainer;
                _backBoneRdr.PgContainer.Controls.Add(footRenderer.PgContainer);
            }
        }

        /// <summary>
        /// Adds the avatar cell on table.
        /// </summary>
        /// <param name="myTable">My table.</param>
        /// <param name="bCellOnly">if set to <c>true</c> [b cell only].</param>
        /// <returns></returns>
        protected override bool AddAvatarCellOnTable(System.Web.UI.WebControls.Table myTable, bool bCellOnly)
        {
            if (myTable == null)
                return false;

            if (myTable.Rows.Count == 0)
                return false;

            if (bCellOnly)
            {
                return false;
            }
            else
            {
                // Initialisation de la photo de la VCARD
                //Panel vcPhoto = new Panel();
                String sFileName = string.Empty;

                //Record disponible
                if (_myFile != null && _myFile.Record != null)
                {
                    eFieldRecord f = _myFile.GetField(_myFile.ViewMainTable.DescId + EudoQuery.AllField.AVATAR.GetHashCode());

                    if (f != null && f.DisplayValue.Length > 0)
                    {
                        TableCell tc = new TableCell();
                        //cadre Photo
                        tc.ID = "vcCadre";
                        tc.CssClass = "vcCadreFile";
                        tc.RowSpan = myTable.Rows.Count;
                        myTable.Rows[0].Cells.AddAt(0, tc);

                        sFileName = f.DisplayValue;

                        //Modifiable seulement en modif
                        // MCR SPH 40510 : ajout du parametre optionnel : bFromFile a  true, en mode fiche, 

                        //eTools.SetGooglePhoto(vcPhoto, Pref, _tab, _nFileId != 0 ? true : false, sFileName, true);

                        HtmlImage img = new HtmlImage();
                        img.Src = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName).TrimEnd('/'), "/", sFileName);
                        tc.Controls.Add(img);

                        //tc.Controls.Add(vcPhoto);

                        return true;
                    }


                }
                return false;

            }
        }

        /// <summary>
        /// Ajoute les relations d'adresse (PP/PM)
        /// </summary>
        /// <param name="myField">eFieldRecord</param>
        /// <param name="maTable">Contrôle Table</param>
        /// <returns></returns>
        protected override bool SetAddrProfLink(eFieldRecord myField, System.Web.UI.WebControls.Table maTable)
        {
            if (myField.FldInfo.Descid == TableType.ADR.GetHashCode() + 1)
            {

                eFieldRecord fldAdrPerso = _myFile.Record.GetFieldByAlias(String.Concat(TableType.ADR.GetHashCode(), "_", AdrField.PERSO.GetHashCode()));
                if (_myFile.FileId == 0)
                {
                    AddAddressLinks(maTable, TableType.PP, false);

                    if (fldAdrPerso.Value == "0" || (!fldAdrPerso.RightIsVisible && fldAdrPerso.FldInfo.DefaultValue == "0"))
                    {
                        AddAddressLinks(maTable, TableType.PM, true);
                        return true;
                    }
                }
                else
                {
                    myField.RightIsUpdatable = false;

                    AddAddressLinks(maTable, TableType.PP, true);
                }
            }
            return false;
        }

        /// <summary>
        /// rajoute les liaisons vers PP et PM sur address
        /// </summary>
        /// <param name="maTable"></param>
        /// <param name="tabTyp"></param>
        /// <param name="isVisible"></param>
        protected void AddAddressLinks(System.Web.UI.WebControls.Table maTable, TableType tabTyp, Boolean isVisible)
        {
            if (!(_myFile.ViewMainTable.TabType == TableType.ADR))
            {
                return;
            }

            eFieldRecord fldLink = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", tabTyp.GetHashCode() + 1));
            eFieldRecord fldAdr01 = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_401"));

            if (fldLink.FileId == 0 && tabTyp == TableType.PP)
                return;

            fldLink.FldInfo.ReadOnly = true;

            TableRow myTr = new TableRow();
            maTable.Rows.Add(myTr);
            if (tabTyp == TableType.PP && _myFile.FileId == 0)
                myTr.Style.Add("display", "none");

            TableCell myLabel = new TableCell();
            TableCell myValue = new TableCell();
            TableCell myButton = new TableCell();

            GetFieldLabelCell(myLabel, _myFile.Record, fldLink);

            myValue = (TableCell)GetFieldValueCell(_myFile.Record, fldLink, 0, Pref);
            //(eConst.NB_COL_BY_FIELD - 1) corresponds au nombre des cellules système associées : (label, boutons, etc.)
            myValue.ColumnSpan = fldAdr01.FldInfo.PosColSpan * eConst.NB_COL_BY_FIELD - (eConst.NB_COL_BY_FIELD - 1);


            myTr.Cells.Add(myLabel);
            myTr.Cells.Add(myValue);
        }

    }
}