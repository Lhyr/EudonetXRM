using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eBookmarkBarRenderer : eRenderer
    {
        eFile _ef;
        Boolean _bFileTabInBkm = false;

        private eBookmarkBarRenderer(ePref pref, eFile ef, Boolean bFileTabInBkm = false)
        {
            Pref = pref;
            _ef = ef;
            _bFileTabInBkm = bFileTabInBkm;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ef"></param>
        /// <param name="bFileTabInBkm"></param>
        /// <returns></returns>
        public static eBookmarkBarRenderer CreateBookmarkBarRenderer(ePref pref, eFile ef, Boolean bFileTabInBkm = false)
        {
            return new eBookmarkBarRenderer(pref, ef, bFileTabInBkm);
        }

        protected override bool Build()
        {
            #region On vérifie si le signet mis en focus est dans la sélection
            if (_ef.ActiveBkm > 0)
            {
                List<Int32> lstBkm = _ef.ListBkm.ConvertToListInt(";");

                if (!lstBkm.Contains(_ef.ActiveBkm))
                {
                    List<SetParam<ePrefConst.PREF_PREF>> _ePrefActivBkm = new List<SetParam<ePrefConst.PREF_PREF>>();
                    _ePrefActivBkm.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.ACTIVEBKM, ActiveBkm.DISPLAYFIRST.GetHashCode().ToString()));
                    Pref.SetPref(_ef.ViewMainTable.DescId, _ePrefActivBkm);

                    _ef.ActiveBkm = ActiveBkm.DISPLAYFIRST.GetHashCode();

                }
            }
            #endregion

            Boolean bDisplayAll = _ef.ActiveBkm == -1;

            this.PgContainer.ID = String.Concat("bkmBar_", _ef.ViewMainTable.DescId);
            this.PgContainer.CssClass = "bkmBar";

            int nfs = 11;
            if (!int.TryParse(Pref.FontSize, out nfs))
                nfs = 11;


            System.Drawing.Font font = new System.Drawing.Font("Verdana", nfs);
            //Taille de l'écran
            Int32 nMaxWidth = Pref.Context.FileWidth;
            if (nMaxWidth <= 0)
            {
                nMaxWidth = Pref.Context.ScreenWidth;
                //  Pour le calcul du paging des signets
                //taille du menu droite
                if (_ef.ViewMainTable.EdnType == EdnType.FILE_PLANNING)
                    nMaxWidth = nMaxWidth - eConst.RIGHT_MENUCAL_WIDTH;
                else
                    nMaxWidth = nMaxWidth - eConst.RIGHT_MENU_WIDTH;
            }
            //On doit retirer la taille du bouton "+"
            nMaxWidth -= eConst.BKM_PLUS_WIDTH;

            /* Entête des signets */

            /* Table Bkm*/
            System.Web.UI.WebControls.Table tabBkm = new System.Web.UI.WebControls.Table();
            this.PgContainer.Controls.Add(tabBkm);
            tabBkm.ID = String.Concat("bkmTab_", _ef.ViewMainTable.DescId);
            tabBkm.CssClass = "bkmBar";

            TableRow trBkm = new TableRow();
            tabBkm.Rows.Add(trBkm);
            trBkm.CssClass = "bkmTr";
            trBkm.ID = "bkmtr";

            TableCell tdBkmSep;
            Int32 nCurrentPage = 1;
            // La page active à laquelle appartient le signet actif (ef.ActiveBkm) #33264
            Int32 nActivePage = 1;
            IDictionary<Int32, IList<TableCell>> bkmPages = new Dictionary<Int32, IList<TableCell>>();
            IList<TableCell> bkmTds = new List<TableCell>();

            #region TD Details de la fiche

            if (
                !(_ef.FileId == 0 && _ef.ViewMainTable.TabType == TableType.PP)
                || _ef.Type == eFile.FType.AdminFile
                )
            {
                //Booleen indiquant si une partie des rubriques de la fiche principale a été déportée dans les signets
                if (_bFileTabInBkm)
                {
                    TableCell tdBkmDetails = new TableCell();
                    trBkm.Cells.Add(tdBkmDetails);

                    String sAdOnCss = String.Empty;
                    if (_ef.ActiveBkm == EudoQuery.ActiveBkm.DISPLAYFIRST.GetHashCode())
                        sAdOnCss = "Sel";

                    tdBkmDetails.CssClass = String.Concat("bkmDtls", sAdOnCss);
                    tdBkmDetails.ID = "bkmDtls";
                    tdBkmDetails.Attributes.Add("ednbkmpage", "0");

                    HyperLink htmlLinkDetails = new HyperLink();
                    htmlLinkDetails.Attributes.Add("class", "txtDetails");

                    Image imgBtn = new Image();
                    imgBtn.ImageUrl = eConst.GHOST_IMG;
                    imgBtn.CssClass = String.Concat("bkmDtlsImg", sAdOnCss);
                    htmlLinkDetails.Controls.Add(imgBtn);
                    htmlLinkDetails.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 860)));

                    if (_ef.Type != eFile.FType.AdminFile)
                    {
                        htmlLinkDetails.Attributes.Add("onclick", "javascript:swFileBkm(0)");
                    }
                    else
                    {
                        tdBkmDetails.Attributes.Add("ble", eLibConst.DESC.BREAKLINE.GetHashCode().ToString());
                    }

                    tdBkmDetails.Controls.Add(htmlLinkDetails);

                    //Taille du signet détails + padding
                    nMaxWidth = nMaxWidth - (eConst.BKM_DTLS_WIDTH + 2);
                }
            }
            #endregion


            #region  TD ALL
            if (!(_ef.FileId == 0 && _ef.ViewMainTable.TabType == TableType.PP) && _ef.Type != eFile.FType.AdminFile)
            {
                //NBA 28/02/2013 correction bug #21681 ne pas afficher "TOUS" lorsqu'on a aucun signet d'affiché.
                // Le signet grille ne sera pas affiché dans le signet Tous, donc on le compte pas
                if (_ef.LstBookmark != null && _ef.LstBookmark.Where(b => b.BkmEdnType != EdnType.FILE_GRID).Count() > 1 && _ef.CalledTabDescId != TableType.CAMPAIGN.GetHashCode())
                {
                    TableCell tdBkmAll = new TableCell();
                    trBkm.Cells.Add(tdBkmAll);
                    tdBkmAll.CssClass = String.Concat("bkmHead", _ef.ActiveBkm == -1 ? "Sel" : "");
                    tdBkmAll.ID = "bkmAll";
                    tdBkmAll.Attributes.Add("ednbkmpage", "0");

                    HyperLink htmlLinkAll = new HyperLink();
                    htmlLinkAll.Attributes.Add("class", "txtAll");
                    htmlLinkAll.Text = HttpUtility.HtmlEncode(eResApp.GetRes(Pref, 435).ToUpper());
                    htmlLinkAll.Attributes.Add("onclick", "loadBkmList(-1)");
                    // htmlLinkAll.NavigateUrl = String.Concat("javascript:loadBkmList(-1);");
                    tdBkmAll.Controls.Add(htmlLinkAll);


                    Int32 nSizeBkmAll = eTools.MesureString(htmlLinkAll.Text, font) + 2;
                    if (nSizeBkmAll < 90)
                        nSizeBkmAll = 90;

                    nMaxWidth -= nSizeBkmAll;

                    //Sep
                    tdBkmSep = new TableCell();
                    trBkm.Cells.Add(tdBkmSep);
                    tdBkmSep.CssClass = "bkmSep";
                    //pour que le séparateur s'affiche toujours en javascript
                    tdBkmSep.Attributes.Add("ednbkmpage", "0");
                }
            }
            #endregion


            int nCurrentSize = 0;

            bool bFirstLoaded = false;

            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);

            List<eBookmark> listBkm = _ef.LstBookmark ?? new List<eBookmark>();
            DescAdvDataSet listCountFilters = null;

            DESCADV_PARAMETER descAdvParam;
            switch (_ef.CalledTabDescId)
            {
                case (int)TableType.PP:
                    descAdvParam = DESCADV_PARAMETER.BKMCOUNTFILTER200;
                    break;
                case (int)TableType.PM:
                    descAdvParam = DESCADV_PARAMETER.BKMCOUNTFILTER300;
                    break;
                default:
                    descAdvParam = DESCADV_PARAMETER.BKMCOUNTFILTER100;
                    break;
            }

            #region Recherche des filtres pour afficher le compteur sur les signets
            if (listBkm != null && listBkm.Count > 0)
            {
                try
                {
                    eDal.OpenDatabase();

                    listCountFilters = new DescAdvDataSet();
                    listCountFilters.LoadAdvParams(eDal, listBkm.Select(b => b.CalledTabDescId), descAdvParam);
                }
                catch (Exception exc)
                {
                    // Recherche des filtres échouée
                    _sErrorMsg = String.Concat("Recherche des filtres échouée : ", exc.Message, " - ", exc.StackTrace);
                }
                finally
                {
                    if (eDal != null)
                        eDal.CloseDatabase();
                }
            }
            #endregion

            int bkmDescid;
            String fileIdField = String.Concat(_ef.ViewMainTable.ShortField, "ID");
            eBookmark bkmSelected = null;
            foreach (eBookmark bkm in listBkm)
            {
                bkmDescid = bkm.CalledTabDescId;

                if (bkm.ErrorMsg.Length > 0)
                    continue;

                //Ajout du bookmark à la barre de signet
                TableCell tdBkmLabel = new TableCell();
                trBkm.Cells.Add(tdBkmLabel);
                Boolean bBkmSelected = _ef.ActiveBkm == bkmDescid ||
                    (_ef.ActiveBkm == ActiveBkm.DISPLAYFIRST.GetHashCode() && !_bFileTabInBkm && !bFirstLoaded && !(bkm.HideWhenEmpty && bkm.IsEmpty));

                if (_ef.Type == eFile.FType.AdminFile)
                    bBkmSelected = false;

                if (bBkmSelected)
                {
                    tdBkmLabel.CssClass = "bkmHeadSel";
                    _ef.ActiveBkm = bkmDescid;
                    bFirstLoaded = true;
                    bkmSelected = bkm;
                }
                else
                {
                    tdBkmLabel.CssClass = "bkmHead";
                }

                StringBuilder sbBkmLabel = new StringBuilder(HttpUtility.HtmlEncode(bkm.Libelle));

                HyperLink htmlLink = new HyperLink();

                if (_ef.Type != eFile.FType.AdminFile)
                {
                    if (bkmDescid % 100 == AllField.ATTACHMENT.GetHashCode())
                    {
                        //Information sur le nombre de pièce jointe de la fiche
                        String err = "";
                        int nPj = _ef.Record?.PjCnt ?? eLibTools.PjListSelect(Pref, bkm.ParentTab, bkm.ParentFileId, null, out err).Count;
                        sbBkmLabel.Append(" <span id='bkmTabCnt_").Append("pj").Append("'>(")
                            .Append(nPj)
                            .Append(")</span>");
                    }
                    else
                    {
                        #region Calcul du nombre de fiches liées au filtre, s'il existe

                        if (listCountFilters != null)
                        {
                            int count = 0;
                            int filterId = eLibTools.GetNum(listCountFilters.GetAdvInfoValue(bkmDescid, descAdvParam));
                            if (filterId > 0)
                            {
                                String filterName = GetBkmFilter(eDal, bkm.CalledTabDescId, fileIdField, filterId, out count);
                                String formattedCount = eNumber.FormatNumber(Pref, count, 0, true);
                                //htmlLink.Attributes.Add("title", String.Concat(formattedCount, " ", filterName));
                                htmlLink.Attributes.Add("fltLbl", String.Concat(formattedCount, " ", filterName));
                                htmlLink.Attributes.Add("onmouseover", String.Concat("shFilterDescriptionById(event, ", filterId, ")"));
                                htmlLink.Attributes.Add("onmouseout", "hideFilterDescription()");
                                sbBkmLabel.Append(" <span id='bkmTabCnt_").Append(bkmDescid).Append("'>(")
                                    .Append(formattedCount).Append(")</span>");

                                // Moins performant
                                //wc = new WhereCustom(fileIdField, Operator.OP_EQUAL, _ef.FileId.ToString());
                                //list = eListFactory.CreateCustomList(Pref, bkmDescid, wc: wc, nFilterId: filterId);
                                //count = list.ListRecords.Count;
                            }
                        }
                        #endregion

                    }
                }
                htmlLink.Text = sbBkmLabel.ToString();

                // Signet Grille en utilisation                
                if (bkm.ViewMainTable != null && bkm.ViewMainTable.EdnType == EdnType.FILE_GRID)
                {
                    if (!Pref.AdminMode)
                        htmlLink.Attributes.Add("onclick", String.Concat("loadBkmGrid(", bkmDescid, ");"));

                    tdBkmLabel.Attributes.Add("edntype", ((int)EdnType.FILE_GRID).ToString());
                }
                else
                //MOU - cf. 31699 on n'autorise pas a cliqué sur le signet adresse de la popup  de creation de contact 
                if (!(_ef.FileId == 0 && _ef.ViewMainTable.TabType == TableType.PP))
                {
                    if (_ef.Type != eFile.FType.AdminFile)
                    {
                        //htmlLink.NavigateUrl = String.Concat("javascript:loadBkmList(", bkm.CalledTabDescId, ");");
                        htmlLink.Attributes.Add("onclick", String.Concat("loadBkmList(", bkmDescid, ");"));
                    }
                    else if (bkm.ViewMainTable != null && bkm.ViewMainTable.EdnType == EdnType.FILE_BKMWEB)
                    {
                        //  htmlLink.Attributes.Add("onclick", String.Concat("nsAdminBkmWeb.editBkmWebProperties(", bkm.CalledTabDescId, ");"));
                    }
                }


                tdBkmLabel.Controls.Add(htmlLink);
                // Calcul de la taille prise par un signet

                Int32 nSizeBkm = eTools.MesureString(htmlLink.Text.Replace("&nbsp;", " "), font);
                if (nSizeBkm < 90)
                    nSizeBkm = 90;

                nSizeBkm += 2;// Padding
                nSizeBkm += 2;// Espace nkm

                nCurrentSize += nSizeBkm;


                if (nCurrentSize > nMaxWidth)
                {
                    nCurrentPage++;
                    nCurrentSize = nSizeBkm;
                }

                tdBkmLabel.ID = String.Concat("BkmHead_", bkmDescid);

                if (bkm.RelationFieldDescid > 0)
                    tdBkmLabel.Attributes.Add("spclnk", bkm.RelationFieldDescid.ToString());

                TableCell tdBkmSepInner = new TableCell();
                trBkm.Cells.Add(tdBkmSepInner);
                tdBkmSepInner.CssClass = "bkmSep";

                tdBkmLabel.Attributes.Add("ednbkmpage", nCurrentPage.ToString());
                tdBkmSepInner.Attributes.Add("ednbkmpage", nCurrentPage.ToString());

                // Affichage de la puce correspondant au signet affiché #33264
                if (_ef.ActiveBkm == bkmDescid)
                    nActivePage = nCurrentPage;

                if (!bkmPages.TryGetValue(nCurrentPage, out bkmTds))
                {
                    bkmTds = new List<TableCell>();
                    bkmPages.Add(nCurrentPage, bkmTds);
                }
                bkmTds.Add(tdBkmLabel);
                bkmTds.Add(tdBkmSepInner);
                tdBkmLabel.Style.Add("display", "none");
                tdBkmSepInner.Style.Add("display", "none");

                //Engrenage pour l'admin
                if (_ef.Type == eFile.FType.AdminFile && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                {

                    tdBkmLabel.ToolTip += string.Concat("  ", bkmDescid);

                    tdBkmLabel.Attributes.Add("cellpos", tdBkmLabel.ID);

                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    tdBkmLabel.Controls.Add(ul);
                    ul.Attributes.Add("class", "fieldOptions");
                    ul.Attributes.Add("did", String.Concat("bkm", bkmDescid));
                    ul.Attributes.Add("edntype", ((int)bkm.BkmEdnType).ToString());

                    HtmlGenericControl li;
                    li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);
                    li.Attributes.Add("class", "icon-cog configOption");
                    li.Attributes.Add("title", eResApp.GetRes(Pref, 7817));


                    if (bkm.BkmEdnType == EdnType.FILE_BKMWEB || bkm.BkmEdnType == EdnType.FILE_GRID)
                    {
                        HtmlGenericControl liDel;
                        liDel = new HtmlGenericControl("li");
                        ul.Controls.Add(liDel);

                        liDel.Attributes.Add("onclick", "nsAdminBkmWeb.deleteBkm (" + bkm.ViewMainTable.DescId + ",event)");

                        liDel.Attributes.Add("class", "icon-delete deleteOption");
                    }
                }
            }

            //Affichage que des signets appartenant à la puce active #33264
            bkmPages.TryGetValue(nActivePage, out bkmTds);
            if (bkmTds != null)
                foreach (TableCell td in bkmTds)
                {
                    td.Style.Add("display", "");
                }

            if (!(_ef.FileId == 0 && _ef.ViewMainTable.TabType == TableType.PP) && _ef.Type != eFile.FType.AdminFile)
            {

                /* TD (+) */
                TableCell tdBkmPlus = new TableCell();
                trBkm.Cells.Add(tdBkmPlus);
                tdBkmPlus.Text = "+";
                tdBkmPlus.CssClass = "bkmHeadPlus";
                tdBkmPlus.Attributes.Add("ednbkmpage", "0");


                HyperLink htmlLinkPlus = new HyperLink();
                htmlLinkPlus.Text = "+";
                //htmlLinkPlus.NavigateUrl = String.Concat("javascript:setBkmOrder(", ef.ViewMainTable.DescId, ")");
                htmlLinkPlus.Attributes.Add("onclick", String.Concat("javascript:setBkmOrder(", _ef.ViewMainTable.DescId, ")"));
                tdBkmPlus.Controls.Add(htmlLinkPlus);
            }

            #region TD Vide 
            TableCell tdBkmEmpty = new TableCell();
            trBkm.Cells.Add(tdBkmEmpty);
            tdBkmEmpty.ID = "bkmHeadClean";
            tdBkmEmpty.Attributes.Add("class", "bkmHeadClean");
            tdBkmEmpty.Attributes.Add("ednbkmpage", "0");


            // Icône pour réduire la fiche
            HtmlGenericControl iconCollapse = new HtmlGenericControl();
            iconCollapse.ID = "opencloseall";
            if (_ef?.IsGlobalSepClosed ?? false)
            {
                // tout afficher
                iconCollapse.Attributes.Add("class", "icon-collapse");
                iconCollapse.Attributes.Add("title", (eResApp.GetRes(Pref, 6878)));
                iconCollapse.Attributes.Add("rmode", "1");

            }
            else
            {
                // tout masquer
                iconCollapse.Attributes.Add("class", "icon-uncollapse");
                iconCollapse.Attributes.Add("title", (eResApp.GetRes(Pref, 6291)));
                iconCollapse.Attributes.Add("rmode", "0");
            }
            //iconCollapse.Attributes.Add("onclick", "OpenCloseSepAll(document.getElementById('opencloseall'))");


            iconCollapse.Attributes.Add("onclick", "switchModeResume(this)");
            iconCollapse.Attributes.Add("tab", _ef.ViewMainTable.DescId.ToString());
            iconCollapse.Attributes.Add("fileid", _ef.FileId.ToString());

            if (_ef.FileId > 0  )
                tdBkmEmpty.Controls.Add(iconCollapse);


            #endregion

            if (nCurrentPage > 1 || Pref.AdminMode)
            {
                TableRow trPagin = new TableRow();
                tabBkm.Rows.AddAt(0, trPagin);
                trPagin.CssClass = "trBtnBkmPag";
                TableCell tcPagin = new TableCell();
                trPagin.Cells.Add(tcPagin);
                tcPagin.ColumnSpan = trBkm.Cells.Count;


                Panel btns = new Panel();
                tcPagin.Controls.Add(btns);
                btns.ID = "divBkmPaging";
                btns.CssClass = "divBkmPaging";
                btns.Attributes.Add("ble", eLibConst.DESC.BREAKLINE.GetHashCode().ToString());
                if (nCurrentPage > 1)
                {
                    for (int i = 1; i <= nCurrentPage; i++)
                    {
                        HtmlGenericControl imgBtn = new HtmlGenericControl();
                        btns.Controls.Add(imgBtn);
                        if (i == nActivePage)
                            imgBtn.Attributes.Add("class", "icon-circle imgAct");
                        else
                            imgBtn.Attributes.Add("class", "icon-circle-thin imgInact");

                        imgBtn.ID = String.Concat("swBkmPg", i);
                        imgBtn.Attributes.Add("onclick", String.Concat("switchActivePageBkm(", i, ")"));
                    }
                }
                else
                {

                    btns.Controls.Add(new LiteralControl("&nbsp;"));
                }
            }

            if (eDal != null)
                eDal.CloseDatabase();

            this.PgContainer.Attributes.Add("activebkm", _ef.ActiveBkm.ToString());
            //Viewmode pour le signet actif
            if (_ef.ActiveBkm > 0 && bkmSelected?.BkmPref != null)
                this.PgContainer.Attributes.Add("abkmvm", bkmSelected.BkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.VIEWMODE).ToString());

            return true;
        }

        /// <summary>
        /// Récupération du compteur et nom du filtre
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="nBkm"></param>
        /// <param name="idField"></param>
        /// <param name="filterId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private String GetBkmFilter(eudoDAL eDal, int nBkm, String idField, int filterId, out int count)
        {
            String filtername = string.Empty;
            DataTableReaderTuned dtr = null;
            count = 0;
            try
            {
                eDal.OpenDatabase();
                EudoQuery.EudoQuery eq = eLibTools.GetEudoQuery(Pref, nBkm, ViewQuery.CUSTOM);
                if (eq.GetError.Length != 0)
                    throw new Exception(String.Concat("eBookmarkBarRenderer.GetBkmFilter - GetEudoQuery : ", eq.GetError));

                eq.SetListCol = idField;
                eq.SetParentFileId = _ef.FileId;
                eq.SetParentDescid = _ef.CalledTabDescId;
                eq.SetFilterId = filterId;

                eq.LoadRequest();
                if (eq.GetError.Length != 0)
                    throw new Exception(String.Concat("eBookmarkBarRenderer.GetBkmFilter - eq.LoadRequest : ", eq.GetError));

                eq.BuildRequest();
                if (eq.GetError.Length != 0)
                    throw new Exception(String.Concat("eBookmarkBarRenderer.GetBkmFilter - eq.BuildRequest : ", eq.GetError));

                filtername = eq.GetParam("FilterName");

                dtr = eDal.Execute(new RqParam(eq.EqCountQuery), out _sErrorMsg);
                if (dtr.Read())
                {
                    count = dtr.GetInt32(0);
                }
            }
            catch (Exception)
            {
                // On n'empêche pas le chargement du signet si on n'a pas pu récupérer le compteur du filtre
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
                if (eDal != null)
                    eDal.CloseDatabase();
            }

            return filtername;

        }
    }
}