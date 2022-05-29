using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;


namespace Com.Eudonet.Xrm
{


    /// <summary>
    /// Classe de rendu des bookmark
    /// </summary>
    public class eBookmarkRenderer : eListMainRenderer
    {


        /// <summary>
        /// Indique si on est en mode Impression 
        /// </summary>
        private bool _bPrintMode = false;
        /// <summary>
        /// eBookmark object
        /// </summary>
        protected eBookmark _bkm = null;

        #region CONSTRUCTEUR

        /// <summary>
        /// Création d'un BookmarkRenderer à partir d'un eBookmark chargé
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="eBkm"></param>
        /// <param name="bDisplayAll"></param>
        /// <param name="bPrintMode"></param>
        public eBookmarkRenderer(ePref ePref, eBookmark eBkm, bool bDisplayAll, bool bPrintMode)
            : base(ePref)
        {
            _tab = eBkm.ViewMainTable.DescId;
            _rows = eBkm.RowsByPage;
            if (eBkm.BkmPagingMode == EudoQuery.BkmPagingMode.PAG_NORM && eBkm.ListRecords != null)
                _rows = eBkm.ListRecords.Count;

            _list = eBkm;

            _page = eBkm.Page;
            _bPrintMode = bPrintMode;
            _rType = RENDERERTYPE.Bookmark;
        }


        /// <summary>
        /// Création d'un BookmarkRenderer à partir d'un descid TabNotDisplayedInV7
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="nParentTab"></param>
        /// <param name="nParentFileid"></param>
        /// <param name="nBkmDescId"></param>
        /// <param name="nPage"></param>
        public eBookmarkRenderer(ePref ePref, int nParentTab, int nParentFileid, int nBkmDescId, int nPage)
            : base(ePref)
        {
            _list = (eList)eBookmark.CreateBookmark(ePref, nParentTab, nParentFileid, nBkmDescId, nPage, 25, load: eBookmark.LoadMode.LOAD);
            _bkm = (eBookmark)_list;

            _rows = _list.RowsByPage;
            if (_bkm.BkmPagingMode == EudoQuery.BkmPagingMode.PAG_NORM)
                _rows = _list.ListRecords.Count;
            _tab = nParentTab;
            _page = nPage;
            _rType = RENDERERTYPE.Bookmark;
        }

        /// <summary>
        /// Instancie un renderer de bookmark
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="eBkm">Bookmark</param>
        private eBookmarkRenderer(ePref pref, eBookmark eBkm)
            : base(pref)
        {
            _list = (eList)eBkm;
            _tab = eBkm.CalledTabDescId;
            _rType = RENDERERTYPE.Bookmark;

            if (eBkm.ErrorMsg.Length > 0)
            {
                _sErrorMsg = eBkm.ErrorMsg;
                _nErrorNumber = eBkm.ErrorType;
            }

            if (eBkm.InnerException != null)
                _eException = eBkm.InnerException;

        }



        #endregion



        /// <summary>
        /// Retourne un bookmark pour la gestion d'erreur
        /// </summary>
        /// <param name="pref">Préférence user</param>
        /// <param name="eBkm">Bookmark</param>
        /// <returns></returns>
        public static eRenderer CreateErrorBookmarkRenderer(ePref pref, eBookmark eBkm)
        {
            eBookmarkRenderer er = new eBookmarkRenderer(pref, eBkm);
            er.CreateErrorHead();
            return er;
        }



        /// <summary>
        /// Correspond au descid de la table demandé pour la liste
        /// Dans certains cas, il ne s'agit pas d'un "vrai" table, par exemple pour
        /// les doublons...
        /// </summary>
        public override int VirtualMainTableDescId
        {
            get
            {
                return _bkm.CalledTabDescId;
            }
        }


        /// <summary>
        /// Génère l'objet _list du renderer
        /// Dans le cas des bkm la liste est générée avant le init, soit dans le construceur
        /// soit récupérer en paramètre du construteur
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {

        }




        /// <summary>
        /// initialise le renderer avant génération
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            try
            {
                if (!base.Init())
                    return false;

                if (_list == null)
                    throw new Exception("_list est null");

                _bkm = (eBookmark)_list;
                if (_bkm == null)
                    throw new Exception("_bkm est null");

                if (_bkm.BkmPagingMode == EudoQuery.BkmPagingMode.PAG_NORM)
                    _rows = _list?.ListRecords?.Count ?? 0;

                return true;
            }
            catch (Exception e)
            {
                this._sErrorMsg = string.Concat("eBookmarkRenderer.Init() : ", e.Message, Environment.NewLine, e.StackTrace);
                this._eException = e;

#if DEBUG
                eModelTools.EudoTraceLog("*************");

                eModelTools.EudoTraceLog(e.Message);
                eModelTools.EudoTraceLog(e.StackTrace);

                eModelTools.EudoTraceLog("*************");
#endif
                return false;

            }
        }

        /// <summary>
        /// builde de la classe parente
        /// </summary>
        /// <returns></returns>
        protected virtual bool BaseBuild()
        {
            if (_bkm.NbTotalRows > 0)
            {
                return base.Build();
            }
            else
            {
                SetEmptyBkmPanel(Pref, _bkm, _pgContainer, _bPrintMode);


            }
            return true;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {
                try
                {
                    _pgContainer.Controls.Add(CreateTitleBar(Pref, _bkm, !_bPrintmode));
                }
                catch (Exception e)
                {
                    _eException = e;
                    _sErrorMsg = string.Concat("eBookmarkRenderer.Build()>eBookmarkRenderer.CreateTitleBar : ", Environment.NewLine,
                        "bkm: ", _bkm?.CalledTabDescId.ToString() ?? "null", ", _bPrintmode : ", _bPrintmode, Environment.NewLine,
                        e.Message, Environment.NewLine,
                        e.StackTrace, Environment.NewLine);
                    return false;
                }

                //if (_bkm.RelationFieldDescid > 0)
                //    _pgContainer.Attributes.Add("spclnk", _bkm.RelationFieldDescid.ToString());

                return BaseBuild();
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = e.Message;
                return false;
            }

        }

        internal static void SetEmptyBkmPanel(ePref pref, eBookmark bkm, Panel pgContainer, bool bPrintmode = false)
        {
            Literal ltl = new Literal();
            HyperLink hl = new HyperLink();
            hl.CssClass = "gofile linkAddNewBkmFile";
            bool bDisableExprFlt = false;

            pgContainer.Attributes.Add("nofile", "1");

            if (!string.IsNullOrEmpty(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL)))
            {
                bDisableExprFlt = true;
            }

