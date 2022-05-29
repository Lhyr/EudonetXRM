using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu du mode impression d'une fiche planning
    /// </summary>
    public class ePrintPlanningFileRenderer : ePlanningFileRenderer
    {
        #region PROPRIETES

        private ePrintParams _nParamsOfPrint = null;
        #endregion


        #region constructeur
        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nTab">DescId de l'onglet</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nPrintParams">paramètres d'impression</param>
        public ePrintPlanningFileRenderer(ePref pref, Int32 nTab, Int32 nFileId, ePrintParams nPrintParams) : base(pref, nTab, nFileId, 0, 0, "", "", false, pref.UserId)
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
                    _sErrorMsg = String.Concat("ePrintPlanningFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
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
                _sErrorMsg = String.Concat("ePrintPlanningFileRenderer.Init ", Environment.NewLine, e.Message);
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

            // Entete de page  
            Panel _pnTopTitle = new Panel();
            _pnTopTitle.CssClass = "divHeadPage";
            _pnTopTitle.ID = "divHeadPage";

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
            if (ednType != EdnType.FILE_STANDARD && ednType != EdnType.FILE_PLANNING) // TODO: A compléter lorsque l'impression sera ajoutée à d'autres types de table
            {
                //Contenu des signets en mode Impression
                eRenderer rendererBkm = eRendererFactory.CreateBookmarkListRenderer(Pref, _myFile, true, true, true);
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




        protected override void AddPlanningParent(Panel container)
        {
            eRenderer footRenderer = new ePrintFileParentInFootRenderer(Pref, this);
            footRenderer.Generate();
            Panel pgC = null;
            if (footRenderer.ErrorMsg.Length > 0)
            {
                this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
            }
            if (footRenderer != null)
                pgC = footRenderer.PgContainer;

            container.Controls.Add(pgC);
        }

    }
}