            if (bkm.ViewMainTable.PermAdd && bkm.IsAddAllowed && !bPrintmode && bkm.IsAddAllowedInRender)
            {
                if (bkm.ViewMainTable.TabType == EudoQuery.TableType.PJ)
                {
                    ltl.Text = eResApp.GetRes(pref, 122);
                }
                else
                {
                    ltl.Text = eResApp.GetRes(pref, 56);
                }

                hl.Attributes.Add("onclick", GetJsAddFunction(pref, bkm));

                hl.Controls.Add(ltl);
                pgContainer.Controls.Add(hl);
            }
            else
            {
                Panel pnl = new Panel();

                pnl.CssClass = "noFileInBkm";

                if (bkm.CalledTabDescId == (int)TableType.BOUNCEMAIL && bkm.ParentTab == (int)TableType.CAMPAIGN)
                    ltl.Text = eResApp.GetRes(pref, 2912);
                else
                    ltl.Text = eResApp.GetRes(pref, 83);

                if (bDisableExprFlt)
                {
                    hl.Controls.Add(ltl);
                    pgContainer.Controls.Add(hl);

                }
                else
                {
                    pnl.Controls.Add(ltl);
                    pgContainer.Controls.Add(pnl);
                }
            }

            if (!bPrintmode)
            {
                if (bDisableExprFlt)
                {
                    ltl.Text = eResApp.GetRes(pref, 6369);
                    // HLA - Bug #24495- Cela fait des appels à eSelectionManager.ashx pour chaque colonnes -> 
                    //      update sur PREF -> super performance -> pas de conservation des resize sur les colonnes -> perte pref des users !
                    hl.Attributes.Add("onclick", $"updateUserBkmPref('tab={bkm.ParentTab};$;bkm={bkm.CalledTabDescId};$;filterExpress=$cancelallexpressfilters$', function () {{ loadBkm({bkm.CalledTabDescId}, 1); }});");
                }
            }
        }

        /// <summary>
        /// Surcharge de la méthode End: on modifie l'attribut ednMode qu'on affecte à bkm
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {


            //S'il n'y a pas d'élément dans le signet, le build n'a pas été fait et du coup, le fillcontenair de eListMainRenderer non plus
            // du cous _divmt n'est pas construit
            if (_divmt != null)
            {

                if (!_bkm.IsUpdateAllowed)
                    _divmt.Attributes.Add("ro", "1");

                _divmt.Attributes.Add("sup", _bkm.IsDeleteAllowed ? "1" : "0");
            }

            if (_list.NbTotalRows > 0)
            {

                bool bRes = base.End();
                if (!bRes)
                    return false;


                _tblMainList.Attributes["ednmode"] = "bkm";

                if (_bkm.BkmEdnType == EdnType.FILE_RELATION)
                {
                    _tblMainList.Attributes["ednrel"] = _bkm.ViewMainTable.DescId.ToString();
                }

                return true;

            }
            else
                return true;
        }

        /// <summary>
        /// la liste ne doit être complétée que dans le mode liste et non dans le mode signet.
        /// </summary>
        /// <param name="idxLine"></param>
        protected override void CompleteList(int idxLine)
        {
            idxLine = _rows;
        }



        /// <summary>
        /// permet de créer une barre des titres de signet  y compris sur les signets Mémo et Page Web
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="bkm"></param>
        /// <param name="bDisplayButtons">Indique s'il faut afficher les boutons d'actions (Ajouter, Rubriques etc.) false par défaut</param>
        /// <param name="sLibelle">Libellé du signet, inutile si le signet est renseigné</param>
        /// <param name="bZoomButton">indique s'il faut afficher le bouton permettant de zoomer sur le signet, false par défaut</param>
        /// <param name="sDivId">id de la div contenant tout le signet, inutile si bZoomButton est à false</param>
        /// <param name="pW">proportion horizontale de l'écran à occuper</param>
        /// <param name="pH">proportion verticale de l'écran à occuper</param>
        /// <returns></returns>
        public static Panel CreateTitleBar(ePref pref, eBookmark bkm = null, bool bDisplayButtons = false, string sLibelle = "", bool bZoomButton = false, string sDivId = "", int pW = 0, int pH = 0)
        {

            try
            {

                ePrefConst.BKMVIEWMODE viewMode = ePrefConst.BKMVIEWMODE.LIST;
                if (bkm != null && bkm.BkmPref != null)
                {
                    // #44368 CRU : On force le bkmviewmode à LIST pour les signets de type relationnel
                    if (bkm.BkmEdnType == EdnType.FILE_RELATION)
                        viewMode = ePrefConst.BKMVIEWMODE.LIST;
                    else
                        viewMode = (ePrefConst.BKMVIEWMODE)eLibTools.GetNum(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE));
                }

                Panel pnTitle = new Panel();

                HtmlGenericControl ulTitleBar = new HtmlGenericControl("ul");
                pnTitle.Controls.Add(ulTitleBar);
                ulTitleBar.Attributes.Add("width", "100%");

                HtmlGenericControl liTitle = new HtmlGenericControl("li");
                liTitle.Attributes.Add("class", "bkmLabel");
                ulTitleBar.Controls.Add(liTitle);
                pnTitle.CssClass = "bkmTitle";

                // Compteur
                if (bkm != null)
                {
                    HtmlGenericControl spNbCnt = new HtmlGenericControl("span");
                    liTitle.ID = string.Concat("bkmLabel_", bkm.CalledTabDescId);
                    pnTitle.ID = string.Concat("bkmTitle_", bkm.CalledTabDescId);

                    spNbCnt.ID = string.Concat("bkmCnt_", bkm.CalledTabDescId);
                    Literal liBkmCnt = new Literal();
                    spNbCnt.Controls.Add(liBkmCnt);
                    liTitle.Controls.Add(spNbCnt);
                }

                #region Ajout du nom du bookmark
                Literal liBkmTitle = new Literal();
                liTitle.Controls.Add(liBkmTitle);

                #region Libellé du signet
                if (sLibelle.Length > 0)
                {
                    liBkmTitle.Text = sLibelle;

                }
                else if (bkm != null && bkm.IsMemo)
                {
                    bool bNotFound = false;
                    eRes res = new eRes(pref, bkm.ViewTabDescId.ToString());
                    liBkmTitle.Text = res.GetRes(bkm.ViewTabDescId, out bNotFound);
                }
                else if (bkm != null && bkm.ViewTabDescId == EudoQuery.TableType.PJ.GetHashCode())
                {
                    bool bFound = false;
                    eRes res = new eRes(pref, (bkm.ParentTab + AllField.ATTACHMENT.GetHashCode()).ToString());
                    liBkmTitle.Text = res.GetRes((bkm.ParentTab + AllField.ATTACHMENT.GetHashCode()), out bFound);
                    if (!bFound)
                    {
                        liBkmTitle.Text = bkm.ViewMainTable.Libelle;
                    }

                }
                else if (bkm != null && bkm.ViewMainTable != null && !string.IsNullOrEmpty(bkm.ViewMainTable.Libelle))
                {
                    liBkmTitle.Text = bkm.ViewMainTable.Libelle;
                }

                liBkmTitle.Text = HttpUtility.HtmlEncode(liBkmTitle.Text);
                #endregion
                #endregion

                if (bkm != null)
                {

                    #region Ajout du filtre compteur du signet
                    HtmlGenericControl spanFilter = new HtmlGenericControl("span");
                    spanFilter.ID = string.Concat("bkmCntFilter_", bkm.CalledTabDescId);
                    Literal litBkmFilter = new Literal();
                    spanFilter.Controls.Add(litBkmFilter);
                    liTitle.Controls.Add(spanFilter);
                    #endregion




                    if (bkm.BkmPref != null && !string.IsNullOrEmpty(bkm.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.BKMFILTERCOL)))
                    {
                        HtmlGenericControl spRemFilt = new HtmlGenericControl("span");
                        spRemFilt.Attributes.Add("class", "icon-rem_filter");
                        spRemFilt.Attributes.Add("title", eResApp.GetRes(pref, 1179));
                        liTitle.Controls.Add(spRemFilt);
                        spRemFilt.Attributes.Add("onclick", $"updateUserBkmPref('tab={bkm.ParentTab};$;bkm={bkm.CalledTabDescId};$;filterExpress=$cancelallexpressfilters$', function () {{ loadBkm({bkm.CalledTabDescId}, 1); }});");
                    }
                }



                if (bkm != null && bkm.ParentTab == EudoQuery.TableType.PM.GetHashCode() && bkm.ViewTabDescId == EudoQuery.TableType.ADR.GetHashCode())
                {
                    eRes resPP = new eRes(pref, bkm.ViewTabDescId.ToString());
                    liTitle.Attributes.Add("bkmtitle", resPP.GetRes(bkm.ViewTabDescId));
                }
                else
                    liTitle.Attributes.Add("bkmtitle", liBkmTitle.Text);

                #region Signet discussion : Tri descendant ascendant
                if (bkm != null && bkm.ViewMainTable != null && bkm.ViewMainTable.EdnType == EdnType.FILE_DISCUSSION)
                {

                    SortOrder sort = SortOrder.DESC;
                    if (bkm.DiscCustomFields.DateFld != null)
                        sort = bkm.DiscCustomFields.DateFld.SortInfo;


                    HtmlImage imgAsc = new HtmlImage();
                    imgAsc.Src = "ghost.gif";
                    imgAsc.Style.Add(HtmlTextWriterStyle.BorderWidth, "0px");
                    imgAsc.Attributes.Add("class", string.Concat("rIco SortAsc", sort == SortOrder.DESC ? " sorthid" : ""));
                    imgAsc.Attributes.Add("onclick", "sdo(this)");
                    imgAsc.ID = string.Concat("IMG_SORT_ASC_", bkm.ViewMainTable.DescId, "_", bkm.DiscCustomFields.DateDescId);
                    liTitle.Controls.Add(imgAsc);

                    HtmlImage imgDesc = new HtmlImage();
                    imgDesc.Src = "ghost.gif";
                    imgDesc.Style.Add(HtmlTextWriterStyle.BorderWidth, "0px");
                    imgDesc.Attributes.Add("class", string.Concat("rIco SortDesc", sort == SortOrder.ASC ? " sorthid" : ""));
                    imgDesc.Attributes.Add("onclick", "sdo(this)");
                    imgDesc.ID = string.Concat("IMG_SORT_DESC_", bkm.ViewMainTable.DescId, "_", bkm.DiscCustomFields.DateDescId);
                    liTitle.Controls.Add(imgDesc);

                }
                #endregion

                #region Mode de paging
                // Le contrôle est toujours créé pour les besoins de mise en forme du signet, même si les boutons de paging ne sont pas affichés
                HtmlGenericControl liPaging = new HtmlGenericControl("li");
                liPaging.Attributes.Add("class", "bkmPagingBar");
                ulTitleBar.Controls.Add(liPaging);

                if (bkm != null && (bkm.BkmPagingMode == EudoQuery.BkmPagingMode.PAG_INLINE || viewMode == ePrefConst.BKMVIEWMODE.FILE))
                {
                    string jsKeyWord = viewMode == ePrefConst.BKMVIEWMODE.FILE ? "File" : "";
                    HtmlGenericControl ulPagingButtons = new HtmlGenericControl("ul");
                    liPaging.Controls.Add(ulPagingButtons);

                    // Paging activé

                    int nPage = 0, nTotalPages = 0;
                    if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                    {
                        nPage = bkm.BkmFilePos;         // +(bkm.RowsByPage * (bkm.Page - 1));
                        nTotalPages = bkm.NbTotalRows;
                    }
                    else
                    {
                        nPage = bkm.Page;
                        nTotalPages = bkm.NbPage;
                    }

                    nPage = nTotalPages == 0 ? nTotalPages : nPage;


                    if ((viewMode != ePrefConst.BKMVIEWMODE.FILE || bkm.BkmFilePos > -1) && !bkm.PrintMode)
                    {
                        HtmlGenericControl liFirst = new HtmlGenericControl("li");
                        liFirst.Attributes.Add("width", "20px");
                        liFirst.Attributes.Add("class", string.Concat("icon-edn-first", nPage > 1 ? "" : " icnBkmDis", " icnBkm"));
                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            liFirst.Attributes.Add("onclick", string.Concat("firstFileBkm(", bkm.CalledTabDescId, ")"));
                        }
                        else if (bkm.Page > 1)
                        {
                            liFirst.Attributes.Add("onclick", string.Concat("firstPageBkm(", bkm.CalledTabDescId, ")"));
                        }

                        ulPagingButtons.Controls.Add(liFirst);

                        HtmlGenericControl tcPrev = new HtmlGenericControl("li");
                        tcPrev.Attributes.Add("width", "15px");
                        tcPrev.Attributes.Add("class", string.Concat("icon-edn-prev", nPage > 1 ? "" : " icnBkmDis", " icnBkm"));
                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            tcPrev.Attributes.Add("onclick", string.Concat("prevFileBkm(", bkm.CalledTabDescId, ")"));
                        }
                        else if (bkm.Page > 1)
                        {
                            tcPrev.Attributes.Add("onclick", string.Concat("prevPageBkm(", bkm.CalledTabDescId, ")"));
                        }
                        ulPagingButtons.Controls.Add(tcPrev);



                        HtmlGenericControl tcNumPage = new HtmlGenericControl("li");
                        tcNumPage.Attributes.Add("class", "numpage");
                        tcNumPage.Attributes.Add("align", "center");
                        tcNumPage.Attributes.Add("width", "4%");
                        ulPagingButtons.Controls.Add(tcNumPage);


                        HtmlInputText inptNumPage = new HtmlInputText();
                        tcNumPage.Controls.Add(inptNumPage);
                        inptNumPage.ID = string.Concat("bkmNum", jsKeyWord, "Page_", bkm.CalledTabDescId);
                        inptNumPage.Attributes.Add("class", "pagInput");
                        inptNumPage.Size = 2;
                        inptNumPage.Value = nPage.ToString();

                        inptNumPage.Attributes.Add("onblur", string.Concat("selectBkm", jsKeyWord, "Page(", bkm.CalledTabDescId, ", this)"));
                        inptNumPage.Attributes.Add("onkeypress", string.Concat("if (isValidationKey(event)) { selectBkm", jsKeyWord, "Page(", bkm.CalledTabDescId, ", this); }"));

                        HtmlGenericControl tcTotalPage = new HtmlGenericControl("li");
                        ulPagingButtons.Controls.Add(tcTotalPage);
                        Panel divTotalPage = new Panel();
                        tcTotalPage.Controls.Add(divTotalPage);
                        tcTotalPage.Attributes.Add("width", "4%");
                        divTotalPage.ID = string.Concat("bkmTotalNumPage_", bkm.CalledTabDescId);

                        Literal liNbPage = new Literal();
                        liNbPage.Text = string.Concat("&nbsp;/&nbsp;", viewMode == ePrefConst.BKMVIEWMODE.FILE ? bkm.NbTotalRows : bkm.NbPage);
                        divTotalPage.Controls.Add(liNbPage);


                        HtmlGenericControl tcNext = new HtmlGenericControl("li");
                        tcNext.Attributes.Add("width", "15px");
                        tcNext.Attributes.Add("class", string.Concat("icon-edn-next", nPage < nTotalPages ? "" : " icnBkmDis", " icnBkm"));
                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            tcNext.Attributes.Add("onclick", string.Concat("nextFileBkm(", bkm.CalledTabDescId, ")"));
                        }
                        else if (bkm.Page < bkm.NbPage)
                        {
                            tcNext.Attributes.Add("onclick", string.Concat("nextPageBkm(", bkm.CalledTabDescId, ")"));
                        }

                        ulPagingButtons.Controls.Add(tcNext);

                        HtmlGenericControl tcLast = new HtmlGenericControl("li");
                        tcLast.Attributes.Add("width", "20px");
                        tcLast.Attributes.Add("class", string.Concat("icon-edn-last", nPage < nTotalPages ? "" : " icnBkmDis", " icnBkm"));
                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            tcLast.Attributes.Add("onclick", string.Concat("lastFileBkm(", bkm.CalledTabDescId, ")"));
                        }
                        else if (bkm.Page < bkm.NbPage)
                        {
                            tcLast.Attributes.Add("onclick", string.Concat("lastPageBkm(", bkm.CalledTabDescId, ")"));
                        }

                        ulPagingButtons.Controls.Add(tcLast);

                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            pnTitle.Attributes.Add("lstid", eLibTools.Join<int>(";", bkm.GetIdsList()));
                            pnTitle.Attributes.Add("pg", bkm.Page.ToString());
                            pnTitle.Attributes.Add("nbpg", bkm.NbPage.ToString());
                            pnTitle.Attributes.Add("rpp", bkm.RowsByPage.ToString());
                        }
                    }
                    if (bkm.IsViewFilePossible)
                        pnTitle.Attributes.Add("vm", viewMode.GetHashCode().ToString());

                }

                #endregion

                HtmlGenericControl liActions = new HtmlGenericControl("li");
                liActions.Attributes.Add("class", "bkmButtonsBar");
                ulTitleBar.Controls.Add(liActions);


                #region Boutons d'actions (Ajout, Rubriques, Ajout depuis un filtre, historique, etc.)

                Panel pnButtonsBar = new Panel();
                HtmlGenericControl tbButtonsBar = new HtmlGenericControl("ul");

                if (bDisplayButtons || bZoomButton)
                {
                    pnButtonsBar = new Panel();
                    liActions.Controls.Add(pnButtonsBar);
                    pnButtonsBar.CssClass = "bkmButtonsBar";

                    pnButtonsBar.Controls.Add(tbButtonsBar);

                    if (bDisplayButtons && bkm != null)
                    {
                        HtmlGenericControl myLi = null;
                        HtmlGenericControl myImgDiv = null;

                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE && eFeaturesManager.IsFeatureAvailable(pref, eConst.XrmFeature.File_CancelLastEntries))
                        {
                            myLi = new HtmlGenericControl("li");
                            myLi.ID = string.Concat("btnCancelLastModif_", bkm.CalledTabDescId);
                            myLi.Attributes.Add("class", "btnCancelLastModif icon-undo");
                            myLi.Attributes.Add("title", eResApp.GetRes(pref, 8223));
                            myLi.Attributes.Add("onmouseover", string.Concat("LastValuesManager.openContextMenu(this, ", bkm.CalledTabDescId, ", arrLastValues);"));
                            tbButtonsBar.Controls.Add(myLi);
                        }

                        //Supression des divs et img obsolètes pour l'intégration des fonts-icon - PNO
                        #region Ajout (+)

                        if (bkm.ViewMainTable.PermAdd && bkm.IsAddAllowed && bkm.IsAddAllowedInRender)
                        {
                            myLi = new HtmlGenericControl("li");
                            myLi.Attributes.Add("class", "icon-add icnBkm");
                            myLi.Attributes.Add("title", eResApp.GetRes(pref, 31));
                            myLi.Attributes.Add("onclick", GetJsAddFunction(pref, bkm));
                            tbButtonsBar.Controls.Add(myLi);
                        }
                        #endregion

                        bool multiAddDel = bkm.ViewMainTable.AdrJoin
                            && ((eBookmark)bkm).ParentTab != TableType.PP.GetHashCode()
                            && ((eBookmark)bkm).ParentTab != TableType.PM.GetHashCode()
                            && bkm.RelationFieldDescid == 0
                            && bkm.ViewMainTable.EdnType != EdnType.FILE_VOICING
                            && bkm.ViewMainTable.EdnType != EdnType.FILE_MAIL
                            && bkm.ViewMainTable.EdnType != EdnType.FILE_HISTO
                            && bkm.ViewMainTable.EdnType != EdnType.FILE_RELATION;

                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            myLi = new HtmlGenericControl("li");
                            myLi.ID = string.Concat("bkmButtonsBar_", bkm.CalledTabDescId, "_delete");
                            myLi.Attributes.Add("class", "icon-delete icnBkm");
                            myLi.Attributes.Add("onclick", "deleteBkmCurrentFile(" + bkm.ViewMainTable.DescId + ");");
                            myLi.Attributes.Add("title", eResApp.GetRes(pref, 19));
                            tbButtonsBar.Controls.Add(myLi);

                        }
                        else
                        {
                            #region Historique

                            if (bkm.HistoInfo.Has)
                            {
                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-edn-history icnBkm");


                                if (bkm.HistoInfo.Actived)
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 6216));
                                else
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 6217));

                                myLi.Attributes.Add("enabled", (bkm.HistoInfo.Actived ? "1" : "0"));

                                //if (_list.ViewMainTable.PermAdd && (((eBookmark)_list).IsAddAllowed))
                                myLi.Attributes.Add("onclick", "onChngBkmHisto(" + bkm.CalledTabDescId + ");");

                                tbButtonsBar.Controls.Add(myLi);

                            }

                            #endregion

                            #region Mettre en pause les prochaines étapes
                            //tache #3 306 ajouter un bouton "Mettre en pause..."
                            if ( /*bkm.ParentFile.ViewMainTable.TabType == TableType.CAMPAIGN ||*/ bkm.ParentFile.ViewMainTable.TabType == TableType.EVENT)
                            {
                                int eventStepDescId = 0;
                                int onBreak = -1;
                                /*  ePrefLite epref = new ePrefLite(pref.GetSqlInstance,pref.GetBaseName,
                                      pref.GetSqlUser,pref.GetSqlPassword,pref.GetSqlApplicationName,
                                      pref.UserId,pref.Lang,pref.GroupMode,pref.CultureInfo);*/
                                eventStepDescId = eLibTools.LoadAndGetAdvValue(pref, bkm.CalledTabDescId,
                                    DESCADV_PARAMETER.EVENT_STEP_DESCID);



                                if (eventStepDescId != 0)
                                {
                                    if (bkm.ParentFile.ViewMainTable.TabType == TableType.CAMPAIGN)
                                    {
                                        if (bkm.ParentFile != null)
                                            onBreak = ((eRecordCampaign)bkm.ParentFile.Record).OnBreak;
                                        else
                                            throw new Exception("eBookmarkRenderer : ParentFile est null");
                                    }

                                    else
                                    {

                                        if (bkm.ParentFile != null)
                                        {
                                            if (!bkm.ParentFile.ViewMainTable.IsEventStep)
                                                onBreak = ((eRecordEvent)bkm.ParentFile.Record).OnBreak;
                                        }
                                        else
                                            throw new Exception("eBookmarkRenderer : ParentFile est null");
                                    }


                                    if (onBreak == 0)
                                    {
                                        myLi = new HtmlGenericControl("li");
                                        myLi.Attributes.Add("class", "icon-pause icnBkm");
                                        myLi.Attributes.Add("title", eResApp.GetRes(pref, 2689));
                                        if (bkm.ParentFile.ViewMainTable.TabType == TableType.CAMPAIGN)
                                            myLi.Attributes.Add("onclick", string.Concat("onBreak(", bkm.ParentFile.FileId, ",1);"));
                                        else
                                            myLi.Attributes.Add("onclick", string.Concat("onBreakEvent(", bkm.ParentTab, ",", bkm.ParentFile.FileId, ",1);"));

                                        myImgDiv = new HtmlGenericControl("div");

                                        tbButtonsBar.Controls.Add(myLi);
                                    }
                                    else
                                    {
                                        myLi = new HtmlGenericControl("li");
                                        myLi.Attributes.Add("class", "icon-play-circle-o icnBkm");
                                        myLi.Attributes.Add("title", eResApp.GetRes(pref, 2690));
                                        if (bkm.ParentFile.ViewMainTable.TabType == TableType.CAMPAIGN)
                                            myLi.Attributes.Add("onclick", string.Concat("onBreak(", bkm.ParentFile.FileId, ",0);"));
                                        else
                                            myLi.Attributes.Add("onclick", string.Concat("onBreakEvent(", bkm.ParentTab, ",", bkm.ParentFile.FileId, ",0);"));

                                        myImgDiv = new HtmlGenericControl("div");

                                        tbButtonsBar.Controls.Add(myLi);
                                    }
                                }
                            }

                            #endregion

                            #region ++ et xx

                            // Droit d'ajout
                            // Droit d'ajout sur ++
                            if (bkm.ViewMainTable.PermAdd && bkm.IsAddAllowed
                                && bkm.ViewMainTable.PermMultiFromFilter
                                && multiAddDel && bkm.IsAddAllowedInRender)
                            {
                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-add_filter icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 428));
                                myLi.Attributes.Add("enabled", "1");
                                myLi.Attributes.Add("onclick", string.Concat("ActionFromFilter(", bkm.ViewMainTable.DescId.ToString(), ",'", HttpUtility.JavaScriptStringEncode(bkm.ViewMainTable.Libelle), "', 0);"));

                                tbButtonsBar.Controls.Add(myLi);

                                if (bkm.ViewMainTable.PermDelete)
                                {
                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", "icon-del_filter icnBkm");
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 529));
                                    myLi.Attributes.Add("enabled", "1");
                                    myLi.Attributes.Add("onclick", string.Concat("ActionFromFilter(", bkm.ViewMainTable.DescId.ToString(), ",'", HttpUtility.JavaScriptStringEncode(bkm.ViewMainTable.Libelle), "', 1);"));



                                    tbButtonsBar.Controls.Add(myLi);

                                }
                            }

                            #endregion

                            #region Sélections carto

                            if (bkm.ViewMainTable.EdnType != EdnType.FILE_DISCUSSION)
                            {
                                if (bkm.ViewMainTable.SelectionSourceTab != 0)
                                {
                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", "icon-site_web icnBkm");
                                    myLi.Attributes.Add("title", "Ajouter à partir des critères de la fiche"); // TODO : RES
                                    myLi.Attributes.Add("onclick", string.Concat("openSelectionByFiltersModal(", bkm.ViewMainTable.SelectionSourceTab, ", ", bkm.ViewMainTable.DescId, ", '", bkm.Libelle, "');"));
                                    tbButtonsBar.Controls.Add(myLi);
                                }

                            }

                            #endregion

                            #region Ajout rubriques

                            if (bkm.ViewMainTable.EdnType != EdnType.FILE_DISCUSSION)
                            {

                                //Choix des rubriques
                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-rubrique icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 20));
                                myLi.Attributes.Add("onclick", string.Concat("setBkmCol(", bkm.CalledTabDescId, ");"));

                                myImgDiv = new HtmlGenericControl("div");

                                tbButtonsBar.Controls.Add(myLi);


                                //Afficher tout (uniquement si le paging est nécessaire)
                                if ((bkm.ListRecords?.Count ?? 0) > bkm.RowsByPage)
                                {
                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", ((bkm.BkmPagingMode != EudoQuery.BkmPagingMode.PAG_NORM) ? "bkmNoPaging icon-edn-eye" : "bkmPaging icon-edn-eye") + " icnBkm");
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref,
                                        (bkm.BkmPagingMode != EudoQuery.BkmPagingMode.PAG_NORM) ? 777 : 6758
                                        ));
                                    myLi.Attributes.Add("onclick", string.Concat("loadBkm(", bkm.CalledTabDescId, ", 1, false, ", ((bkm.BkmPagingMode != EudoQuery.BkmPagingMode.PAG_NORM) ? "true" : "false"), ");"));

                                    myImgDiv = new HtmlGenericControl("div");

                                    tbButtonsBar.Controls.Add(myLi);

                                }

                            }

                            #endregion

                            #region Cibles étendues - TODO bTplInvitation de la page eqlist.asp

                            // TODO voir les spec si on inclut Cible etendu dans la vérification des droits
                            if (bkm.ViewMainTable.ProspectEnabled)
                            {
                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-import icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 6340));
                                myLi.Attributes.Add("onclick", string.Concat("importTargets(", bkm.ParentTab, ",", bkm.ParentFileId, ",", bkm.CalledTabDescId, ");"));

                                myImgDiv = new HtmlGenericControl("div");

                                tbButtonsBar.Controls.Add(myLi);
                            }


                            // L'import des données sur le signet tient compte aussi des droits d'ajout et de modification
                            else if (bkm.IsImportAllowed && bkm.IsAddAllowed && bkm.IsUpdateAllowed &&
                                     eFeaturesManager.IsFeatureAvailable(pref, eConst.XrmFeature.Import))
                            {


                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-import icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 6340));
                                myLi.Attributes.Add("onclick", string.Concat("oImportWizard.ShowBkmWizard(", bkm.ParentTab, ",", bkm.ParentFileId, ",", bkm.CalledTabDescId, ");"));

                                myImgDiv = new HtmlGenericControl("div");

                                tbButtonsBar.Controls.Add(myLi);
                            }


                            #endregion
                        }

                        #region Export/publipostage pour signet EVENT, PP, PM, ADR et Template

                        bool bExport = (bkm.ViewMainTable.TabType == TableType.EVENT)
                            || (bkm.ViewMainTable.TabType == TableType.PM)
                            || (bkm.ViewMainTable.TabType == TableType.PP)
                            || (bkm.ViewMainTable.TabType == TableType.ADR)
                            || (bkm.ViewMainTable.TabType == TableType.TEMPLATE)
                            || (bkm.ViewMainTable.TabType == TableType.CAMPAIGNSTATS)
                            || (bkm.ViewMainTable.TabType == TableType.CAMPAIGNSTATSADV);

                        bExport = bExport && bkm.ViewMainTable.EdnType != EdnType.FILE_DISCUSSION;

                        if (bExport)
                        {

                            //Export mode signet
                            //[Bug-Export MOU #35536] Le button d'export affiché sur la bar du chaque signet, doit vérifier les droits de traitement
                            //sur tous les types d'analyse (impression, EXPORT, graphique, publipostage) pas seulement l'export. 

                            //Gestion des droits de traitement sur tous types de rapports : 
                            eRightReport oRightManager = new eRightReport(pref, TypeReport.ALLFORWIZARD);

                            Dictionary<eLibConst.CONFIG_DEFAULT, string> config = pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.ThresholdMailing, eLibConst.CONFIG_DEFAULT.ThresholdExport });

                            // Gestion des seuils et les droits sur les seuils, ça s'applique uniquement sur les exports [voir la doc technique : #24493] 
                            bool bExportThresholdExceeded = false;

                            int nExportThresholdValue = 0;

                            if (multiAddDel
                                && int.TryParse(config[eLibConst.CONFIG_DEFAULT.ThresholdExport].ToString(), out nExportThresholdValue))
                            {
                                bExportThresholdExceeded = nExportThresholdValue > 0
                                    && bkm.NbTotalRows > nExportThresholdValue
                                    && !oRightManager.HasRight(eLibConst.TREATID.THRESHOLD_REPORT);
                            }


                            // Vérification des droits de traitement
                            bool canExport = oRightManager.HasRight(eLibConst.TREATID.EXPORT);
                            bool canExportWithThreshold = canExport && !bExportThresholdExceeded; // avec le seuil
                            bool canMakePublipostage = oRightManager.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_HTML) ||
                                                          oRightManager.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_PDF) ||
                                                          oRightManager.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_WORD); // word/html

                            //Par défaut, on affiche le premier type de rapport dans la liste, auquel on a le droit
                            TypeReport reportType = canExportWithThreshold ? TypeReport.EXPORT :
                                canMakePublipostage ? TypeReport.MERGE :
                                oRightManager.HasRight(eLibConst.TREATID.GRAPHIQUE) ? TypeReport.CHARTS :
                                oRightManager.HasRight(eLibConst.TREATID.PRINT) ? TypeReport.PRINT : TypeReport.SPECIF;


                            //Si l'un des droits est octroyé, on affiche le button rapport pour afficher la liste  des rapports
                            if ((oRightManager.CanDisplayItemList() || canExportWithThreshold) ||
                                (canExport && bExportThresholdExceeded))
                            {
                                myLi = new HtmlGenericControl("li");

                                myLi.Attributes.Add("class", "icon-export icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 16));

                                if (oRightManager.CanDisplayItemList() || canExportWithThreshold)
                                {
                                    myLi.Attributes.Add("onclick",
                                        string.Concat("reportList(", reportType.GetHashCode(), ", ", bkm.CalledTabDescId, ");"));


                                    myImgDiv = new HtmlGenericControl("div");
                                }
                                else if (canExport && bExportThresholdExceeded)
                                {
                                    myLi.Attributes.Add("onclick", "eAlert(0,'Export','" + eResApp.GetRes(pref, 5034)
                                    .Replace("#NBFILE#", Convert.ToString(bkm.NbTotalRows))
                                    .Replace("#NBLIMIT#", Convert.ToString(nExportThresholdValue)) + "')");
                                }

                                tbButtonsBar.Controls.Add(myLi);
                            }

                            // CRU : Comme vu en revue de sprint le 07/01/2016, on rajoute le bouton de publipostage
                            if (canMakePublipostage)
                            {
                                #region Publipostage
                                myLi = new HtmlGenericControl("li");

                                myLi.Attributes.Add("class", "icon-word icnBkm");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 438)); // Publipostage

                                myLi.Attributes.Add("onclick", string.Concat("reportList(", TypeReport.MERGE.GetHashCode(), ", ", bkm.CalledTabDescId, ");"));

                                tbButtonsBar.Controls.Add(myLi);
                                #endregion

                            }
                        }

                        #endregion

                        if (viewMode == ePrefConst.BKMVIEWMODE.FILE)
                        {
                            myLi = new HtmlGenericControl("li");
                            myLi.Attributes.Add("class", "icon-signet-liste icnBkm");
                            myLi.Attributes.Add("onclick", "chgBkmViewMode(" + bkm.ParentTab + ", " + bkm.ViewMainTable.DescId.ToString() + ", 0);");
                            myLi.Attributes.Add("title", eResApp.GetRes(pref, 1145));
                            tbButtonsBar.Controls.Add(myLi);
                        }
                        else
                        {
                            //GMA 20140122
                            #region Emailing/Smsing ou formulaire depuis invitation/cible etendu
                            bool bPlusPlusOrExtendedTarget =
                                (bkm.ParentFile.ViewMainTable.TabType == TableType.EVENT)   //Depuis un EVENT
                                &&
                                (
                                    (bkm.ViewMainTable.ProspectEnabled) // Cible étendue
                                    ||
                                    (bkm.ViewMainTable.AdrJoin && bkm.ViewMainTable.InterEVT && bkm.ViewMainTable.InterPP) // Invitations
                                )
                                && bkm.ViewMainTable.EdnType != EdnType.FILE_MAIL;

                            if (bPlusPlusOrExtendedTarget)
                            {
                                //Liste des ids des droits de traitement
                                eLibConst.TREATID[] treatments =
                                    new eLibConst.TREATID[]
                                        {
                                            eLibConst.TREATID.FORMULAR,
                                            eLibConst.TREATID.EMAILING,
                                            eLibConst.TREATID.SMSING,
                                            eLibConst.TREATID.THRESHOLD_EMAILING,
                                            eLibConst.TREATID.THRESHOLD_REPORT
                                        };

                                IDictionary<eLibConst.TREATID, bool> globalRight = eLibDataTools.GetTreatmentGlobalRight(pref, treatments);

                                Dictionary<eLibConst.CONFIG_DEFAULT, string> config = pref.GetConfigDefault(
                                    new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.ThresholdMailing, eLibConst.CONFIG_DEFAULT.ThresholdExport });

                                #region Bouton Emailing mode signet
                                if (globalRight[eLibConst.TREATID.EMAILING])
                                {
                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", "icon-email icnBkm");
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 6391));

                                    bool resulexport;
                                    int nThresholdValueEmailing;

                                    resulexport = int.TryParse(config[eLibConst.CONFIG_DEFAULT.ThresholdMailing].ToString(), out nThresholdValueEmailing);


                                    if (nThresholdValueEmailing > 0 && bkm.NbTotalRows > nThresholdValueEmailing && !globalRight[eLibConst.TREATID.THRESHOLD_EMAILING])
                                    {
                                        myLi.Attributes.Add("onclick", "eAlert(0,'Emailing','" + eResApp.GetRes(pref, 5034).Replace("#NBFILE#", Convert.ToString(bkm.NbTotalRows)).Replace("#NBLIMIT#", Convert.ToString(nThresholdValueEmailing)) + "')");
                                    }
                                    else
                                    {
                                        string sTypeMailing = TypeMailing.MAILING_FROM_BKM.GetHashCode().ToString();

                                        myLi.Attributes.Add("onclick", string.Concat("AddMailing(", bkm.CalledTabDescId, ", ", sTypeMailing, ");"));

                                    }

                                    tbButtonsBar.Controls.Add(myLi);


                                }
                                #endregion

                                #region Bouton SMS


                                // Depuis eCircle ou eudonet
                                IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv = eLibTools.GetConfigAdvValues(pref,
                                    new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.SMS_SERVER_ENABLED
                                    });

                                //eLibConst.TREATID.SMSING
                                if (dicConfigAdv[eLibConst.CONFIGADV.SMS_SERVER_ENABLED] == "1" && globalRight[eLibConst.TREATID.SMSING])
                                {

                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", "bkmForm icnBkm icon-sms");
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 6857));
                                    myLi.Attributes.Add("onclick", string.Concat("AddSmsMailing(", bkm.CalledTabDescId, ", ", TypeMailing.SMS_MAILING_FROM_BKM.GetHashCode().ToString(), ");"));
                                    tbButtonsBar.Controls.Add(myLi);
                                }

                                #endregion

                                #region Bouton Formulaire

                                if (globalRight[eLibConst.TREATID.FORMULAR] && eConst.FORMULARENABLED)
                                {
                                    myLi = new HtmlGenericControl("li");
                                    myLi.Attributes.Add("class", "bkmForm icnBkm icon-form");
                                    myLi.Attributes.Add("title", eResApp.GetRes(pref, 6127));
                                    myLi.Attributes.Add("onclick", string.Concat("ShowFormularList(", bkm.CalledTabDescId, ", ", bkm.ParentFileId, ", 0, 0);"));

                                    tbButtonsBar.Controls.Add(myLi);

                                    //SHA : tâche #1 874
                                    if (eExtension.IsReady(pref, ExtensionCode.ADVANCED_FORM))
                                    {
                                        myLi = new HtmlGenericControl("li");
                                        myLi.Attributes.Add("class", "bkmForm icnBkm icon-list-alt");
                                        myLi.Attributes.Add("title", eResApp.GetRes(pref, 2449));
                                        myLi.Attributes.Add("onclick", string.Concat("ShowFormularList(", bkm.CalledTabDescId, ", ", bkm.ParentFileId, ", 0, 1);"));

                                        tbButtonsBar.Controls.Add(myLi);
                                    }
                                }
                                #endregion
                            }

                            #endregion

                            #region Passer en mode fiche incrustée
                            if (bkm.IsViewFilePossible)
                            {
                                myLi = new HtmlGenericControl("li");
                                myLi.Attributes.Add("class", "icon-signet-fiche icnBkm");
                                myLi.Attributes.Add("onclick", "chgBkmViewMode(" + bkm.ParentTab + ", " + bkm.ViewMainTable.DescId.ToString() + ", 1);");
                                myLi.Attributes.Add("title", eResApp.GetRes(pref, 6283));
                                tbButtonsBar.Controls.Add(myLi);

                                if (bkm.GetIdsList() != null)
                                {
                                    pnTitle.Attributes.Add("lstid", eLibTools.Join<int>(";", bkm.GetIdsList()));
                                    pnTitle.Attributes.Add("pg", bkm.Page.ToString());
                                    pnTitle.Attributes.Add("nbpg", bkm.NbPage.ToString());
                                    pnTitle.Attributes.Add("rpp", bkm.RowsByPage.ToString());
                                }
                            }

                            #endregion
                        }
                    }

                    if (bZoomButton)
                    {
                        HtmlGenericControl myLi = null;
                        myLi = new HtmlGenericControl("li");
                        myLi.Attributes.Add("class", "icon-search-plus StatsLoupePos");
                        myLi.Attributes.Add("title", eResApp.GetRes(pref, 714));

                        if (!string.IsNullOrEmpty(sDivId))
                            myLi.Attributes.Add("onclick", string.Concat("ZoomBookmark('", sDivId.Replace("'", @"\'"), "',", pW, ",", pH, ", this)"));

                        tbButtonsBar.Controls.Add(myLi);


                    }
                }
                #endregion

                return pnTitle;
            }
            catch (Exception e)
            {

#if DEBUG
                eModelTools.EudoTraceLog("*************");

                eModelTools.EudoTraceLog(e.Message);

                eModelTools.EudoTraceLog("*************");
#endif
                throw;

            }

        }
        /// <summary>
        /// Création d'un header pour les bookmark en erreurs
        /// </summary>
        /// <returns></returns>
        private void CreateErrorHead()
        {
            Panel pnTitle = new Panel();

            System.Web.UI.WebControls.Table tblTitleBar = new System.Web.UI.WebControls.Table();
            pnTitle.Controls.Add(tblTitleBar);
            tblTitleBar.Attributes.Add("width", "100%");

            TableRow tblRow = new TableRow();
            tblTitleBar.Controls.Add(tblRow);

            TableCell tcTitle = new TableCell();
            tcTitle.CssClass = "bkmLabel";
            tblRow.Controls.Add(tcTitle);

            pnTitle.CssClass = "bkmTitle";

            // Message d'erreur
            Literal liBkmTitle = new Literal();

            eRes tabRes = new eRes(Pref, _list.CalledTabDescId.ToString());
            string BkmLibelle = HttpUtility.HtmlEncode(tabRes.GetRes(_list.CalledTabDescId, ""));

            switch (_nErrorNumber)
            {
                case QueryErrorType.ERROR_NUM_BKM_NOT_LINKED:
                    liBkmTitle.Text = string.Concat(eResApp.GetRes(Pref, 6759).Replace("<BOOKMARK>", BkmLibelle));
                    break;
                case QueryErrorType.ERROR_NUM_FILTER_DBL_NOT_FOUND:
                    liBkmTitle.Text = string.Concat(eResApp.GetRes(Pref, 6764));
                    break;
                default:
                    string sErrMsg = eResApp.GetRes(Pref, 6432).Replace("<BKMRES>", BkmLibelle);
                    liBkmTitle.Text = string.Concat(sErrMsg, "-", eResApp.GetRes(Pref, 107));
                    break;
            }

            tcTitle.Controls.Add(liBkmTitle);



            _pgContainer.Controls.Add(pnTitle);

            Panel pnl = new Panel();

            Literal ltl = new Literal();
            pnl.CssClass = "noFileInBkm";

            ltl.Text = eResApp.GetRes(Pref, 83);
            pnl.Controls.Add(ltl);
            _pgContainer.Controls.Add(pnl);

        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

        }

        /// <summary>
        /// Ajoute l'entête de l'icone en debut de ligne
        /// </summary>
        /// <param name="headerRowicon"></param>
        protected override void AddIconHead(TableRow headerRowicon)
        {
            // TODO - FAIRE L'ICONE POUR PJ
            //if (_list.MainField == null || _list.MainField != null && _list.MainField.Table.EdnType != EudoQuery.EdnType.FILE_PJ)
            base.AddIconHead(headerRowicon);

            if (this._bkm.IsViewFilePossible)
            {
                TableHeaderCell selIcon2 = new TableHeaderCell();
                selIcon2.CssClass = "head icon";
                selIcon2.ID = string.Concat("HEAD_ICON_COL2_", this._tab);
                selIcon2.Attributes.Add("nomove", "1");
                selIcon2.Attributes.Add("width", string.Concat(_sizeTdIcon, "px"));
                headerRowicon.Cells.Add(selIcon2);
            }
        }

        /// <summary>
        /// Ajoute l'icone en debut de ligne
        /// Elle ne sera pas ajouter en cas de PJ
        /// </summary>
        /// <param name="rowIcon">eRecord </param>
        /// <param name="bodyRowicon">ligne à modifier</param>
        /// <param name="cssIcon">classe CCS origine</param>
        /// <param name="idxLine">Index de la ligne en cours de construction</param>
        protected override void AddIconBody(eRecord rowIcon, TableRow bodyRowicon, string cssIcon, int idxLine)
        {
            // TODO - FAIRE L'ICONE POUR PJ
            //if (_list.MainField == null || _list.MainField != null && _list.MainField.Table.EdnType != EudoQuery.EdnType.FILE_PJ)
            base.AddIconBody(rowIcon, bodyRowicon, cssIcon, idxLine);

            if (this._bkm.IsViewFilePossible)
            {
                TableCell selIcon2 = new TableCell();

                // TODO - ACTION ?
                selIcon2.Attributes.Add("eaction", "LNKOPENFILEINBKM");
                selIcon2.CssClass = "cell icon icon-signet-fiche icnBkm";
                selIcon2.ToolTip = eResApp.GetRes(Pref, 1122);
                bodyRowicon.Cells.Add(selIcon2);
                selIcon2.Attributes.Add("efld", "1");
            }
        }

        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, string sAltLineCss)
        {
            return;
        }

        /// <summary>
        /// identifie les paramètres de pagination
        /// </summary>
        protected override void SetPagingInfo()
        {
            //Information de paging
            _tblMainList.Attributes.Add("eNbCnt", eNumber.FormatNumber(Pref, _list.NbTotalRows, 0, true));
            _tblMainList.Attributes.Add("eNbTotal", eNumber.FormatNumber(Pref, _list.NbTotalRows, 0, true));
            _tblMainList.Attributes.Add("eHasCount", "1");

            //Compteur appararent
            if (_list.GetParam<bool>("PagingEnabled") || !_list.GetParam<bool>("CountOnDemand"))
            {
                _tblMainList.Attributes.Add("cnton", "1");
                _tblMainList.Attributes.Add("nbPage", _list.NbPage.ToString());
            }
            else
                _tblMainList.Attributes.Add("cnton", "0");

        }

        /// <summary>
        /// obtient la chaine de caractères représentant l'action javascript à exécuter pour 
        /// ajouter un enregistrement dans le signet.
        /// </summary>
        /// <returns></returns>
        internal static string GetJsAddFunction(ePref pref, eBookmark bkm)
        {
            string sAddJs = string.Empty;
            string modalSize = string.Empty;

            if (bkm.ViewMainTable.EdnType == EudoQuery.EdnType.FILE_PLANNING)
            {
                sAddJs = string.Concat("showTplPlanning(", bkm.ViewMainTable.DescId, ", 0, null, '", eResApp.GetRes(pref, 31), "')");
            }
            else if (bkm.ViewMainTable.TabType == EudoQuery.TableType.PJ)
            {
                if(pref.ThemeXRM.Version > 1)
                {
                    modalSize = string.Concat("{width: 650 , height: 550 }");
                }
                sAddJs = string.Concat("showAddPJ(undefined,undefined,",modalSize,")");
            }
            else if (bkm.ViewMainTable.EdnType == EudoQuery.EdnType.FILE_MAIN && bkm.ViewMainTable.AutoCreate)
            {
                //sAddJs = string.Concat("autoCreate(", bkm.ViewMainTable.DescId, ", new Array({tab: ", ((eBookmark)bkm).ParentTab, ", fid: ", ((eBookmark)bkm).ParentFileId, "}));");
                sAddJs = string.Format("autoCreate({0}, new Array({{tab: {1} , fid: {2}, spclnk: {3}}}));", bkm.ViewMainTable.DescId, bkm.ParentTab, bkm.ParentFileId, bkm.RelationFieldDescid);
            }
            else if (bkm.ViewMainTable.EdnType == EudoQuery.EdnType.FILE_MAIL)
            {
                sAddJs = string.Concat("sendMailTo('', ", bkm.ViewMainTable.DescId, ", '", bkm.ViewMainTable.Libelle.Replace("'", @"\'"), "', null, TypeMailing.MAILING_FROM_BKM)");
            }
            else if (bkm.ViewMainTable.EdnType == EudoQuery.EdnType.FILE_SMS)
            {
                sAddJs = string.Concat("sendMailTo('', ", bkm.ViewMainTable.DescId, ", '", bkm.ViewMainTable.Libelle.Replace("'", @"\'"), "', null, TypeMailing.SMS_MAILING_FROM_BKM)");
            }
            else if (bkm.ViewMainTable.TabType == EudoQuery.TableType.ADR
                    && bkm.ParentTab == EudoQuery.TableType.PM.GetHashCode()
                    )
            {
                //A priori, pas de raison pour que openLnk soit limitée à interPPNeeded
                //if (bkm.ViewMainTable.InterPPNeeded)
                sAddJs = string.Concat("openLnkFileDialog(", eFinderList.SearchType.Add.GetHashCode(), ", ", EudoQuery.TableType.PP.GetHashCode(), ",true);");
                //else
                //  sAddJs = string.Concat("shFileInPopup(", EudoQuery.TableType.PP.GetHashCode(), ", 0, '", bkm.ViewMainTable.Libelle.Replace("'", @"\'"), "', null, null, false, null, true, null, 4)");
            }
            else
            {
                string sBApplyCloseOnly = "false";
                if (bkm.ViewMainTable.EdnType == EudoQuery.EdnType.FILE_MAIN)
                    sBApplyCloseOnly = "true";
                sAddJs = string.Concat("shFileInPopup(", bkm.ViewMainTable.DescId, ", 0, '", bkm.ViewMainTable.Libelle.Replace("'", @"\'"), "', null, null, false, null, " + sBApplyCloseOnly + ", null, 4)");
            }


            return sAddJs;
        }


    }
}